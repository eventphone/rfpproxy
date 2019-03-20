using System;

namespace RfpProxy.Log.Messages.Sync
{
    public sealed class UnknownSyncMessage : SyncMessage
    {
        public ReadOnlyMemory<byte> Reserved => Raw;

        public override bool HasUnknown => true;

        public UnknownSyncMessage(SyncMessageType type, ReadOnlyMemory<byte> data):base(type, data)
        {
        }
    }
}