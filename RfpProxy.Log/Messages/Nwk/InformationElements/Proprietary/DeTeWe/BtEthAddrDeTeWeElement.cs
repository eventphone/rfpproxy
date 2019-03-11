using System;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages.Nwk.InformationElements.Proprietary.DeTeWe
{
    public class BtEthAddrDeTeWeElement : DeTeWeElement
    {
        public ReadOnlyMemory<byte> Address { get; }

        public override bool HasUnknown => false;

        public BtEthAddrDeTeWeElement(ReadOnlyMemory<byte> data) : base(DeTeWeType.BtEthAddr)
        {
            Address = data;
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"({Address.ToHex()})");
        }
    }
}