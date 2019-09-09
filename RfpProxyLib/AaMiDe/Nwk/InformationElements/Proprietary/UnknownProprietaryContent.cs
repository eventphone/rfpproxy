using System;
using System.IO;

namespace RfpProxyLib.AaMiDe.Nwk.InformationElements.Proprietary
{
    public class UnknownProprietaryContent : NwkIeProprietaryContent
    {
        public ReadOnlyMemory<byte> Raw { get; }

        public override bool HasUnknown => false;//true;

        public UnknownProprietaryContent(ReadOnlyMemory<byte> data)
        {
            Raw = data;
        }

        public override void Log(TextWriter writer)
        {
            writer.Write($" Reserved({Raw.ToHex()})");
        }
    }
}