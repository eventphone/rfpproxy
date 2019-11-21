using System;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.AaMiDe.Nwk.InformationElements.Proprietary.DeTeWe.Reserved2
{
    public class UnknownReserved2DeTeWeContent : Reserved2DeTeWeContent
    {
        public UnknownReserved2DeTeWeContent(Reserved2ContentDeTeWeType type, ReadOnlyMemory<byte> data) : base(type, data)
        {
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"({Raw.ToHex()})");
        }
    }
}