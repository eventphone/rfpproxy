using System;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages.Nwk.InformationElements
{
    public sealed class NwkIePortableIdentity : NwkVariableLengthInformationElement
    {
        public enum PortableIdentityType : byte
        {
            IPUI = 0b1000_0000,
            IPEI = 0b1001_0000,
            TPUI = 0b1010_0000
        }

        public ReadOnlyMemory<byte> Identity { get; }

        public PortableIdentityType IdentityType { get; }

        public override bool HasUnknown => false;

        public NwkIePortableIdentity(ReadOnlyMemory<byte> data) : base(NwkVariableLengthElementType.PortableIdentity)
        {
            var span = data.Span;
            IdentityType = (PortableIdentityType) span[0];
            var length = (span[1] & 0x7f) / 8;
            Identity = data.Slice(2, length);
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" {IdentityType:G}({Identity.ToHex()})");
        }
    }
}