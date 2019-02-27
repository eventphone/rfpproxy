using System;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages.Nwk.InformationElements
{
    public sealed class NwkIeRs : NwkVariableLengthInformationElement
    {
        public ReadOnlyMemory<byte> Rs { get; }

        public override bool HasUnknown => false;

        public NwkIeRs(ReadOnlyMemory<byte> data) : base(NwkVariableLengthElementType.RS)
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