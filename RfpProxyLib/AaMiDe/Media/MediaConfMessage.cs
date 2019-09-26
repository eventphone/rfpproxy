using System;
using System.Buffers.Binary;
using System.IO;
using System.Net;

namespace RfpProxyLib.AaMiDe.Media
{
    public sealed class MediaConfMessage : MediaMessage
    {
        public enum CodecName:byte
        {
            G711aLaw = 0,
            G723_53k = 1,
            G723_63k = 2,
            G729 = 3,
            G711uLaw = 4,
        }

        public class Codec
        {
            public CodecName Name { get; }

            public byte Rate { get; }

            public byte Pt { get; }

            public Codec(ReadOnlySpan<byte> data)
            {
                if (data.Length != 3)
                    throw new ArgumentOutOfRangeException(nameof(data));
                Name = (CodecName) data[0];
                Pt = data[1];
                Rate = data[2];
            }
        }

        public byte MCEI { get; }

        public byte Vif { get; }

        public bool Vad { get; }

        public byte NumCodecs { get; }

        public Codec[] Codecs { get; }

        public ushort PPN { get; }

        public ushort LocalPort1 { get; }

        public ushort LocalPort2 { get; }

        public IPAddress RxIpAddress { get; }

        public ushort RxPort1 { get; }

        public ushort RxPort2 { get; }

        public IPAddress TxIpAddress { get; }

        public ushort TxPort1 { get; }

        public ushort TxPort2 { get; }

        public override bool HasUnknown => true;

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(86);

        public ushort Reserved1 { get; }

        public ReadOnlyMemory<byte> Reserved2 { get; }

        public byte Reserved3 { get; }

        public ReadOnlyMemory<byte> Reserved4 { get; }

        public ReadOnlyMemory<byte> Reserved5 { get; }

        public MediaConfMessage(ReadOnlyMemory<byte> data):base(MsgType.MEDIA_CONF, data)
        {
            var span = base.Raw.Span;
            Reserved1 = BinaryPrimitives.ReadUInt16BigEndian(span);
            Vif = span[2];
            Vad = span[3] != 0;
            NumCodecs = span[4];
            Codecs = new Codec[NumCodecs];
            var codecs = span.Slice(5);
            for (int i = 0; i < NumCodecs; i++)
            {
                Codecs[i] = new Codec(codecs.Slice(0, 3));
                codecs = codecs.Slice(3);
            }
            Reserved2 = base.Raw.Slice(0, 56).Slice(5).Slice(NumCodecs * 3);//additional codecs?

            MCEI = span[56];
            Reserved3 = span[57];

            PPN = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(58));
            Reserved4 = base.Raw.Slice(60, 4);

            LocalPort1 = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(64));
            LocalPort2 = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(66));
            Reserved5 = base.Raw.Slice(68, 2);

            RxIpAddress = new IPAddress(span.Slice(70,4));
            RxPort1 = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(74));
            RxPort2 = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(76));
            TxIpAddress = new IPAddress(span.Slice(78,4));
            TxPort1 = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(82));
            TxPort2 = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(84));
            //SRTP?
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Reserved1({Reserved1:x4}) ");
            writer.WriteLine();
            writer.WriteLine($"\tvif({Vif}ms) vad({Vad}) numCodecs({NumCodecs})");
            foreach (var codec in Codecs)
            {
                writer.WriteLine($"\t{codec.Name} rate({codec.Rate}000/s) pt({codec.Pt})");
            }
            writer.Write($"\tReserved2({Reserved2.ToHex()}) ");
            writer.Write($"MCEI({MCEI}) ");
            writer.Write($"Reserved3({Reserved3:x2}) ");
            writer.Write($"PPN({PPN:x4}) ");
            writer.Write($"Reserved4({Reserved4.ToHex()}) ");
            writer.Write($"Local({LocalPort1}/{LocalPort2}) ");
            writer.Write($"Reserved5({Reserved5.ToHex()}) ");
            writer.Write($"Rx({RxIpAddress}:{RxPort1}/{RxPort2}) ");
            writer.Write($"Tx({TxIpAddress}:{TxPort1}/{TxPort2}) ");
        }
    }
}