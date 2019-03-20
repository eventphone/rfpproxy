using System;

namespace RfpProxy.Log.Messages.Sync
{
    public sealed class EmptySyncMessage : SyncMessage
    {
        public override bool HasUnknown => !Raw.IsEmpty;

        public EmptySyncMessage(SyncMessageType type, ReadOnlyMemory<byte> data):base(type, data)
        {
        }
    }
}