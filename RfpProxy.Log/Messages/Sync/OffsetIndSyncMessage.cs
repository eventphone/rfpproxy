using System;
using System.Buffers.Binary;
using System.IO;

namespace RfpProxy.Log.Messages.Sync
{
    public sealed class OffsetIndSyncMessage : SyncMessage
    {
        public byte Reserved1 { get; }

        /// <summary>
        /// rpn, not omm rfp id
        /// </summary>
        public ushort Rpn { get; }

        public int Offset { get; }

        public byte Rssi { get; }

        public byte QtSyncCheck { get; }

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(7);

        public override bool HasUnknown => true;

        public OffsetIndSyncMessage(ReadOnlyMemory<byte> data):base(SyncMessageType.PhaseOfsWithRssiInd, data)
        {
            var span = base.Raw.Span;
            Reserved1 = span[0];
            Rpn = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(1));
            Offset = BinaryPrimitives.ReadInt16BigEndian(span.Slice(3));
            Rssi = span[5];
            QtSyncCheck = span[6];
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" Reserved1({Reserved1:x2}) RPN({Rpn:x4}) Offset({Offset*48,4}ns) RSSI({Rssi}) QT-Sync-Check({QtSyncCheck:x2})");
        }
    }
}