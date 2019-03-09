using System;
using System.IO;

namespace RfpProxy.Log.Messages.Rfpc
{
    public sealed class NumberOfUpnRfpcValue : DnmRfpcValue
    {
        public byte Count { get; }

        public override bool HasUnknown => false;

        public NumberOfUpnRfpcValue(ReadOnlyMemory<byte> data):base(RfpcKey.NumberOfUpn)
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