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

        public byte Reserved2 { get; }

        public override bool HasUnknown => true;

        public OffsetIndSyncMessage(ReadOnlyMemory<byte> data):base(SyncMessageType.PhaseOfsWithRssiInd, data)
        {
            var span = Raw.Span;
            Reserved1 = Raw.Slice(0, 3);
            Offset = BinaryPrimitives.ReadInt16BigEndian(Raw.Span.Slice(3));
            Rssi = span[5];
            QtSyncCheck = span[6];
            Reserved2 = span[7];
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" Reserved1({Reserved1.ToHex()}) Offset({Offset,2}) RSSI({Rssi}) QT-Sync-Check({QtSyncCheck:x2}) Reserved2({Reserved2:x2})");
        }
    }
}