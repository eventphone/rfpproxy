using System;
using System.IO;

namespace RfpProxyLib.AaMiDe.Nwk.InformationElements
{
    public sealed class NwkIeFixedIdentity : NwkVariableLengthInformationElement
    {
        public enum FixedIdentityType : byte
        {
            Ari = 0b1000_0000,
            AriRpn = 0b1000_0001,
            WRSAriRpn = 0b1000_0010,
            Park = 0b1010_0000
        }

        public FixedIdentityType IdentityType { get; }
        
        public ReadOnlyMemory<byte> Identity { get; }

        public override ReadOnlyMemory<byte> Raw { get; }

        public NwkIeFixedIdentity(ReadOnlyMemory<byte> data) : base(NwkVariableLengthElementType.FixedIdentity, data)
        {
            var span = data.Span;
            IdentityType = (FixedIdentityType) span[0];
            var length = (span[1] & 0x7f) / 8;
            Identity = data.Slice(2, length);
            Raw = data.Slice(2).Slice(length);
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" {IdentityType:G}({Identity.ToHex()})");
        }
    }
}