using System;
using System.Buffers.Binary;
using System.IO;
using RfpProxy.AaMiDe.Dnm;
using RfpProxyLib;

namespace RfpProxy.AaMiDe.Mac
{
    public sealed class MacConIndPayload : DnmPayload
    {
        /// <summary>
        /// Portable part MAC Identity / OMM PPn
        /// </summary>
        public uint PMID { get; }

        /// <summary>
        /// Handover
        /// </summary>
        public bool Ho { get; }

        public ReadOnlyMemory<byte> Reserved { get; }

        public override bool HasUnknown => Reserved.Length != 1 || Reserved.Span[0] != 1;

        public MacConIndPayload(ReadOnlyMemory<byte> data) : base(data)
        {
            var span = data.Span;
            PMID = (uint) (((span[0] & 0xf) << 16) | BinaryPrimitives.ReadUInt16BigEndian(span.Slice(1)));
            Ho = (span[3] & 0b0000_0010) != 0;
            Reserved = data.Slice(4);
        }
        
        public override void Log(TextWriter writer)
        {
            writer.Write($" PMID({PMID:x5}) Ho({Ho}) Reserved({Reserved.ToHex()})");
        }
    }
}