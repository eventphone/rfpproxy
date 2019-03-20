using System;
using System.Buffers.Binary;
using System.IO;

namespace RfpProxy.Log.Messages.Sync
{
    public sealed class StartMacSlaveModeIndSyncMessage : SyncMessage
    {
        public ushort Rfp { get; }

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(2);

        public StartMacSlaveModeIndSyncMessage(ReadOnlyMemory<byte> data):base(SyncMessageType.StartMacSlaveModeInd, data)
        {
            Rfp = BinaryPrimitives.ReadUInt16BigEndian(base.Raw.Span);
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" RFP({Rfp:x4})");
        }
    }
}