using System;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages.Nwk.InformationElements.Proprietary.DeTeWe
{
    public class UnknownDeTeWeElement : DeTeWeElement
    {
        public override ReadOnlyMemory<byte> Raw => ReadOnlyMemory<byte>.Empty;

        public override bool HasUnknown => true;

        public UnknownDeTeWeElement(DeTeWeType type, ReadOnlyMemory<byte> data):base(type, data)
        {
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"({base.Raw.ToHex()})");
        }
    }
}