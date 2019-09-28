using System;

namespace RfpProxyLib.AaMiDe.Nwk.InformationElements
{
    public sealed class NwkIeEmpty : NwkVariableLengthInformationElement
    {
        public NwkIeEmpty() : base(0, Array.Empty<byte>())
        {
        }
    }
}