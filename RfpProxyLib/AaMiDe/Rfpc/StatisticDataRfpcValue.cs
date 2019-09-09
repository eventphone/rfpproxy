using System;
using System.Buffers.Binary;
using System.IO;

namespace RfpProxyLib.AaMiDe.Rfpc
{
    public sealed class StatisticDataRfpcValue:DnmRfpcValue
    {
        public ushort BmcConnections01_03 { get; }

        public ushort BmcConnections04_06 { get; }

        public ushort BmcConnections07_09 { get; }

        public ushort BmcConnections10_12 { get; }

        public ushort BmcDspChans01_02 { get; }

        public ushort BmcDspChans03_04 { get; }

        public ushort BmcDspChans05_06 { get; }

        public ushort BmcDspChans07_08 { get; }

        public ushort LostConnections { get; }

        public ReadOnlyMemory<byte> Reserved1 { get; }

        public ushort MacReset { get; }

        public ushort RejectDummy { get; }

        public ushort HoTimer { get; }

        public ReadOnlyMemory<byte> Reserved2 { get; }

        public uint BadFrames { get; }

        public uint GoodFrames { get; }

        public override bool HasUnknown => !Reserved1.Span.IsEmpty() || !Reserved2.Span.IsEmpty() || base.HasUnknown;

        public override ReadOnlyMemory<byte> Raw => base.Raw.Slice(52);

        public StatisticDataRfpcValue(ReadOnlyMemory<byte> data):base(RfpcKey.StatisticData, data)
        {
            var span = data.Span;
            BmcConnections01_03 = BinaryPrimitives.ReadUInt16LittleEndian(span);
            BmcConnections04_06 = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(2));
            BmcConnections07_09 = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(4));
            BmcConnections10_12 = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(6));
            BmcDspChans01_02 = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(8));
            BmcDspChans03_04 = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(10));
            BmcDspChans05_06 = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(12));
            BmcDspChans07_08 = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(14));
            LostConnections = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(16));
            Reserved1 = data.Slice(18, 16);
            MacReset = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(34));
            Reserved2 = data.Slice(36, 4);
            RejectDummy = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(40));
            BadFrames = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(42));
            GoodFrames = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(46));
            HoTimer = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(50));
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" Connections 01-03({BmcConnections01_03}) Connections 04-06({BmcConnections04_06}) Connections 07-09({BmcConnections07_09}) Connections 10-12({BmcConnections10_12})");
            writer.Write($" DSP Chan used 01-02({BmcDspChans01_02}) DSP Chan used 03-04({BmcDspChans03_04}) DSP Chan used 05-06({BmcDspChans05_06}) DSP Chan used 07-08({BmcDspChans07_08})");
            writer.Write($" Lost Connections({LostConnections})");
            if (!Reserved1.Span.IsEmpty())
                writer.Write($" Reserved1({Reserved1.ToHex()})");
            writer.Write($" MAC Reset({MacReset}) Reject Dummy({RejectDummy}) Ho Timer > 150ms({HoTimer})");
            if (!Reserved2.Span.IsEmpty())
                writer.Write($" Reserved2({Reserved2.ToHex()})");
            writer.Write($" Bad Frames({BadFrames}) Good Frames({GoodFrames})");
        }
    }
}