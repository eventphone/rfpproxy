using System;
using System.IO;

namespace RfpProxy.Log.Messages.Rfpc
{
    public sealed class NumberOfBearerRfpcValue : DnmRfpcValue
    {
        public byte Count { get; }

        public override bool HasUnknown => false;

        public NumberOfBearerRfpcValue(ReadOnlyMemory<byte> data):base(RfpcKey.NumberOfBearer)
        {
            Count = data.Span[0];
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" {Count}");
        }
    }
}