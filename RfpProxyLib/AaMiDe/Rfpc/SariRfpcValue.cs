using System;
using System.IO;
using RfpProxyLib;

namespace RfpProxyLib.AaMiDe.Rfpc
{
    public sealed class SariRfpcValue : DnmRfpcValue
    {
        public ReadOnlyMemory<byte> Sari { get; }

        public override ReadOnlyMemory<byte> Raw=> ReadOnlyMemory<byte>.Empty;

        public SariRfpcValue(ReadOnlyMemory<byte> data):base(RfpcKey.SARI, data)
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