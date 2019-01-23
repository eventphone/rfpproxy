using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

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

        public Task RunAsync(CancellationToken cancellationToken)
        {
            var pipe = new Pipe();
            var fillPipe = PipeHelper.FillPipeAsync(Socket, pipe.Writer, cancellationToken);
            var readPipe = ReadPipeAsync(pipe.Reader, cancellationToken);
            return Task.WhenAny(fillPipe, readPipe);
        }

        private async Task ReadPipeAsync(PipeReader client, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var result = await client.ReadAsync(cancellationToken).ConfigureAwait(false);
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
                                await _messageCallback(message, cancellationToken).ConfigureAwait(false);
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
    }
}