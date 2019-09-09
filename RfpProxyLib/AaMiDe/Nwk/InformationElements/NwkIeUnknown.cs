using System;
using System.IO;

namespace RfpProxyLib.AaMiDe.Nwk.InformationElements
{
    public sealed class NwkIeUnknown : NwkVariableLengthInformationElement
    {
        public override bool HasUnknown => true;

        public NwkIeUnknown(NwkVariableLengthElementType type, ReadOnlyMemory<byte> data) : base(type, data)
        {
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" Reserved({Raw.ToHex()})");
        }
    }
}