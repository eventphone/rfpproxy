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

        public enum IPUITypeCoding : byte
        {
            N= 0b0000,
            O= 0b0001,
            P= 0b0010,
            Q= 0b0011,
            R= 0b0100,
            S= 0b0101,
            T= 0b0110,
            U= 0b0111,
        }

        public enum TPUITypeCoding : byte
        {
            WithoutNumber = 0b0000,
            WithNumber = 0b0001,
        }

        public ReadOnlyMemory<byte> Identity { get; }

        public PortableIdentityType IdentityType { get; }

        public IPUITypeCoding IPUIType { get; }
        
        public TPUITypeCoding TPUIType { get; }

        public override bool HasUnknown { get; }

        public NwkIePortableIdentity(ReadOnlyMemory<byte> data) : base(NwkVariableLengthElementType.PortableIdentity)
        {
            var span = data.Span;
            IdentityType = (PortableIdentityType) span[0];
            var length = (span[1] & 0x7f) / 8;
            switch (IdentityType)
            {
                case PortableIdentityType.IPUI:
                    IPUIType = (IPUITypeCoding) (span[2] >> 4);
                    HasUnknown = IPUIType != IPUITypeCoding.O;
                    Identity = data.Slice(2, length);
                    break;
                case PortableIdentityType.IPEI:
                    Identity = data.Slice(2, length);
                    HasUnknown = true;
                    break;
                case PortableIdentityType.TPUI:
                    TPUIType = (TPUITypeCoding) (span[2] >> 4);
                    HasUnknown = (span[1] & 0x7f) != 20;
                    Identity = data.Slice(2);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);

            switch (IdentityType)
            {
                case PortableIdentityType.IPUI:
                    writer.Write($" IPUI-{IPUIType:G}({Identity.ToHex()})");
                    break;
                case PortableIdentityType.TPUI:
                    writer.Write($" TPUI-{TPUIType:G}({Identity.ToHex().Substring(1)})");
                    break;
                case PortableIdentityType.IPEI:
                    writer.Write($" IPEI({Identity.ToHex()})");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}