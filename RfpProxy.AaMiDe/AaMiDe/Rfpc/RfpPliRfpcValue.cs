using System;
using System.IO;

namespace RfpProxy.AaMiDe.Rfpc
{
    public sealed class RfpPliRfpcValue : DnmRfpcValue
    {
        public byte LengthIndicator { get; }

        public override ReadOnlyMemory<byte> Raw => base.Raw.Slice(1);

        public RfpPliRfpcValue(ReadOnlyMemory<byte> data):base(RfpcKey.RfpPli, data)
        {
            LengthIndicator = data.Span[0];
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" ParkLengthIndicator({LengthIndicator})");
        }
    }
}