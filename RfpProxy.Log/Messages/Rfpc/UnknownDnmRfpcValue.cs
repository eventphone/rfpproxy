using System;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages.Rfpc
{
    public sealed class UnknownDnmRfpcValue : DnmRfpcValue
    {
        public ReadOnlyMemory<byte> Value { get; }

        public override bool HasUnknown => true;

        public UnknownDnmRfpcValue(RfpcKey type, ReadOnlyMemory<byte> data):base(type, data)
        {
            Value = data;
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" {Value.ToHex()}");
        }
    }
}