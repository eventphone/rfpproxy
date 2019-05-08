using System;
using System.Buffers.Binary;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages.Dnm
{
    public sealed class MacPageReqMessage : AaMiDeMessage
    {
        public DnmLayer Layer { get; }

        public DnmType DnmType { get; }

        public byte Padding { get; }

        public uint PMID { get; }

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(6);

        public MacPageReqMessage(ReadOnlyMemory<byte> data) : base(MsgType.DNM, data)
        {
            var span = base.Raw.Span;
            Layer = (DnmLayer) span[0];
            DnmType = (DnmType) span[1];
            Padding = span[2];
            PMID = (uint)(((span[3] & 0xf) << 16) | BinaryPrimitives.ReadUInt16BigEndian(span.Slice(4)));
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Layer({Layer,-3:G}) Type({DnmType,-20:G})");
            writer.Write($" Padding({Padding:x2}) PMID({PMID:x5})");
        }
    }
}