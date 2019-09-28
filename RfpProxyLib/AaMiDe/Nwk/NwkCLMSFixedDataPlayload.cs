using System;
using System.Buffers.Binary;
using System.IO;

namespace RfpProxyLib.AaMiDe.Nwk
{
    public sealed class NwkCLMSFixedDataPlayload : NwkCLMSFixedPayload
    {
        public byte Section { get; }

        public uint Data { get; }

        public override bool HasUnknown => false;
        
        public NwkCLMSFixedDataPlayload(byte ti, bool f, ReadOnlyMemory<byte> data) : base(ti, f)
        {
            Section = (byte) (data.Span[0] & 0x7);
            Data = BinaryPrimitives.ReadUInt32BigEndian(data.Span.Slice(1));
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.WriteLine(" Data");
            writer.Write($"\t\tSection({Section}) Data({Data:x8})");
        }
    }
}