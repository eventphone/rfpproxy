using System;
using System.Buffers.Binary;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages.Dnm
{
    public sealed class MacConIndPayload : DnmPayload
    {
        public byte FMID { get; }

        public ushort PMID { get; }

        public ReadOnlyMemory<byte> Reserved { get; }

        public MacConIndPayload(ReadOnlyMemory<byte> data) : base(data)
        {
            var span = data.Span;
            FMID = span[0];
            PMID = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(1));
            Reserved = data.Slice(2);
        }
        
        public override void Log(TextWriter writer)
        {
            writer.WriteLine();
            writer.Write($"\tMAC: FMID({FMID:x2}) PMID({PMID:x4}) Reserved({HexEncoding.ByteToHex(Reserved.Span)})");
        }
    }
}