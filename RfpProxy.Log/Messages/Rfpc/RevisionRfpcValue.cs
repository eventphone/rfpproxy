using System;
using System.Buffers.Binary;
using System.IO;

namespace RfpProxy.Log.Messages.Rfpc
{
    public sealed class RevisionRfpcValue : DnmRfpcValue
    {
        public byte Generation { get; }

        public ushort ProgSW { get; }
        
        public ushort BootSW { get; }
        
        public ushort HW { get; }

        public override ReadOnlyMemory<byte> Raw => base.Raw.Slice(7);

        public RevisionRfpcValue(ReadOnlyMemory<byte> data) : base(RfpcKey.Revision, data)
        {
            var span = data.Span;
            Generation = span[0];
            BootSW = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(1));
            ProgSW = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(3));
            HW = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(5));
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Gen({Generation}) BootSW({BootSW:x4}) ProgSW({ProgSW:x4}) HW({HW:x4})");
        }
    }
}