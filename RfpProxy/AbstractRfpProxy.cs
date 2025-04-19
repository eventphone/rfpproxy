using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RfpProxyLib;

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
            Console.WriteLine($"[{connection.TraceId}] SYS_INIT");
            var iv = connection.RfpToOmmIv;
            connection.RfpToOmmIv = ReadOnlyMemory<byte>.Empty;

            connection.Identifier = new RfpIdentifier(sysInit.Slice(12,6).ToArray());
            var key = await GetRfpKeyAsync(connection, cancellationToken).ConfigureAwait(false);
            SetKeys(key, connection);

            await OnClientMessageAsync(connection, sysInit, cancellationToken).ConfigureAwait(false);
            connection.RfpToOmmIv = iv;
            await ReadAsync(connection, client, OnClientMessageAsync, connection.RfpToOmmIv, connection.DecryptRfpToOmm, cancellationToken).ConfigureAwait(false);
        }

        protected override async Task ReadFromServerAsync(CryptedRfpConnection connection, PipeReader server, CancellationToken cancellationToken)
        {
            var sysAuthenticate = await ReadPacketAsync(0x012d, 0x20, server, cancellationToken).ConfigureAwait(false);
            Console.WriteLine($"[{connection.TraceId}] SYS_AUTHENTICATE");
            connection.InitRfpToOmmIv(sysAuthenticate.Slice(11, 8).Span);

            await OnServerMessageAsync(connection, sysAuthenticate, cancellationToken).ConfigureAwait(false);

            var packet = await ReadPacketAsync(0, 0, server, cancellationToken).ConfigureAwait(false);
            var type = packet.Slice(0, 2);
            if (type.Span[0] == 0x01)
            {
                ReadOnlyMemory<byte> key;
                if (type.Span[1] == 0x24)
                {
                    //SYS_RFP_AUTH_KEY
                    Console.WriteLine($"[{connection.TraceId}] SYS_RFP_AUTH_KEY");
                    key = packet.Slice(4);
                }
                else if (type.Span[1] == 0x25)
                {
                    //SYS_RFP_RE_ENROLEMENT
                    Console.WriteLine($"[{connection.TraceId}] SYS_RFP_RE_ENROLEMENT");
                    var checksum = packet.Slice(0x44);
                    var crypted = packet.Slice(4, 0x40);
                    var pw = await GetRootPasswordHashAsync(cancellationToken).ConfigureAwait(false);
                    var pwBytes = Encoding.ASCII.GetBytes(pw);

                    using (var sha = SHA256.Create())
                    {
                        var data = new byte[0x20 + 0x1a + 0x40].AsMemory();
                        sysAuthenticate.Slice(4).CopyTo(data);
                        pwBytes.CopyTo(data.Slice(0x20));
                        crypted.CopyTo(data.Slice(0x20 + 0x1a));
                        var hash = new byte[0x20];
                        if (!sha.TryComputeHash(data.Span, hash, out _))
                            throw new InvalidOperationException();
                        if (!checksum.Span.SequenceEqual(hash))
                            throw new InvalidOperationException();
                    }
                    var aesKey = new byte[0x20];
                    sysAuthenticate.Slice(4).CopyTo(aesKey);
                    pwBytes.CopyTo(aesKey.AsSpan());
                    using (var aes = Aes.Create())
                    {
                        aes.Key = aesKey; 
                        aes.Mode = CipherMode.ECB;
                        aes.Padding = PaddingMode.None;
                        using (var decryptor = aes.CreateDecryptor())
                        {
                            var buffer = crypted.ToArray();
                            key = decryptor.TransformFinalBlock(buffer, 0, buffer.Length);
                        } 
                    }
                }
                else
                {
                    throw new InvalidOperationException();
                }
                SetKeys(key, connection);
                await OnServerMessageAsync(connection, packet, cancellationToken).ConfigureAwait(false);
                packet = await ReadPacketAsync(0x01, 0x08, server, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                Console.WriteLine($"[{connection.TraceId}] PACKET 0x{type.Span[0]:X2}{type.Span[1]:X2}");
            }
            var ack = packet;
            await OnServerMessageAsync(connection, ack, cancellationToken).ConfigureAwait(false);
            //check if we have another unencrypted SYS_OMM_CONTROL
            //this may happen if the RFP requires a firmware update
            if (server.TryRead(out var available)){
                server.CancelPendingRead();
                var buffer = available.Buffer;
                if (buffer.Length > 2 && buffer.FirstSpan[0] == 0x01 && buffer.Slice(1).FirstSpan[0] == 0x0c)
                {
                    Console.WriteLine($"[{connection.TraceId}] found SYS_OMM_CONTROL");
                    packet = await ReadPacketAsync(0x010c, 0x08, server, cancellationToken).ConfigureAwait(false);
                    Console.WriteLine($"[{connection.TraceId}] SYS_OMM_CONTROL");
                    await OnServerMessageAsync(connection, packet, cancellationToken).ConfigureAwait(false);
                }
            }

            connection.InitOmmToRfpIv(sysAuthenticate.Slice(27, 8).Span);

            await ReadAsync(connection, server, OnServerMessageAsync, connection.OmmToRfpIv, connection.DecryptOmmToRfp, cancellationToken).ConfigureAwait(false);
        }

        private static async Task ReadAsync(CryptedRfpConnection connection, PipeReader reader, Func<RfpConnection, ReadOnlyMemory<byte>, CancellationToken, Task> messageCallback, ReadOnlyMemory<byte> iv, Func<ReadOnlyMemory<byte>, ReadOnlyMemory<byte>, Memory<byte>> decrypt, CancellationToken cancellationToken)
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

                        iv = block.Slice(block.Length - 8).ToArray();
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
                Console.WriteLine($"[{connection.TraceId}] cancelled in AbstractRfpProxy.ReadAsync");
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
                    if (type == 0 || buffer.Slice(0, 2).ToMemory().Span.SequenceEqual(typeArray))
                    {
                        //found message type
                        var packetLength = BinaryPrimitives.ReadUInt16BigEndian(buffer.Slice(2, 2).ToMemory().Span);
                        if (length == 0 || packetLength == length)
                        {
                            //length is correct
                            buffer = buffer.Slice(0, packetLength + 4);
                            var copy = buffer.ToArray();
                            reader.AdvanceTo(buffer.End, buffer.End);
                            return copy;
                        }
                    }
                    throw new Exception("unexpected packet");
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

        protected virtual void SetKeys(ReadOnlyMemory<byte> key, CryptedRfpConnection connection)
        {
            if (key.IsEmpty) return;
            connection.SetOmmKey(key.Span.Slice(0, 56));
            connection.SetRfpKey(key.Span.Slice(8));
        }

        private async Task<ReadOnlyMemory<byte>> GetRfpKeyAsync(CryptedRfpConnection connection, CancellationToken cancellationToken)
        {
            var rfpa = await GetRfpaAsync(connection, cancellationToken).ConfigureAwait(false);
            if (rfpa.IsEmpty) return rfpa;
            HexEncoding.SwapEndianess(rfpa.Span);
            var key = connection.Identifier.ToString() + '\0';
            var bf = new BlowFish(Encoding.ASCII.GetBytes(key));
            var plain = bf.Decrypt_ECB(rfpa.Span);
            HexEncoding.SwapEndianess(plain.Span);
            return plain;
        }

        protected abstract Task<Memory<byte>> GetRfpaAsync(CryptedRfpConnection connection, CancellationToken cancellationToken);

        protected abstract Task<string> GetRootPasswordHashAsync(CancellationToken cancellationToken);

        protected override CryptedRfpConnection OnClientConnected(TcpClient client, TcpClient server)
        {
            return new CryptedRfpConnection(client, server);
        }
    }
}