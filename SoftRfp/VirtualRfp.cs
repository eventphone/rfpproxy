using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.Pipelines;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RfpProxy.AaMiDe;
using RfpProxy.AaMiDe.Sys;
using RfpProxyLib;
using RfpCapabilities = RfpProxy.AaMiDe.Sys.SysInitMessage.RfpCapabilities;

namespace RfpProxy.Virtual
{
    partial class VirtualRfp
    {
        private readonly string _mac;
        private readonly string _omm;
        private ReadOnlyMemory<byte> _rxIv;
        private ReadOnlyMemory<byte> _txIv;
        private ReadOnlyMemory<byte> _rfpa;
        private ReadOnlyMemory<byte> _auth;
        private BlowFish _decipher;
        private BlowFish _encipher;
        private Pipe _pipe;
        private Socket _socket;
        private readonly RfpConnectionTracker _connectionTracker;
        
        public string RFPA
        {
            get => HexEncoding.ByteToHex(_rfpa.Span);
            set => _rfpa = value is null ? ReadOnlyMemory<byte>.Empty : HexEncoding.HexToByte(value);
        }

        public string OmmConfPath { get; set; }

        public bool Debug { get; set; }

        public VirtualRfp(string mac, string omm)
        {
            _mac = mac.ToUpperInvariant();
            _omm = omm;
            _pipe = new Pipe();
            _connectionTracker = new RfpConnectionTracker(new RfpIdentifier(HexEncoding.HexToByte(mac)));
            _heartbeatTimer = new Timer(SendHeartbeat, null, -1, Timeout.Infinite);
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            using var connection = new TcpClient(AddressFamily.InterNetworkV6);
            using var registration = cancellationToken.Register(() => connection.Close());
            await connection.ConnectAsync(_omm, 16321);
            _socket = connection.Client;
            _auth = await ReadPacketAsync(cancellationToken);
            if (!await InitAsync(cancellationToken)) return;
            await StartEncryptionAsync(cancellationToken);
            await Task.WhenAny(FillPipeAsync(cancellationToken), ReadMessagesAsync(cancellationToken));
        }

        public event EventHandler<AaMiDeMessageEventArgs> OnMessage;

        public void SendMessage(AaMiDeMessage message)
        {
            var data = new byte[message.Length];
            message.Serialize(data);
            if (Debug)
            {
                Console.WriteLine($"> {HexEncoding.ByteToHex(data)}");
            }
            var crypted = _encipher.Encrypt_CBC(_txIv.Span, data);
            _txIv = crypted.Slice(crypted.Length - 8);
            _socket.Send(crypted.Span);
        }

        private async Task<bool> InitAsync(CancellationToken cancellationToken)
        {
            var caps = RfpCapabilities.Indoor;
            var init = new SysInitMessage(PhysicalAddress.Parse(_mac), caps);
            init.Sign(_auth.Span);
            await SendPacketAsync(init, cancellationToken);
            var ack = await ReadPacketAsync(cancellationToken);
            if (ack.IsEmpty) return false;
            if (ack.Span[0] == 0x01)
            {
                if (ack.Span[1] == 0x24)
                {
                    //SYS_RFP_AUTH_KEY
                    _rfpa = ack.Slice(4);
                    Console.WriteLine($"new RFPA: {HexEncoding.ByteToHex(_rfpa.Span)}");
                }
                else if (ack.Span[1] == 0x25)
                {
                    //SYS_RFP_RE_ENROLEMENT
                    Console.WriteLine("RFP_RE_ENROLMENT is not yet supported (because we would need to store the root password hash).\n" +
                                      "Please delete the RFP and reconnect to get a new key.");
                    return false;
                }
                else
                {
                    throw new NotImplementedException($"Unexpected packet 0x{BinaryPrimitives.ReadUInt16BigEndian(ack.Span):x4}");
                }
                ack = await ReadPacketAsync(cancellationToken);
                if (ack.IsEmpty) return false;
            }
            return true;
        }

        private async Task StartEncryptionAsync(CancellationToken cancellationToken)
        {
            if (_rfpa.IsEmpty)
            {
                if (!String.IsNullOrEmpty(OmmConfPath) && File.Exists(OmmConfPath))
                {
                    var crypted = await LoadRfpaAsync(cancellationToken);
                    if (!String.IsNullOrEmpty(crypted))
                    {
                        _rfpa = DecryptRfpa(crypted);
                    }
                }
                if (_rfpa.IsEmpty)
                    throw new Exception("Key missing. Can't start encryption");
            }
            _decipher = new BlowFish(_rfpa.Span.Slice(0, 56));
            _encipher = new BlowFish(_rfpa.Span.Slice(8));
            var txIv = HexEncoding.HexToByte("68e8364be9c234c1");
            BlowFish.XorBlock(txIv, _auth.Span.Slice(11, 8));
            _txIv = txIv;
            var rxIv = HexEncoding.HexToByte("dfe66571fac45a42");
            BlowFish.XorBlock(rxIv, _auth.Span.Slice(27, 8));
            _rxIv = rxIv;
            SendMessage(new SysEncryptionConf());
        }

        private async Task<string> LoadRfpaAsync(CancellationToken cancellationToken)
        {
            using (var reader = new OmmConfReader(OmmConfPath))
            {
                var rfp = await reader.GetValueAsync("RFP", "mac", _mac, cancellationToken);
                if (rfp != null)
                {
                    var id = rfp["id"];
                    var rfpa = await reader.GetValueAsync("RFPA", "id", id, cancellationToken);
                    if (rfpa != null)
                    {
                        return rfpa[1];
                    }
                }
                return null;
            }
        }

        private ReadOnlyMemory<byte> DecryptRfpa(string rfpa)
        {
            return DecryptRfpa(rfpa, _mac);
        }

        public static ReadOnlyMemory<byte> DecryptRfpa(string rfpa, string mac)
        {
            var bytes = HexEncoding.HexToByte(rfpa);
            HexEncoding.SwapEndianess(bytes);
            var key = (mac + '\0').ToLowerInvariant();
            var bf = new BlowFish(Encoding.ASCII.GetBytes(key));
            var plain = bf.Decrypt_ECB(bytes);
            HexEncoding.SwapEndianess(plain.Span);
            return plain;
        }

        private async Task<ReadOnlyMemory<byte>> ReadPacketAsync(CancellationToken cancellationToken)
        {
            var header = new byte[4];
            var read = await _socket.ReceiveAsync(header, SocketFlags.None, cancellationToken);
            if (read == 0)
            {
                return Memory<byte>.Empty;
            }
            var length = BinaryPrimitives.ReadUInt16BigEndian(header.AsSpan().Slice(2));
            var packet = new byte[length + 4];
            header.CopyTo(packet.AsSpan());
            read = await _socket.ReceiveAsync(packet.AsMemory(4), SocketFlags.None, cancellationToken);
            return packet.AsMemory(0, read + 4);
        }

        private async Task SendPacketAsync(AaMiDeMessage packet, CancellationToken cancellationToken)
        {
            var data = new byte[packet.Length];
            packet.Serialize(data);
            await _socket.SendAsync(data, SocketFlags.None, cancellationToken);
        }

        private async Task FillPipeAsync(CancellationToken cancellationToken)
        {
            const int minimumBufferSize = 512;
            var writer = _pipe.Writer;

            while (true)
            {
                // Allocate at least 512 bytes from the PipeWriter.
                Memory<byte> memory = writer.GetMemory(minimumBufferSize);
                try
                {
                    int bytesRead = await _socket.ReceiveAsync(memory, SocketFlags.None, cancellationToken);
                    if (bytesRead == 0)
                    {
                        break;
                    }
                    // Tell the PipeWriter how much was read from the Socket.
                    writer.Advance(bytesRead);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                    break;
                }

                var result = await writer.FlushAsync(cancellationToken);
                if (result.IsCompleted)
                {
                    break;
                }
            }
            await writer.CompleteAsync();
        }

        private async Task ReadMessagesAsync(CancellationToken cancellationToken)
        {
            var reader = _pipe.Reader;
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
                        var plain = _decipher.Decrypt_CBC(_rxIv.Span, block.Span);
                        var length = BinaryPrimitives.ReadUInt16BigEndian(plain.Slice(2, 2).Span);
                        if (length > 4)
                        {
                            //decrypt remaining data
                            if (buffer.Length < length + 4) continue;
                            var cryptedLength = (length + 4 + 7) & ~7; //next multiple of 8
                            if (buffer.Length < cryptedLength) continue;

                            block = buffer.Slice(0, cryptedLength).ToMemory();
                            plain = _decipher.Decrypt_CBC(_rxIv.Span, block.Span);
                        }

                        _rxIv = block.Slice(block.Length - 8);
                        OnOnMessage(plain.Slice(0, length + 4));
                        buffer = buffer.Slice(block.Length);
                        success = true;
                    } while (success && buffer.Length >= 8);
                    if (result.IsCompleted)
                        break;
                    reader.AdvanceTo(buffer.Start, buffer.End);
                }
                await reader.CompleteAsync();
            }
            catch (OperationCanceledException ex)
            {
                Console.WriteLine("cancelled in AbstractRfpProxy.ReadAsync");
                await reader.CompleteAsync(ex);
            }
        }

        protected virtual void OnOnMessage(ReadOnlyMemory<byte> data)
        {
            if (Debug)
            {
                Console.WriteLine($"< {HexEncoding.ByteToHex(data.Span)}");
            }
            var message = new AaMiDeMessageEventArgs(data, _connectionTracker);
            switch (message.Message.Type)
            {
                case MsgType.SYS_HEARTBEAT_INTERVAL:
                    OnHeartbeatInterval((SysHeartbeatIntervalMessage) message.Message);
                    break;
                case MsgType.SYS_LICENSE_TIMER:
                    OnLicenseTimer((SysLicenseTimerMessage) message.Message);
                    break;
            }
            OnMessage?.Invoke(this, message);
        }
    }

    public class AaMiDeMessageEventArgs : EventArgs
    {
        public AaMiDeMessage Message { get; }

        public AaMiDeMessageEventArgs(ReadOnlyMemory<byte> packet, RfpConnectionTracker connectionTracker)
        {
            Message = AaMiDeMessage.Create(packet, connectionTracker);
        }
    }
}