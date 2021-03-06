﻿using System;
using System.Buffers.Binary;
using System.IO;

namespace RfpProxy.AaMiDe.Sync
{
    public sealed class OffsetIndSyncMessage : SyncMessage
    {
        public byte Count { get; }

        public OffsetInd[] RFPs { get; }

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(1 + RFPs.Length * 6);

        public override bool HasUnknown => false;

        public OffsetIndSyncMessage(ReadOnlyMemory<byte> data):base(SyncMessageType.PhaseOfsWithRssiInd, data)
        {
            var span = base.Raw.Span;
            var count = span[0];
            RFPs = new OffsetInd[count];
            span = span.Slice(1);
            for (int i = 0; i < RFPs.Length; i++)
            {
                var rpn = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(0));
                var offset = BinaryPrimitives.ReadInt16BigEndian(span.Slice(2));
                var rssi = span[4];
                var qtSyncCheck = span[5];
                RFPs[i] = new OffsetInd(rpn, offset, rssi, qtSyncCheck);
                span = span.Slice(6);
            }
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            for (int i = 0; i < RFPs.Length; i++)
            {
                var rfp = RFPs[i];
                writer.WriteLine();
                writer.Write($"\tRPN({rfp.Rpn:x4}) Offset({rfp.Offset*48,4}ns) RSSI({rfp.Rssi}) QT-Sync-Check({rfp.QtSyncCheck:x2})");
            }
        }

        public class OffsetInd
        {
            public OffsetInd(ushort rpn, short offset, byte rssi, byte qtSyncCheck)
            {
                Rpn = rpn;
                Offset = offset;
                Rssi = rssi;
                QtSyncCheck = qtSyncCheck;
            }

            /// <summary>
            /// rpn, not omm rfp id
            /// </summary>
            public ushort Rpn { get; }

            public short Offset { get; }

            public byte Rssi { get; }

            public byte QtSyncCheck { get; }

        }
    }
}