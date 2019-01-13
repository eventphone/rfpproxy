using System;
using System.Buffers.Binary;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RfpProxy
{
    public abstract class AbstractRfpProxy : TcpProxy<CryptedRfpConnection>
    {
        public AbstractRfpProxy(int listenPort, string ommHost, int ommPort) 
            : base(listenPort, ommHost, ommPort)
        {
        }

        protected override async Task ReadFromClientAsync(CryptedRfpConnection connection, PipeReader client, CancellationToken cancellationToken)
        {
            var sysInit = await ReadPacketAsync(0x0120, 0, client, cancellationToken).ConfigureAwait(false);
            await OnClientMessageAsync(connection, sysInit, cancellationToken);
            await ReadAsync(connection, client, OnClientMessageAsync, connection.RfpToOmmIv, connection.DecryptRfpToOmm, cancellationToken).ConfigureAwait(false);
        }

        protected override async Task ReadFromServerAsync(CryptedRfpConnection connection, PipeReader server, CancellationToken cancellationToken)
        {
            var sysAuthenticate = await ReadPacketAsync(0x012d, 0x20, server, cancellationToken).ConfigureAwait(false);
            connection.InitRfpToOmmIv(sysAuthenticate.Slice(11, 8).Span);

            await OnServerMessageAsync(connection, sysAuthenticate, cancellationToken);

            var ack = await ReadPacketAsync(0x01, 0x08, server, cancellationToken).ConfigureAwait(false);
            await OnServerMessageAsync(connection, ack, cancellationToken).ConfigureAwait(false);

            connection.InitOmmToRfpIv(sysAuthenticate.Slice(27, 8).Span);

            await ReadAsync(connection, server, OnServerMessageAsync, connection.OmmToRfpIv, connection.DecryptOmmToRfp, cancellationToken).ConfigureAwait(false);
        }

        private static async Task ReadAsync(CryptedRfpConnection connection, PipeReader reader, Func<RfpConnection, ReadOnlyMemory<byte>, CancellationToken, Task> messageCallback, ReadOnlyMemory<byte> iv, Func<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>, Memory<byte>> decrypt,  CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var result = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
                    var buffer = result.Buffer;

                    bool success;
                    do
                    {
                        success = false;
                        if (buffer.Length < 8)
                        {
                            break;
                        }
                        var block = buffer.Slice(0,8).ToMemory();
                        var plain = decrypt(block, iv);
                        var length = BinaryPrimitives.ReadUInt16BigEndian(plain.Slice(2, 2).Span);
                        if (length <= 4)
                        {
                            iv = block;
                            await messageCallback(connection, plain.Slice(0, length + 4), cancellationToken).ConfigureAwait(false);
                            buffer = buffer.Slice(8);
                            success = true;
                        }
                        else if (buffer.Length >= length + 4)
                        {
                            var cryptedLength = (length + 4 + 7) & ~7;//next multiple of 8
                            if (buffer.Length >= cryptedLength)
                            {
                                var data = buffer.Slice(0, cryptedLength).ToMemory();
                                var plaintext = decrypt(data, iv);
                                iv = data.Slice(data.Length - 8);
                                plaintext = plaintext.Slice(0, length + 4);
                                await messageCallback(connection, plaintext, cancellationToken).ConfigureAwait(false);
                                buffer = buffer.Slice(cryptedLength);
                                success = true;
                            }
                        }
                    } while (success && buffer.Length >= 8);
                    if (result.IsCompleted)
                        break;
                    reader.AdvanceTo(buffer.Start, buffer.End);
                }
                reader.Complete();
            }
            catch (OperationCanceledException ex)
            {
                reader.Complete(ex);
            }
        }

        private static async Task<ReadOnlyMemory<byte>> ReadPacketAsync(ushort type, ushort length, PipeReader reader, CancellationToken cancellationToken)
        {
            var typeArray = new byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(typeArray, type);

            while (!cancellationToken.IsCancellationRequested)
            {
                var result = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
                var buffer = result.Buffer;
                if (buffer.Length >= 4)
                {
                    if (buffer.Slice(0, 2).ToMemory().Span.SequenceEqual(typeArray))
                    {
                        //found message type
                        var packetLength = BinaryPrimitives.ReadUInt16BigEndian(buffer.Slice(2, 2).ToMemory().Span);
                        if (length == 0 || packetLength == length)
                        {
                            //length is correct
                            buffer = buffer.Slice(0, packetLength + 4);
                            reader.AdvanceTo(buffer.Start, buffer.End);
                            return buffer.ToMemory();
                        }
                        return Array.Empty<byte>();
                    }
                }
                if (result.IsCompleted)
                {
                    return Array.Empty<byte>();
                }
                reader.AdvanceTo(buffer.Start, buffer.End);
            }
            cancellationToken.ThrowIfCancellationRequested();
            return Array.Empty<byte>();
        }

        protected abstract Task OnClientMessageAsync(RfpConnection connection, ReadOnlyMemory<byte> data, CancellationToken cancellationToken);

        protected abstract Task OnServerMessageAsync(RfpConnection connection, ReadOnlyMemory<byte> data, CancellationToken cancellationToken);

        protected override CryptedRfpConnection OnClientConnected(TcpClient client, TcpClient server)
        {
            return new CryptedRfpConnection(client, server);
        }
    }
}