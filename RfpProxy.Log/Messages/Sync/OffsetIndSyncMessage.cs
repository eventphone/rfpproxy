using System;
using System.Buffers.Binary;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages.Sync
{
    public sealed class OffsetIndSyncMessage : SyncMessage
    {
        public ReadOnlyMemory<byte> Reserved1 { get; }

        public int Offset { get; }

        public byte Rssi { get; }

        public byte QtSyncCheck { get; }

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(7);

        public override bool HasUnknown => true;

        public OffsetIndSyncMessage(ReadOnlyMemory<byte> data):base(SyncMessageType.PhaseOfsWithRssiInd, data)
        {
            var span = base.Raw.Span;
            Reserved1 = base.Raw.Slice(0, 3);
            Offset = BinaryPrimitives.ReadInt16BigEndian(span.Slice(3));
            Rssi = span[5];
            QtSyncCheck = span[6];
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" Reserved1({Reserved1.ToHex()}) Offset({Offset,2}) RSSI({Rssi}) QT-Sync-Check({QtSyncCheck:x2})");
        }
    }
}