using System;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.AaMiDe.Nwk.InformationElements.Proprietary.DeTeWe
{
    public class BtEthAddrDeTeWeElement : DeTeWeElement
    {
        public ReadOnlyMemory<byte> Address => base.Raw.Slice(0, 6);

        public override ReadOnlyMemory<byte> Raw => base.Raw.Slice(6);

        public BtEthAddrDeTeWeElement(ReadOnlyMemory<byte> data) : base(DeTeWeType.BtEthAddr, data)
        {
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"({Address.ToHex()})");
        }
    }
}