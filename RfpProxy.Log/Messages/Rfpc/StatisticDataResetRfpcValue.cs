using System;
using System.IO;

namespace RfpProxy.Log.Messages.Rfpc
{
    public sealed class StatisticDataResetRfpcValue : DnmRfpcValue
    {
        public bool Reset { get; }

        public override ReadOnlyMemory<byte> Raw => base.Raw.Slice(1);

        public StatisticDataResetRfpcValue(ReadOnlyMemory<byte> data):base(RfpcKey.StatisticDataReset, data)
        {
            Reset = data.Span[0] != 0;
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" {Reset}");
        }
    }
}