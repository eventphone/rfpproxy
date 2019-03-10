using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RfpProxyLib;
using RfpProxyLib.Messages;

namespace RfpProxy
{
    public class ClientConnection
    {
        private readonly Func<OmmMessage, CancellationToken, Task> _messageCallback;
        private readonly SemaphoreSlim _writeLock = new SemaphoreSlim(1, 1);
        private readonly ConcurrentDictionary<uint, OmmMessage> _pending = new ConcurrentDictionary<uint, OmmMessage>();

        public ClientConnection(Socket socket, Func<OmmMessage, CancellationToken, Task> messageCallback)
        {
            _messageCallback = messageCallback;
            Socket = socket;
        }

        public Socket Socket { get; }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            var pipe = new Pipe();
            var fillPipe = PipeHelper.FillPipeAsync(Socket, pipe.Writer, cancellationToken);
            var handshake = HandshakeAsync(pipe.Reader, cancellationToken);
            await Task.WhenAny(fillPipe, handshake).ConfigureAwait(false);
            var readPipe = ReadPipeAsync(pipe.Reader, cancellationToken);
            await Task.WhenAny(fillPipe, readPipe).ConfigureAwait(false);
        }

        private async Task HandshakeAsync(PipeReader client, CancellationToken cancellationToken)
        {
            bool finished = false;
            try
            {
                await SendAsync(Socket, Serialize(new Hello("stay connected")), cancellationToken).ConfigureAwait(false);
                while (!cancellationToken.IsCancellationRequested && !finished)
                {
                    var result = await client.ReadAsync(cancellationToken).ConfigureAwait(false);
                    var buffer = result.Buffer;
                    bool success;
                    do
                    {
                        var eol = buffer.PositionOf((byte) 0x0a);
                        success = eol.HasValue;
                        if (eol.HasValue)
                        {
                            var bytes = buffer.Slice(0, eol.Value).ToMemory();
                            var line = Encoding.UTF8.GetString(bytes.Span);
                            Console.WriteLine($"new subscription: {line}");
                            cancellationToken.ThrowIfCancellationRequested();
                            var msg = Deserialize(line);
                            buffer = buffer.Slice(eol.Value).Slice(1);
                            if (msg.Type == SubscriptionType.End)
                            {
                                finished = true;
                                break;
                            }
                            OnSubscribed(msg);
                        }
                    } while (success && buffer.Length > 2);
                    if (result.IsCompleted)
                        break;
                    client.AdvanceTo(buffer.Start, buffer.End);
                }
                await SendAsync(Socket, Serialize(new Hello("switching protocols")), cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException ex)
            {
                client.Complete(ex);
            }
            catch (Exception ex) when (Socket.Connected)
            {
                await SendAsync(Socket, ex.Message, cancellationToken).ConfigureAwait(false);
                Console.WriteLine(ex);
                return;
            }
        }

        public event EventHandler<SubscriptionEventArgs> Subscribed; 

        private async Task ReadPipeAsync(PipeReader client, CancellationToken cancellationToken)
        {
            try
            {
                var tasks = new List<Task>();
                while (!cancellationToken.IsCancellationRequested)
                {
                    var read = client.ReadAsync(cancellationToken);
                    ReadResult result;
                    if (read.IsCompleted)
                    {
                        result = await read;
                    }
                    else
                    {
                        var readTask = read.AsTask();
                        tasks.Add(readTask);
                        while (!read.IsCompleted)
                        {
                            var completed = await Task.WhenAny(tasks).ConfigureAwait(false);
                            if (!read.IsCompleted)
                            {
                                tasks.Remove(completed);
                                await completed.ConfigureAwait(false);
                            }
                        }
                        tasks.Remove(readTask);
                        result = await readTask.ConfigureAwait(false);
                    }
                    var buffer = result.Buffer;

                    bool success;
                    do
                    {
                        success = false;
                        if (buffer.Length < 4)
                            break;
                        var length = BinaryPrimitives.ReadUInt32BigEndian(buffer.Slice(0, 4).ToMemory().Span);
                        if (buffer.Length >= length + 4)
                        {
                            var data = buffer.Slice(4, length).ToMemory();
                            var message = new OmmMessage(data);
                            if (message.IsReply)
                            {
                                //dispatch reply
                                if (!_pending.TryGetValue(message.MessageId, out var request))
                                {
                                    throw new InvalidDataException("unexpected reply");
                                }
                                request.Reply = message;
                            }
                            else
                            {
                                tasks.Add(_messageCallback(message, cancellationToken));
                            }
                            buffer = buffer.Slice(4).Slice(length);
                            success = true;
                        }
                    } while (success && buffer.Length >= 4);
                    if (result.IsCompleted)
                        break;
                    client.AdvanceTo(buffer.Start, buffer.End);
                }
                client.Complete();
            }
            catch (OperationCanceledException ex)
            {
                client.Complete(ex);
            }
        }
        
        private static string Serialize<T>(T message)
        {
            return JsonConvert.SerializeObject(message);
        }

        private static Subscribe Deserialize(string message)
        {
            return JsonConvert.DeserializeObject<Subscribe>(message);
        }

        private static Task SendAsync(Socket client, string message, CancellationToken cancellationToken)
        {
            var data = Encoding.UTF8.GetBytes(message + "\n");
            return SendAsync(client, data, cancellationToken);
        }

        private static async Task SendAsync(Socket client, ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
        {
            await client.SendAsync(data, SocketFlags.None, cancellationToken).ConfigureAwait(false);
        }

        public void Send(OmmMessage message, CancellationToken cancellationToken)
        {
            _ = SendInternalAsync(message, cancellationToken);
        }

        private async Task SendInternalAsync(OmmMessage message, CancellationToken cancellationToken)
        {
            await _writeLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await message.SendAsync(Socket, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _writeLock.Release();
            }
        }

        public async Task<OmmMessage> HandleAsync(OmmMessage message, CancellationToken cancellationToken)
        {
            if (!_pending.TryAdd(message.MessageId, message))
                throw new ArgumentException("message id must be unique", nameof(message));
            await SendInternalAsync(message, cancellationToken).ConfigureAwait(false);
            var result = await message.WaitForReplyAsync(cancellationToken).ConfigureAwait(false);
            _pending.TryRemove(message.MessageId, out _);
            return result;
        }

        protected virtual void OnSubscribed(Subscribe subscribe)
        {
            Subscribed?.Invoke(this, new SubscriptionEventArgs(subscribe));
        }
    }

    public class SubscriptionEventArgs : EventArgs
    {
        public Subscribe Subscription { get; }

        public SubscriptionEventArgs(Subscribe subscription)
        {
            Subscription = subscription;
        }
    }
}