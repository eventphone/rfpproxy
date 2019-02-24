using System;
using System.Buffers.Binary;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages.Dnm
{
    public sealed class MacConIndPayload : DnmPayload
    {
        public byte Reserved1 { get; }

        /// <summary>
        /// Portable part MAC Identity / OMM PPn
        /// </summary>
        public ushort PMID { get; }

        public ReadOnlyMemory<byte> Reserved2 { get; }

        public MacConIndPayload(ReadOnlyMemory<byte> data) : base(data)
        {
            var span = data.Span;
            Reserved1 = span[0];
            PMID = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(1));
            Reserved2 = data.Slice(3);
        }
        
        public override void Log(TextWriter writer)
        {
            writer.Write($" Reserved1({Reserved1:x2}) PMID({PMID:x5}) Reserved2({Reserved2.ToHex()})");
        }
    }
}