using System;
using System.Buffers.Binary;
using System.IO;

namespace RfpProxy.Log.Messages.Sync
{
    public sealed class SystemSearchCfmSyncMessage : SyncMessage
    {
        public (ushort,ushort)[] Rssi { get; }

        public override bool HasUnknown => false;

        public SystemSearchCfmSyncMessage(ReadOnlyMemory<byte> data):base(SyncMessageType.SystemSearchCfm, data)
        {
            if (Raw.IsEmpty)
                return;
            var span = Raw.Span;
            var count = span[0];
            span = span.Slice(1);
            Rssi = new (ushort, ushort)[count];
            for (int i = 0; i < count; i++)
            {
                var rfpn = BinaryPrimitives.ReadUInt16LittleEndian(span);
                var rssi = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(2));
                Rssi[i] = (rfpn, rssi);
            }
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            if (Rssi == null) return;
            foreach (var (rfpn,rssi) in Rssi)
            {
                writer.Write($" RSSI({rfpn:x4},{rssi})");
            }
        }
    }
}