using System;
using System.Buffers.Binary;
using System.IO;

namespace RfpProxy.Log.Messages.Sync
{
    public sealed class SetFrequencySyncMessage : SyncMessage
    {
        public uint Frequency { get; }

        public override bool HasUnknown => false;

        public SetFrequencySyncMessage(ReadOnlyMemory<byte> data):base(SyncMessageType.SetFrequency, data)
        {
            Frequency = BinaryPrimitives.ReadUInt16BigEndian(Raw.Span);
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" Frequency({Frequency:x})");
        }
    }
}