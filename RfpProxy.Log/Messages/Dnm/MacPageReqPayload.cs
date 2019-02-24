using System;
using System.Buffers.Binary;
using System.IO;

namespace RfpProxy.Log.Messages.Dnm
{
    public sealed class MacPageReqPayload : DnmPayload
    {
        public byte Reserved { get; }

        public uint PMID { get; }

        public MacPageReqPayload(ReadOnlyMemory<byte> data) : base(data)
        {
            var span = data.Span;
            Reserved = span[0];
            PMID = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(1));
        }

        public override void Log(TextWriter writer)
        {
            writer.Write($" Reserved({Reserved:x2}) PMID({PMID:x5})");
        }
    }
}