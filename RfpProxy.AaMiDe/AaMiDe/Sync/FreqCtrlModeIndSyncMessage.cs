using System;
using System.IO;

namespace RfpProxy.AaMiDe.Sync
{
    public sealed class FreqCtrlModeIndSyncMessage : SyncMessage
    {
        public byte Mode { get; }

        public override bool HasUnknown => true;

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(1);

        public FreqCtrlModeIndSyncMessage(ReadOnlyMemory<byte> data):base(SyncMessageType.SystemSearchInd, data)
        {
            Mode = base.Raw.Span[0];
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" Mode({Mode:x2})");
        }
    }
}