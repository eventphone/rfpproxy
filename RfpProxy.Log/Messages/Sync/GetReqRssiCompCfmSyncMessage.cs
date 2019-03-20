using System;

namespace RfpProxy.Log.Messages.Sync
{
    public sealed class GetReqRssiCompCfmSyncMessage : SyncMessage
    {
        public override bool HasUnknown => true;

        public GetReqRssiCompCfmSyncMessage(ReadOnlyMemory<byte> data):base(SyncMessageType.GetReqRssiCompCfm, data)
        {
        }
    }
}