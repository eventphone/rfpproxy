using System;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages.Nwk.InformationElements.Proprietary.DeTeWe
{
    public class UnknownDeTeWeElement : DeTeWeElement
    {
        public override ReadOnlyMemory<byte> Raw { get; }

        public override bool HasUnknown => true;

        public UnknownDeTeWeElement(DeTeWeType type, ReadOnlyMemory<byte> data):base(type, data)
        {
            Raw = data;
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"({Raw.ToHex()})");
        }
    }
}