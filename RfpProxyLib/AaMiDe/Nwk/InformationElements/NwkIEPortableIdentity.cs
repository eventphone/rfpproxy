using System;
using System.Buffers.Binary;
using System.IO;

namespace RfpProxyLib.AaMiDe.Nwk.InformationElements
{
    public sealed class NwkIePortableIdentity : NwkVariableLengthInformationElement
    {
        public class IPUI
        {
            public IPUITypeCoding Put { get; }

            public ulong Number { get; }

            public ushort EMC { get; }

            public uint PSN { get; }

            public byte C { get; }

            public ReadOnlyMemory<byte> Raw { get; }

            public bool HasUnknown => Put != IPUITypeCoding.O && Put != IPUITypeCoding.N;

            public IPUI(ReadOnlyMemory<byte> data, int length)
            {
                var span = data.Span;
                Put = (IPUITypeCoding) (span[0]>>4);
                switch (Put)
                {
                    case IPUITypeCoding.O:
                        var pun = span[0] & 0xfUL;
                        span = span.Slice(1);
                        length -= 8;
                        while (length > 0)
                        {
                            pun <<= 8;
                            pun = pun | span[0];
                            span = span.Slice(1);
                            length -= 8;
                        }
                        Number = pun >> (0 - length);
                        break;
                    case IPUITypeCoding.N:
                        if (length != 40)
                            throw new ArgumentOutOfRangeException(nameof(length));
                        Number = BinaryPrimitives.ReadUInt32BigEndian(span.Slice(1));
                        Number |= (span[0] & 0xfUL) << 32;
                        EMC = (ushort) ((span[0] & 0xf) << 12 | (span[1] << 4) | (span[2] >> 4));
                        PSN = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(3)) | ((span[2] & 0xfu)<<16);
                        int checksum = 0;
                        var number = EMC * 10000000L + PSN;
                        var position = 10000_0000000L;
                        for (int i = 1; i <= 12; i++)
                        {
                            var current = number / position;
                            checksum += (int)(i * current);
                            number -= current * position;
                            position /= 10;
                        }
                        C = (byte) (checksum % 11);
                        break;
                    default:
                        Raw = data;
                        break;
                }
            }

            public override string ToString()
            {
                switch (Put)
                {
                    case IPUITypeCoding.O:
                        return Number.ToString("x");
                    case IPUITypeCoding.N:
                        return $"{EMC:D5} {PSN:D7} {(C==10?"*":C.ToString())}";
                    default:
                        return (Raw.Span[0]&0xf).ToString("x1") +  Raw.ToHex().Substring(1);
                }
            }
        }

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

        public IPUI Ipui { get; }

        public ReadOnlyMemory<byte> Identity { get; }

        public PortableIdentityType IdentityType { get; }
        
        public TPUITypeCoding TPUIType { get; }

        public override bool HasUnknown { get; }

        public override ReadOnlyMemory<byte> Raw { get; }

        public NwkIePortableIdentity(ReadOnlyMemory<byte> data) : base(NwkVariableLengthElementType.PortableIdentity, data)
        {
            var span = data.Span;
            IdentityType = (PortableIdentityType) span[0];
            int bitCount = (span[1] & 0x7f);
            var length = bitCount / 8;
            switch (IdentityType)
            {
                case PortableIdentityType.IPEI:
                case PortableIdentityType.IPUI:
                    Ipui = new IPUI(data.Slice(2), bitCount);
                    Raw = Ipui.Raw;
                    HasUnknown = Ipui.HasUnknown;
                    break;
                case PortableIdentityType.TPUI:
                    TPUIType = (TPUITypeCoding) (span[2] >> 4);
                    HasUnknown = (span[1] & 0x7f) != 20;
                    Identity = data.Slice(2);
                    Raw = ReadOnlyMemory<byte>.Empty;
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
                    writer.Write($" IPUI-{Ipui.Put:G}({Ipui})");
                    break;
                case PortableIdentityType.TPUI:
                    writer.Write($" TPUI-{TPUIType:G}({Identity.ToHex().Substring(1)})");
                    break;
                case PortableIdentityType.IPEI:
                    writer.Write($" IPEI({Ipui})");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}