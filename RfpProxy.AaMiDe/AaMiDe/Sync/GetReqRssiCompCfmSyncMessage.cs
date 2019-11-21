using System;

namespace RfpProxy.AaMiDe.Sync
{
    public sealed class GetReqRssiCompCfmSyncMessage : SyncMessage
    {
        public override bool HasUnknown => true;

        public GetReqRssiCompCfmSyncMessage(ReadOnlyMemory<byte> data):base(SyncMessageType.GetReqRssiCompCfm, data)
        {
        }
    }
}