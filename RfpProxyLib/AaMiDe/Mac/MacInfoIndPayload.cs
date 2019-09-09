using RfpProxyLib.AaMiDe.Dnm;
using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace RfpProxyLib.AaMiDe.Mac
{
    public sealed class MacInfoIndPayload : DnmPayload
    {
        public uint PMID { get; }

        public byte Reserved { get; }

        public string Text { get; }

        public override bool HasUnknown => Reserved != 0;

        public MacInfoIndPayload(ReadOnlyMemory<byte> data):base(data)
        {
            var span = data.Span;
            PMID = (uint) (((span[0] & 0xf) << 16) | BinaryPrimitives.ReadUInt16BigEndian(span.Slice(1)));

            Reserved = data.Span[3];
            Text = Encoding.UTF8.GetString(data.Span.Slice(4, data.Length-5));
        }

        public override void Log(TextWriter writer)
        {
            writer.Write($" PMID({PMID:x5}) Reserved({Reserved:x2}) Text({Text})");
        }
    }
}