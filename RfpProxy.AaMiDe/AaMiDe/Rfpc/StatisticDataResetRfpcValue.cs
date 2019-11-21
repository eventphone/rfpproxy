using System;
using System.Buffers.Binary;
using System.IO;

namespace RfpProxy.AaMiDe.Rfpc
{
    public sealed class StatisticDataResetRfpcValue : DnmRfpcValue
    {
        public bool Reset { get; }

        public override ReadOnlyMemory<byte> Raw => base.Raw.Slice(4);

        public StatisticDataResetRfpcValue(ReadOnlyMemory<byte> data):base(RfpcKey.StatisticDataReset, data)
        {
            var span = data.Span;
            Reset = BinaryPrimitives.ReadUInt32LittleEndian(span) != 0;
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" {Reset}");
        }
    }
}