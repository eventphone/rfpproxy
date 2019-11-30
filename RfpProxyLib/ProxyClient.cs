using System;
using System.Buffers.Binary;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RfpProxyLib.Messages;

namespace RfpProxyLib
{
    public abstract class ProxyClient:IDisposable
    {
        private readonly Socket _socket;
        private readonly string _socketPath;
        private readonly SemaphoreSlim _readLock = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _writeLock = new SemaphoreSlim(1, 1);

        public ProxyClient(string socket)
        {
            _socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
            _socketPath = socket;
        }

        public Task AddListenAsync(string mac, string macMask, string filter, string filterMask, CancellationToken cancellationToken)
        {
            var subscription = new Subscribe
            {
                Type = SubscriptionType.Listen
            };
            return AddSubscriptionAsync(subscription, mac, macMask, filter, filterMask, cancellationToken);
        }

        public Task AddHandlerAsync(byte priority, string mac, string macMask, string filter, string filterMask, CancellationToken cancellationToken)
        {
            var subscription = new Subscribe
            {
                Type = SubscriptionType.Handle,
                Priority = priority
            };
            return AddSubscriptionAsync(subscription, mac, macMask, filter, filterMask, cancellationToken);
        }

        public async Task FinishHandshakeAsync(CancellationToken cancellationToken)
        {
            await InitAsync(cancellationToken).ConfigureAwait(false);
            if (_finished)
                return;
            var eos = new Subscribe
            {
                Type = SubscriptionType.End
            };
            await _readLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (_finished)
                    return;
                _finished = true;
                using (var stream = new NetworkStream(_socket, false))
                using (var writer = new StreamWriter(stream))
                using (var reader = new StreamReader(stream))
                {
                    var msg = JsonConvert.SerializeObject(eos);
                    await writer.WriteLineAsync(msg).ConfigureAwait(false);
                    LogWritten(msg);
                    cancellationToken.ThrowIfCancellationRequested();
                    await writer.FlushAsync().ConfigureAwait(false);
                    cancellationToken.ThrowIfCancellationRequested();
                    msg = await reader.ReadLineAsync().ConfigureAwait(false);
                    LogRead(msg);
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
            finally
            {
                _readLock.Release();
            }
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            await FinishHandshakeAsync(cancellationToken).ConfigureAwait(false);
            await _readLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var length = new byte[4];
                while (!cancellationToken.IsCancellationRequested)
                {
                    var success = await FillBufferAsync(_socket, length, cancellationToken).ConfigureAwait(false);
                    if (!success) return;

                    var msgLength = BinaryPrimitives.ReadUInt32BigEndian(length);
                    var msg = new byte[msgLength];

                    success = await FillBufferAsync(_socket, msg, cancellationToken).ConfigureAwait(false);
                    if (!success) return;

                    await OnMessageAsync(msg, cancellationToken).ConfigureAwait(false);
                }
            }
            finally
            {
                _readLock.Release();
            }
        }

        public virtual async Task WriteAsync(MessageDirection direction, uint messageId, RfpIdentifier rfp, ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
        {
            var header = new byte[4 + 1 + 4 + RfpIdentifier.Length];
            BinaryPrimitives.WriteUInt32BigEndian(header, (uint) (1+4+RfpIdentifier.Length + data.Length));
            header[4] = (byte) direction;
            BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(5), messageId);
            rfp.CopyTo(header.AsSpan(4 + 1 + 4));
            await _socket.SendAsync(header, SocketFlags.None, cancellationToken).ConfigureAwait(false);
            await _socket.SendAsync(data, SocketFlags.None, cancellationToken).ConfigureAwait(false);
        }

        public void Stop()
        {
            _socket.Close();
        }

        private Task OnMessageAsync(byte[] message, CancellationToken cancellationToken)
        {
            var direction = (MessageDirection) message[0];
            var messageId = BinaryPrimitives.ReadUInt32BigEndian(message.AsSpan(1));
            var rfp = new RfpIdentifier(message.AsMemory(5, 6));
            return OnMessageAsync(direction, messageId, rfp, message.AsMemory(5).Slice(RfpIdentifier.Length), cancellationToken);
        }

        protected abstract Task OnMessageAsync(MessageDirection direction, uint messageId, RfpIdentifier rfp, Memory<byte> data, CancellationToken cancellationToken);

        private static async Task<bool> FillBufferAsync(Socket socket, Memory<byte> buffer, CancellationToken cancellationToken)
        {
            while (buffer.Length > 0)
            {
                var bytesRead = await socket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken).ConfigureAwait(false);
                if (bytesRead == 0) return false;
                buffer = buffer.Slice(bytesRead);
            }
            return true;
        }

        private bool _initialized = false;
        private bool _finished = false;

        private async Task InitAsync(CancellationToken cancellationToken)
        {
            if (_initialized)
                return;
            await _readLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (_initialized)
                    return;
                await _socket.ConnectAsync(new UnixDomainSocketEndPoint(_socketPath)).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                _initialized = true;
                await _writeLock.WaitAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    using (var stream = new NetworkStream(_socket, false))
                    using (var reader = new StreamReader(stream))
                    {
                        var init = await reader.ReadLineAsync().ConfigureAwait(false);
                        cancellationToken.ThrowIfCancellationRequested();
                        LogRead(init);
                    }
                }
                finally
                {
                    _writeLock.Release();
                }
            }
            finally
            {
                _readLock.Release();
            }
        }

        private async Task AddSubscriptionAsync(Subscribe subscription, string mac, string macMask, string filter,
            string filterMask, CancellationToken cancellationToken)
        {
            subscription.Rfp = new SubscriptionFilter
            {
                Filter = mac.Replace(" ", String.Empty),
                Mask = macMask.Replace(" ", String.Empty)
            };
            subscription.Message = new SubscriptionFilter
            {
                Filter = filter.Replace(" ", String.Empty),
                Mask = filterMask.Replace(" ", String.Empty)
            };
            await InitAsync(cancellationToken).ConfigureAwait(false);
            var msg = JsonConvert.SerializeObject(subscription);
            using (var stream = new NetworkStream(_socket, false))
            using (var writer = new StreamWriter(stream))
            {
                await writer.WriteLineAsync(msg).ConfigureAwait(false);
                LogWritten(msg);
                cancellationToken.ThrowIfCancellationRequested();
                await writer.FlushAsync().ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        public event EventHandler<LogEventArgs> Log;

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _socket.Dispose();
                _readLock.Dispose();
                _writeLock.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void LogRead(string message)
        {
            Log?.Invoke(this, new LogEventArgs(LogDirection.Read, message));
        }

        private void LogWritten(string message)
        {
            Log?.Invoke(this, new LogEventArgs(LogDirection.Written, message));
        }
    }
}