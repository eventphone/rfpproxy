using System;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages.Rfpc
{
    public sealed class SariRfpcValue : DnmRfpcValue
    {
        public ReadOnlyMemory<byte> Sari { get; }

        public override bool HasUnknown => false;

        public SariRfpcValue(ReadOnlyMemory<byte> data):base(RfpcKey.SARI)
        {
            Sari = data;
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" {Sari.ToHex()}");
        }
    }
}