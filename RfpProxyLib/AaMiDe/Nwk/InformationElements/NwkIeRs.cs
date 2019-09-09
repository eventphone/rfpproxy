using System;
using System.IO;

namespace RfpProxyLib.AaMiDe.Nwk.InformationElements
{
    public sealed class NwkIeRs : NwkVariableLengthInformationElement
    {
        public ReadOnlyMemory<byte> Rs { get; }

        public override ReadOnlyMemory<byte> Raw => ReadOnlyMemory<byte>.Empty;

        public NwkIeRs(ReadOnlyMemory<byte> data) : base(NwkVariableLengthElementType.RS, data)
        {
            Rs = data;
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" RS({Rs.ToHex()})");
        }
    }
}