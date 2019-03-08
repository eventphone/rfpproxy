using System;
using System.IO;

namespace RfpProxy.Log.Messages.Rfpc
{
    public sealed class RfpPliRfpcValue : DnmRfpcValue
    {
        public byte LengthIndicator { get; }

        public override bool HasUnknown => false;

        public RfpPliRfpcValue(ReadOnlyMemory<byte> data):base(RfpcKey.RfpPli)
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