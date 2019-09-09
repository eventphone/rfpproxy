using System;
using System.IO;

namespace RfpProxyLib.AaMiDe.Nwk.InformationElements
{
    public sealed class NwkIeRes : NwkVariableLengthInformationElement
    {
        public ReadOnlyMemory<byte> Res { get; }
        
        public override bool HasUnknown => false;

        public NwkIeRes(ReadOnlyMemory<byte> data):base(NwkVariableLengthElementType.RES, data)
        {
            Res = data;
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" RES({Res.ToHex()})");
        }
    }
}