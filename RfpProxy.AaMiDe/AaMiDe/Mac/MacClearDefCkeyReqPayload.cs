using System;
using System.Buffers.Binary;
using System.IO;
using RfpProxy.AaMiDe.Dnm;

namespace RfpProxy.AaMiDe.Mac
{
    public sealed class MacClearDefCkeyReqPayload : AaMiDeMessage
    {
        public DnmLayer Layer { get; }

        public DnmType DnmType { get; }

        public uint PMID { get; }

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(5);
        
        public MacClearDefCkeyReqPayload(ReadOnlyMemory<byte> data) : base(MsgType.DNM, data)
        {
            var span = base.Raw.Span;
            Layer = (DnmLayer) span[0];
            DnmType = (DnmType) span[1];
            PMID = (uint)(((span[2] & 0xf) << 16) | BinaryPrimitives.ReadUInt16BigEndian(span.Slice(3)));
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Layer({Layer,-3:G}) Type({DnmType,-20:G}) PMID({PMID:x5})");
        }
    }
}