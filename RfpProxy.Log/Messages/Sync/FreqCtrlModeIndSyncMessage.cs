using System;
using System.IO;

namespace RfpProxy.Log.Messages.Sync
{
    public sealed class FreqCtrlModeIndSyncMessage : SyncMessage
    {
        public byte Mode { get; }

        public override bool HasUnknown => true;

        public FreqCtrlModeIndSyncMessage(ReadOnlyMemory<byte> data):base(SyncMessageType.SystemSearchInd, data)
        {
            Mode = Raw.Span[0];
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" Mode({Mode:x2})");
        }
    }
}