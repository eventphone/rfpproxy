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

        public ReadOnlyMemory<byte> Reserved { get; }

        public uint PMID { get; }

        public override bool HasUnknown => true;

        public MacPageReqMessage(ReadOnlyMemory<byte> data) : base(MsgType.DNM, data)
        {
            var span = Raw.Span;
            Layer = (DnmLayer) span[0];
            DnmType = (DnmType) span[1];
            Reserved = Raw.Slice(2,2);
            PMID = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(4));
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Layer({Layer,-3:G}) Type({DnmType,-20:G})");
            writer.Write($" Reserved({Reserved.ToHex()}) PMID({PMID:x5})");
        }
    }
}