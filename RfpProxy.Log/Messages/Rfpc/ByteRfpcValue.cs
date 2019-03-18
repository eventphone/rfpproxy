using System;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages.Rfpc
{
    public sealed class ByteRfpcValue : DnmRfpcValue
    {
        public byte Value { get; }

        public ReadOnlyMemory<byte> Reserved { get; }

        public override bool HasUnknown => !Reserved.IsEmpty;

        public ByteRfpcValue(RfpcKey type, ReadOnlyMemory<byte> data):base(type)
        {
            Value = data.Span[0];
            Reserved = data.Slice(1);
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" {Value}");
            if (HasUnknown)
                writer.Write($" Reserved({Reserved.ToHex()})");
        }
    }
}