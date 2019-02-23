using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using RfpProxyLib;
using RfpProxyLib.Messages;

namespace RfpProxy.Pcap
{
    class AaMiDePcapClient : PcapClient
    {
        private ConcurrentDictionary<RfpIdentifier, uint> _sequenceNumbers = new ConcurrentDictionary<RfpIdentifier, uint>();

        public AaMiDePcapClient(string socket, string filename) : base(socket, filename)
        {
        }

        protected override int PacketHeaderSize => 6 + 6 + 2 + 20 + 20;

        protected override ReadOnlyMemory<byte> PreprocessData(ReadOnlyMemory<byte> data)
        {
            return data;
        }

        protected override void WritePacketHeader(byte[] header, MessageDirection direction, uint messageId, RfpIdentifier rfp, ReadOnlyMemory<byte> data)
        {
            var span = header.AsSpan();
            
            rfp.CopyTo(header);
            rfp.CopyTo(span.Slice(6));
            if (direction == MessageDirection.FromOmm)
            {
                span[6] = 0x02;
            }
            else
            {
                span[0] = 0x02;
            }
            span[12] = 0x08;

            span = span.Slice(14);
            span[0] = 0x45;//verion + IHL
            span[1] = 0x10;//tos
            BinaryPrimitives.WriteUInt16BigEndian(span.Slice(2), (ushort) (PacketHeaderSize + data.Length)); //total length
            BinaryPrimitives.WriteUInt16BigEndian(span.Slice(4), (ushort) messageId); //identification
            BinaryPrimitives.WriteUInt16BigEndian(span.Slice(6), 0x4000); // flags
            span[8] = 0xff; //ttl
            span[9] = 6; //protocol
            if (direction == MessageDirection.ToOmm)
            {
                rfp.CopyTo(span.Slice(10)); //source address
                span[12] = 127;
                span[16] = 127; //destination address
                span[17] = 0;
                span[18] = 0;
                span[19] = 1;
            }
            else
            {
                rfp.CopyTo(span.Slice(14)); //destination address
                span[16] = 127;
                span[12] = 127; //source address
                span[13] = 0;
                span[14] = 0;
                span[15] = 1;
            }
            span = span.Slice(20);
            BinaryPrimitives.WriteUInt16BigEndian(span, 54321); //source port
            BinaryPrimitives.WriteUInt16BigEndian(span.Slice(2), 16321); //destination port
            var seq = _sequenceNumbers.AddOrUpdate(rfp, x => 1, (x, i) => ++i);
            BinaryPrimitives.WriteUInt32BigEndian(span.Slice(4), seq);
            span[8] = 0x50;//data offset
        }
    }
}