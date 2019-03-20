using System;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages.Sync
{
    public sealed class GetReqRssiCompCfmSyncMessage : SyncMessage
    {
        public override bool HasUnknown => true;

        public GetReqRssiCompCfmSyncMessage(ReadOnlyMemory<byte> data):base(SyncMessageType.GetReqRssiCompCfm, data)
        {
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" Reserved({Raw.ToHex()})");
        }
    }
}