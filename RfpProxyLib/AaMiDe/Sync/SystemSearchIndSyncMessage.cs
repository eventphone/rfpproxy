using System;
using System.IO;

namespace RfpProxyLib.AaMiDe.Sync
{
    public sealed class SystemSearchIndSyncMessage : SyncMessage
    {
        public byte Mode { get; }

        public override bool HasUnknown => true;

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(1);

        public SystemSearchIndSyncMessage(ReadOnlyMemory<byte> data):base(SyncMessageType.SystemSearchInd, data)
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