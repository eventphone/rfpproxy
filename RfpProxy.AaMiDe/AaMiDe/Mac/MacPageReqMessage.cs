using RfpProxy.AaMiDe.Dnm;
using System;
using System.Buffers.Binary;
using System.IO;

namespace RfpProxy.AaMiDe.Mac
{
    public sealed class MacPageReqMessage : AaMiDeMessage
    {
        public DnmLayer Layer { get; }

        public DnmType DnmType { get; }

        public byte Padding { get; }

        public FlagsType Flags { get; }

        public ushort PMID { get; }

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(6);

        [Flags]
        public enum FlagsType : byte
        {
            Three = 4,
            Four = 8,
            Five = 16,
            Eight = 128,
        }
        public MacPageReqMessage(ReadOnlyMemory<byte> data) : base(MsgType.DNM, data)
        {
            var span = base.Raw.Span;
            Layer = (DnmLayer) span[0];
            DnmType = (DnmType) span[1];
            Padding = span[2];
            Flags = (FlagsType) span[3];
            PMID = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(4));
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Layer({Layer,-3:G}) Type({DnmType,-20:G})");
            writer.Write($" Padding({Padding:x2}) Flags({Flags:F}) PMID({PMID:x5})");
        }
    }
}