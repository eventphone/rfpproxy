using System;

namespace RfpProxyLib.AaMiDe.Sync
{
    public sealed class UnknownSyncMessage : SyncMessage
    {
        public ReadOnlyMemory<byte> Reserved { get; }

        public override bool HasUnknown => true;

        public UnknownSyncMessage(SyncMessageType type, ReadOnlyMemory<byte> data):base(type, data)
        {
            Reserved = Raw;
        }
    }
}