using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.IO;
using RfpProxyLib;
using RfpProxyLib.Messages;

namespace RfpProxy.Pcap
{
    class AaMiDePcapClient : PcapClient
    {
        private readonly ConcurrentDictionary<RfpIdentifier, int> _rfpSequenceNumbers = new ConcurrentDictionary<RfpIdentifier, int>();
        private readonly ConcurrentDictionary<RfpIdentifier, int> _ommSequenceNumbers = new ConcurrentDictionary<RfpIdentifier, int>();

        public AaMiDePcapClient(string socket, Stream file) : base(socket, file)
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
            span[0] = 0x45;//version + IHL
            span[1] = 0x10;//tos
            BinaryPrimitives.WriteUInt16BigEndian(span.Slice(2), (ushort) (span.Length + data.Length)); //total length
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
            int seq;
            int ack;
            if (direction == MessageDirection.ToOmm)
            {
                seq = _rfpSequenceNumbers.AddOrUpdate(rfp, x => data.Length, (x, i) => i+data.Length);
                ack = _ommSequenceNumbers.GetOrAdd(rfp, 1);
                BinaryPrimitives.WriteUInt16BigEndian(span, 54321); //source port
                BinaryPrimitives.WriteUInt16BigEndian(span.Slice(2), 16321); //destination port
            }
            else
            {
                seq = _ommSequenceNumbers.AddOrUpdate(rfp, x => data.Length, (x, i) => i+data.Length);
                ack = _rfpSequenceNumbers.GetOrAdd(rfp, 1);
                BinaryPrimitives.WriteUInt16BigEndian(span, 16321); //source port
                BinaryPrimitives.WriteUInt16BigEndian(span.Slice(2), 54321); //destination port

            }
            BinaryPrimitives.WriteInt32BigEndian(span.Slice(4), seq - data.Length);
            BinaryPrimitives.WriteInt32BigEndian(span.Slice(8), ack);
            span[12] = 0x50;//data offset
            span[13] = 0b0001_0000;//flags
            span[14] = 0xff;//window size
            span[15] = 0xff;
        }
    }
}