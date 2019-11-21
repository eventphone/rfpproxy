using System;
using System.Buffers.Binary;
using System.IO;

namespace RfpProxy.AaMiDe.Sync
{
    public sealed class FreqCtrlModeCfmSyncMessage : SyncMessage
    {
        public byte Mode { get; }

        public ushort Ppm { get; }

        public ushort Avg { get; }
        
        public override bool HasUnknown => true;

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(5);

        public FreqCtrlModeCfmSyncMessage(ReadOnlyMemory<byte> data):base(SyncMessageType.FreqCtrlModeCfm, data)
        {
            var span = base.Raw.Span;
            Mode = span[0];
            Ppm = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(1));
            Avg = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(3));
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" Mode({Mode:x2}) ppm({Ppm}) avg({Avg})");
        }
    }
}