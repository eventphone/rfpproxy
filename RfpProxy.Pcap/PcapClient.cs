using System;
using System.Buffers.Binary;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using RfpProxyLib;
using RfpProxyLib.Messages;

namespace RfpProxy.Pcap
{
    abstract class PcapClient : ProxyClient
    {
        private readonly FileStream _file;

        public PcapClient(string socket, string filename) : base(socket)
        {
            _file = File.OpenWrite(filename);
            _file.SetLength(0);
            WritePcapHeader();
        }

        private void WritePcapHeader()
        {
            var header = new byte[24];
            header[0] = 0xa1; // magic
            header[1] = 0xb2;
            header[2] = 0xc3;
            header[3] = 0xd4;

            header[4] = 0x00; // major version
            header[5] = 0x02;
            header[6] = 0x00; // minor version
            header[7] = 0x04;
            
            header[16] = 0xff; // snaplen
            header[17] = 0xff;
            header[18] = 0xff;
            header[19] = 0xff;

            header[23] = 0x01; // ethernet
            _file.Write(header);
        }

        protected abstract int PacketHeaderSize { get; }

        protected abstract ReadOnlyMemory<byte> PreprocessData(ReadOnlyMemory<byte> data);

        protected abstract void WritePacketHeader(byte[] header, MessageDirection direction, uint messageId, RfpIdentifier rfp, ReadOnlyMemory<byte> data);

        protected override async Task OnMessageAsync(MessageDirection direction, uint messageId, RfpIdentifier rfp, Memory<byte> data, CancellationToken cancellationToken)
        {
            var packetheaderlength = PacketHeaderSize;
            var packetData = PreprocessData(data);
            var timestamp = DateTimeOffset.UtcNow;
            var header = new byte[16];
            BinaryPrimitives.WriteUInt32BigEndian(header, (uint) timestamp.ToUnixTimeSeconds());
            BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(4), (uint) timestamp.Millisecond * 1000);
            BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(8), (uint) (packetData.Length + packetheaderlength));
            BinaryPrimitives.WriteUInt32BigEndian(header.AsSpan(12), (uint) (packetData.Length + packetheaderlength));
            await _file.WriteAsync(header, cancellationToken).ConfigureAwait(false);
            
            header = new byte[packetheaderlength];
            WritePacketHeader(header, direction, messageId, rfp, packetData);

            await _file.WriteAsync(header, cancellationToken).ConfigureAwait(false);

            await _file.WriteAsync(packetData, cancellationToken).ConfigureAwait(false);
        }



        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _file.Dispose();
            }
        }
    }
}