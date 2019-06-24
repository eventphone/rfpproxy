using System;
using System.Buffers.Binary;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages.Dnm
{
    public sealed class MacHoInProgressIndPayload : DnmPayload
    {
        public uint PMID { get; }

        public override ReadOnlyMemory<byte> Raw => base.Raw.Slice(3);
        
        public override bool HasUnknown => Raw.Length != 2 || (Raw.Span[0] != 0 && Raw.Span[0] != 2) || Raw.Span[1] != 1;

        public MacHoInProgressIndPayload(ReadOnlyMemory<byte> data):base(data)
        {
            var span = data.Span;
            PMID = (uint) (((span[0] & 0xf) << 16) | BinaryPrimitives.ReadUInt16BigEndian(span.Slice(1)));
        }

        public override void Log(TextWriter writer)
        {
            writer.Write($" PMID({PMID:x5}) Reserved({Raw.ToHex()})");
        }
    }
}