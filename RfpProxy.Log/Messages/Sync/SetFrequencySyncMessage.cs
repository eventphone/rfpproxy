using System;
using System.Buffers.Binary;
using System.IO;

namespace RfpProxy.Log.Messages.Sync
{
    public sealed class SetFrequencySyncMessage : SyncMessage
    {
        public uint Frequency { get; }

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(2);

        public SetFrequencySyncMessage(ReadOnlyMemory<byte> data):base(SyncMessageType.SetFrequency, data)
        {
            Frequency = BinaryPrimitives.ReadUInt16BigEndian(base.Raw.Span);
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" Frequency({Frequency:x})");
        }
    }
}