using System;
using System.Buffers.Binary;
using System.IO;
using RfpProxyLib;
using RfpProxyLib.Messages;

namespace RfpProxy.Pcap
{
    class DnmPcapClient : PcapClient
    {
        public DnmPcapClient(string socket, Stream file) : base(socket, file)
        {
        }

        protected override int PacketHeaderSize => 6 + 6 + 2 + 4;

        protected override ReadOnlyMemory<byte> PreprocessData(ReadOnlyMemory<byte> data)
        {
            return data.Slice(4);
        }

        protected override void WritePacketHeader(byte[] header, MessageDirection direction, uint messageId, RfpIdentifier rfp, ReadOnlyMemory<byte> data)
        {
            rfp.CopyTo(header);
            rfp.CopyTo(header.AsSpan(6));
            if (direction == MessageDirection.FromOmm)
            {
                header[6] = 0x02;
            }
            else
            {
                header[0] = 0x02;
            }
            header[12] = 0xa0;

            BinaryPrimitives.WriteUInt16BigEndian(header.AsSpan(14), (ushort) data.Length);
            header[16] = 0xba;
            header[17] = 0xbe;
        }
    }
}