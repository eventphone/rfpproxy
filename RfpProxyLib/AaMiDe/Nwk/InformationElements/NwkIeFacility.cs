using System;
using System.IO;

namespace RfpProxyLib.AaMiDe.Nwk.InformationElements
{
    public sealed class NwkIeFacility : NwkVariableLengthInformationElement
    {
        public bool HasValidServiceDiscriminator { get; }

        //see EN 300 196-1
        public ReadOnlyMemory<byte> Components { get; }

        public override bool HasUnknown => !HasValidServiceDiscriminator;

        public override ReadOnlyMemory<byte> Raw => ReadOnlyMemory<byte>.Empty;

        public NwkIeFacility(ReadOnlyMemory<byte> data):base(NwkVariableLengthElementType.Facility, data)
        {
            var span = data.Span;
            HasValidServiceDiscriminator = span[0] == 0x91;
            Components = data.Slice(1);
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            if (!HasValidServiceDiscriminator)
                writer.Write(" invalid ServiceDiscriminator");
            writer.Write($" Components({Components.ToHex()})");
        }
    }
}