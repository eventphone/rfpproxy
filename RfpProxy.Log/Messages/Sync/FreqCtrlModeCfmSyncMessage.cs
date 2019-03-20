﻿using System;
using System.Buffers.Binary;
using System.IO;

namespace RfpProxy.Log.Messages.Sync
{
    public sealed class FreqCtrlModeCfmSyncMessage : SyncMessage
    {
        public byte Mode { get; }

        public ushort Ppm { get; }

        public ushort Avg { get; }

        public byte Reserved { get; }

        public override bool HasUnknown => true;

        public FreqCtrlModeCfmSyncMessage(ReadOnlyMemory<byte> data):base(SyncMessageType.FreqCtrlModeCfm, data)
        {
            var span = Raw.Span;
            Mode = span[0];
            Ppm = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(1));
            Avg = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(3));
            Reserved = span[5];
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" Mode({Mode:x2}) ppm({Ppm}) avg({Avg}) Reserved({Reserved:x2})");
        }
    }
}