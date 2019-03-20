using System;

namespace RfpProxy.Log.Messages.Sync
{
    public sealed class SetReportLimitSyncMessage : SyncMessage
    {
        public override bool HasUnknown => true;

        public SetReportLimitSyncMessage(ReadOnlyMemory<byte> data):base(SyncMessageType.SetReportLimit, data)
        {
        }
    }
}