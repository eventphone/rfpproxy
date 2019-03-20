using System;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages.Sync
{
    public sealed class UnknownSyncMessage : SyncMessage
    {
        public ReadOnlyMemory<byte> Reserved => Raw;

        public override bool HasUnknown => true;

        public UnknownSyncMessage(SyncMessageType type, ReadOnlyMemory<byte> data):base(type, data)
        {
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" Reserved({Raw.ToHex()})");
        }
    }
}