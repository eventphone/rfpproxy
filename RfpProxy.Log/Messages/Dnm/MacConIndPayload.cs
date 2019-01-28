using System;
using System.Buffers.Binary;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages.Dnm
{
    public sealed class MacConIndPayload : DnmPayload
    {
        /// <summary>
        /// Portable part MAC Identity / OMM PPn
        /// </summary>
        public uint PMID { get; }

        public ReadOnlyMemory<byte> Reserved { get; }

        public MacConIndPayload(ReadOnlyMemory<byte> data) : base(data)
        {
            var span = data.Span;
            PMID = (uint)(span[0] << 16) | BinaryPrimitives.ReadUInt16BigEndian(span.Slice(1));
            Reserved = data.Slice(3);
        }
        
        public override void Log(TextWriter writer)
        {
            writer.Write($"\tPMID({PMID:x5}) Reserved({HexEncoding.ByteToHex(Reserved.Span)})");
        }
    }
}