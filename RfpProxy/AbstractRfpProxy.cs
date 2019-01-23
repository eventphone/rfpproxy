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

            var iv = connection.RfpToOmmIv;
            connection.RfpToOmmIv = ReadOnlyMemory<byte>.Empty;
            await OnClientMessageAsync(connection, sysInit, cancellationToken).ConfigureAwait(false);
            connection.RfpToOmmIv = iv;
            await ReadAsync(connection, client, OnClientMessageAsync, connection.RfpToOmmIv, connection.DecryptRfpToOmm, connection.RekeyRfpToOmmDecrypt, cancellationToken).ConfigureAwait(false);
        }

        protected override async Task ReadFromServerAsync(CryptedRfpConnection connection, PipeReader server, CancellationToken cancellationToken)
        {
            var sysAuthenticate = await ReadPacketAsync(0x012d, 0x20, server, cancellationToken).ConfigureAwait(false);
            connection.InitRfpToOmmIv(sysAuthenticate.Slice(11, 8).Span);

            await OnServerMessageAsync(connection, sysAuthenticate, cancellationToken).ConfigureAwait(false);

            var ack = await ReadPacketAsync(0x01, 0x08, server, cancellationToken).ConfigureAwait(false);
            await OnServerMessageAsync(connection, ack, cancellationToken).ConfigureAwait(false);

            connection.InitOmmToRfpIv(sysAuthenticate.Slice(27, 8).Span);

            await ReadAsync(connection, server, OnServerMessageAsync, connection.OmmToRfpIv, connection.DecryptOmmToRfp, connection.RekeyOmmToRfpDecrypt, cancellationToken).ConfigureAwait(false);
        }

        private static async Task ReadAsync(CryptedRfpConnection connection, PipeReader reader, Func<RfpConnection, ReadOnlyMemory<byte>, CancellationToken, Task> messageCallback, ReadOnlyMemory<byte> iv, Func<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>, Memory<byte>> decrypt, Action<ReadOnlyMemory<byte>> rekey,  CancellationToken cancellationToken)
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
                        var block = buffer.Slice(0, 8).ToMemory();
                        var plain = decrypt(block, iv);
                        var length = BinaryPrimitives.ReadUInt16BigEndian(plain.Slice(2, 2).Span);
                        if (length > 4)
                        {
                            //decrypt remaining data
                            if (buffer.Length < length + 4) continue;
                            var cryptedLength = (length + 4 + 7) & ~7; //next multiple of 8
                            if (buffer.Length < cryptedLength) continue;

                            block = buffer.Slice(0, cryptedLength).ToMemory();
                            plain = decrypt(block, iv);
                        }

                        iv = block.Slice(block.Length - 8);
                        rekey(block);
                        await messageCallback(connection, plain.Slice(0, length + 4), cancellationToken).ConfigureAwait(false);
                        buffer = buffer.Slice(block.Length);
                        success = true;
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
                            reader.AdvanceTo(buffer.End, buffer.End);
                            return buffer.ToMemory();
                        }
                        throw new Exception("unexpected packet");
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