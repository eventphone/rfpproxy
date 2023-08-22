using System;
using System.Buffers.Binary;
using System.IO;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using RfpProxyLib;

namespace RfpProxy.AaMiDe.Sys
{
    public sealed class SysInitMessage : AaMiDeMessage
    {
        private static readonly byte[] AesKey = 
        {
            0xe7, 0x05, 0xbc, 0x1a, 0x92, 0x41, 0x2f, 0x32,
            0x62, 0xc5, 0x47, 0xf8, 0x79, 0x46, 0x93, 0x69,
            0x97, 0xe6, 0x90, 0xad, 0xa4, 0x6f, 0xad, 0x25,
            0xbb, 0xc6, 0x26, 0xf6, 0xf5, 0xa5, 0xa6, 0xce
        };

        private static readonly byte[] _signatureKey = HexEncoding.HexToByte(
            "e7adda3adb0521f3d3fbdf3a18ee8648" +
            "b47398b1570c2b45ef8d2a9180a1a32c" +
            "69284a9c97d444abf87f5c578f942821" +
            "4dd0183cba969dc5");

        private static readonly uint[] CrcTable = {
                    0x00000000, 0xB71DC104, 0x6E3B8209, 0xD926430D,
                    0xDC760413, 0x6B6BC517, 0xB24D861A, 0x0550471E,
                    0xB8ED0826, 0x0FF0C922, 0xD6D68A2F, 0x61CB4B2B,
                    0x649B0C35, 0xD386CD31, 0x0AA08E3C, 0xBDBD4F38,
                    0x70DB114C, 0xC7C6D048, 0x1EE09345, 0xA9FD5241,
                    0xACAD155F, 0x1BB0D45B, 0xC2969756, 0x758B5652,
                    0xC836196A, 0x7F2BD86E, 0xA60D9B63, 0x11105A67,
                    0x14401D79, 0xA35DDC7D, 0x7A7B9F70, 0xCD665E74,
                    0xE0B62398, 0x57ABE29C, 0x8E8DA191, 0x39906095,
                    0x3CC0278B, 0x8BDDE68F, 0x52FBA582, 0xE5E66486,
                    0x585B2BBE, 0xEF46EABA, 0x3660A9B7, 0x817D68B3,
                    0x842D2FAD, 0x3330EEA9, 0xEA16ADA4, 0x5D0B6CA0,
                    0x906D32D4, 0x2770F3D0, 0xFE56B0DD, 0x494B71D9,
                    0x4C1B36C7, 0xFB06F7C3, 0x2220B4CE, 0x953D75CA,
                    0x28803AF2, 0x9F9DFBF6, 0x46BBB8FB, 0xF1A679FF,
                    0xF4F63EE1, 0x43EBFFE5, 0x9ACDBCE8, 0x2DD07DEC,
                    0x77708634, 0xC06D4730, 0x194B043D, 0xAE56C539,
                    0xAB068227, 0x1C1B4323, 0xC53D002E, 0x7220C12A,
                    0xCF9D8E12, 0x78804F16, 0xA1A60C1B, 0x16BBCD1F,
                    0x13EB8A01, 0xA4F64B05, 0x7DD00808, 0xCACDC90C,
                    0x07AB9778, 0xB0B6567C, 0x69901571, 0xDE8DD475,
                    0xDBDD936B, 0x6CC0526F, 0xB5E61162, 0x02FBD066,
                    0xBF469F5E, 0x085B5E5A, 0xD17D1D57, 0x6660DC53,
                    0x63309B4D, 0xD42D5A49, 0x0D0B1944, 0xBA16D840,
                    0x97C6A5AC, 0x20DB64A8, 0xF9FD27A5, 0x4EE0E6A1,
                    0x4BB0A1BF, 0xFCAD60BB, 0x258B23B6, 0x9296E2B2,
                    0x2F2BAD8A, 0x98366C8E, 0x41102F83, 0xF60DEE87,
                    0xF35DA999, 0x4440689D, 0x9D662B90, 0x2A7BEA94,
                    0xE71DB4E0, 0x500075E4, 0x892636E9, 0x3E3BF7ED,
                    0x3B6BB0F3, 0x8C7671F7, 0x555032FA, 0xE24DF3FE,
                    0x5FF0BCC6, 0xE8ED7DC2, 0x31CB3ECF, 0x86D6FFCB,
                    0x8386B8D5, 0x349B79D1, 0xEDBD3ADC, 0x5AA0FBD8,
                    0xEEE00C69, 0x59FDCD6D, 0x80DB8E60, 0x37C64F64,
                    0x3296087A, 0x858BC97E, 0x5CAD8A73, 0xEBB04B77,
                    0x560D044F, 0xE110C54B, 0x38368646, 0x8F2B4742,
                    0x8A7B005C, 0x3D66C158, 0xE4408255, 0x535D4351,
                    0x9E3B1D25, 0x2926DC21, 0xF0009F2C, 0x471D5E28,
                    0x424D1936, 0xF550D832, 0x2C769B3F, 0x9B6B5A3B,
                    0x26D61503, 0x91CBD407, 0x48ED970A, 0xFFF0560E,
                    0xFAA01110, 0x4DBDD014, 0x949B9319, 0x2386521D,
                    0x0E562FF1, 0xB94BEEF5, 0x606DADF8, 0xD7706CFC,
                    0xD2202BE2, 0x653DEAE6, 0xBC1BA9EB, 0x0B0668EF,
                    0xB6BB27D7, 0x01A6E6D3, 0xD880A5DE, 0x6F9D64DA,
                    0x6ACD23C4, 0xDDD0E2C0, 0x04F6A1CD, 0xB3EB60C9,
                    0x7E8D3EBD, 0xC990FFB9, 0x10B6BCB4, 0xA7AB7DB0,
                    0xA2FB3AAE, 0x15E6FBAA, 0xCCC0B8A7, 0x7BDD79A3,
                    0xC660369B, 0x717DF79F, 0xA85BB492, 0x1F467596,
                    0x1A163288, 0xAD0BF38C, 0x742DB081, 0xC3307185,
                    0x99908A5D, 0x2E8D4B59, 0xF7AB0854, 0x40B6C950,
                    0x45E68E4E, 0xF2FB4F4A, 0x2BDD0C47, 0x9CC0CD43,
                    0x217D827B, 0x9660437F, 0x4F460072, 0xF85BC176,
                    0xFD0B8668, 0x4A16476C, 0x93300461, 0x242DC565,
                    0xE94B9B11, 0x5E565A15, 0x87701918, 0x306DD81C,
                    0x353D9F02, 0x82205E06, 0x5B061D0B, 0xEC1BDC0F,
                    0x51A69337, 0xE6BB5233, 0x3F9D113E, 0x8880D03A,
                    0x8DD09724, 0x3ACD5620, 0xE3EB152D, 0x54F6D429,
                    0x7926A9C5, 0xCE3B68C1, 0x171D2BCC, 0xA000EAC8,
                    0xA550ADD6, 0x124D6CD2, 0xCB6B2FDF, 0x7C76EEDB,
                    0xC1CBA1E3, 0x76D660E7, 0xAFF023EA, 0x18EDE2EE,
                    0x1DBDA5F0, 0xAAA064F4, 0x738627F9, 0xC49BE6FD,
                    0x09FDB889, 0xBEE0798D, 0x67C63A80, 0xD0DBFB84,
                    0xD58BBC9A, 0x62967D9E, 0xBBB03E93, 0x0CADFF97,
                    0xB110B0AF, 0x060D71AB, 0xDF2B32A6, 0x6836F3A2,
                    0x6D66B4BC, 0xDA7B75B8, 0x035D36B5, 0xB440F7B1
                };

        public enum RfpBranding:ushort
        {
            Avaya = 0x01,
            FFSIP = 0x02,
            A5000 = 0x04,
            Mitel = 0x08,
            OC01XX = 0x10,
            OCX = 0x20,
        }

        public enum RfpType
        {
            RFP31 = 0x01,
            RFP33 = 0x02,
            RFP41 = 0x03,
            RFP32 = 0x04,
            RFP32US = 0x05,
            RFP34 = 0x06,
            RFP34US = 0x07,
            RFP42 = 0x08,
            RFP42US = 0x09,
            RFP35 = 0x0b,
            RFP36 = 0x0c,
            RFP43 = 0x0d,
            RFP37 = 0x0e,
            RFP44 = 0x10,
            RFP45 = 0x11,
            RFP47 = 0x12,
            RFP48 = 0x13,
            PC_ECM = 0x14,
            PC = 0x15,
            RFPL31 = 0x1001,
            RFPL33 = 0x1002,
            RFPL41 = 0x1003,
            RFPL32US = 0x1005,
            RFPL34 = 0x1006,
            RFPL34US = 0x1007,
            RFPL42 = 0x1008,
            RFPL42US = 0x1009,
            RFPL35 = 0x100B,
            RFPL36 = 0x100C,
            RFPL43 = 0x100D,
            RFPL37 = 0x100E,
            RFPSL35 = 0x200B,
            RFPSL36 = 0x200C,
            RFPSL43 = 0x200D,
            RFPSL37 = 0x200E,
        }

        [Flags]
        public enum RfpCapabilities : uint
        {
            None =             0b_00000000_00000000_00000000_00000000,
            Reserved1 =        0b_00000000_00000000_00000000_00000001,
            Reserved2 =        0b_00000000_00000000_00000000_00000010,
            Reserved3 =        0b_00000000_00000000_00000000_00000100,
            NormalTx =         0b_00000000_00000000_00000000_00001000,
            Indoor =           0b_00000000_00000000_00000000_00010000,
            Wlan =             0b_00000000_00000000_00000000_00100000,
            Reserved7 =        0b_00000000_00000000_00000000_01000000,
            Reserved8 =        0b_00000000_00000000_00000000_10000000,
            Encryption =       0b_00000000_00000000_00000001_00000000,
            FrequencyShift =   0b_00000000_00000000_00000010_00000000,
            LowTx =            0b_00000000_00000000_00000100_00000000,
            Reserved12 =       0b_00000000_00000000_00001000_00000000,
            Reserved13 =       0b_00000000_00000000_00010000_00000000,
            Reserved14 =       0b_00000000_00000000_00100000_00000000,
            Reserved15 =       0b_00000000_00000000_01000000_00000000,
            Reserved16 =       0b_00000000_00000000_10000000_00000000,
            WlanDfsSupported = 0b_00000000_00000001_00000000_00000000,
            Reserved18 =       0b_00000000_00000010_00000000_00000000,
            Reserved19 =       0b_00000000_00000100_00000000_00000000,
            Reserved20 =       0b_00000000_00001000_00000000_00000000,
            Reserved21 =       0b_00000000_00010000_00000000_00000000,
            Reserved22 =       0b_00000000_00100000_00000000_00000000,
            Reserved23 =       0b_00000000_01000000_00000000_00000000,
            Reserved24 =       0b_00000000_10000000_00000000_00000000,
            Reserved25 =       0b_00000001_00000000_00000000_00000000,
            Reserved26 =       0b_00000010_00000000_00000000_00000000,
            Reserved27 =       0b_00000100_00000000_00000000_00000000,
            Reserved28 =       0b_00001000_00000000_00000000_00000000,
            Reserved29 =       0b_00010000_00000000_00000000_00000000,
            Reserved30 =       0b_00100000_00000000_00000000_00000000,
            Reserved31 =       0b_01000000_00000000_00000000_00000000,
            Reserved32 =       0b_10000000_00000000_00000000_00000000,
            ConfigurableTX = NormalTx | LowTx,
            AdvancedFeature =  Reserved12 | Reserved14 | Reserved15| Reserved16
        }

        public RfpType Hardware { get; }

        public ReadOnlyMemory<byte> Reserved1 { get; }

        public PhysicalAddress Mac { get; }

        public ReadOnlyMemory<byte> Reserved2 { get; }

        public RfpCapabilities Capabilities { get; }

        public byte[] Crypted { get; private set; }
        
        public byte[] Plain { get; private set; }

        public uint Crc32 { get; private set; }

        public ulong Magic { get; }

        public RfpBranding Branding { get; }

        public PhysicalAddress Mac2 { get; }

        public ReadOnlyMemory<byte> Reserved3 { get; }

        public uint Protocol { get; }

        public ReadOnlyMemory<byte> Reserved4 { get; }

        public string SwVersion { get; }

        public ReadOnlyMemory<byte> Signature { get; private set; }

        public override bool HasUnknown => true;

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(Protocol > 0x080000?0x110:0xF4);

        public override ushort Length => (ushort) (base.Length + 0x110u);

        public SysInitMessage(PhysicalAddress mac, RfpCapabilities capabilities) : base(MsgType.SYS_INIT)
        {
            Mac = mac;
            Hardware = RfpType.RFP35;
            Mac2 = mac;
            Magic = 0x00037a20000529d9;
            Protocol = 0x080303u;
            Capabilities = capabilities;
            SwVersion = "SIP-DECT 8.3-eventphone";
        }

        public SysInitMessage(ReadOnlyMemory<byte> data):base(MsgType.SYS_INIT, data)
        {
            Hardware = (RfpType) BinaryPrimitives.ReadInt32BigEndian(base.Raw.Span);
            Reserved1 = base.Raw.Slice(0x04, 0x04);//protocol?
            Mac = new PhysicalAddress(base.Raw.Slice(0x08, 0x06).ToArray());
            Reserved2 = base.Raw.Slice(0x0e, 0x06);
            Capabilities = (RfpCapabilities) BinaryPrimitives.ReadUInt32BigEndian(base.Raw.Slice(0x14).Span);
            Crypted = base.Raw.Slice(0x18, 0x40).ToArray();
            Protocol = BinaryPrimitives.ReadUInt32BigEndian(base.Raw.Slice(0x58, 0x04).Span);
            Reserved4 = base.Raw.Slice(0x5c, 0x08);

            Plain = new byte[Crypted.Length];
            AesDecrypt();

            var plain = Plain.AsSpan();
            Magic = BinaryPrimitives.ReadUInt64BigEndian(plain);
            plain = plain.Slice(8);
            Mac2 = new PhysicalAddress(plain.Slice(0, 6).ToArray());
            plain = plain.Slice(6);
            Branding = (RfpBranding) (BinaryPrimitives.ReadUInt16LittleEndian(plain) & 0x3ffu);
            Reserved3 = Plain.AsMemory().Slice(16, 44);
            Crc32 = BinaryPrimitives.ReadUInt32BigEndian(Plain.AsSpan().Slice(60));

            int offset = 0x64;
            int length = 0x80;
            if (Protocol > 0x080000u)
            {
                offset = 0x70;
                length = 0x90;
            }

            SwVersion = base.Raw.Slice(offset, length).Span.CString();
            Signature = base.Raw.Slice(offset + length, 0x10);
        }

        public override Span<byte> Serialize(Span<byte> data)
        {
            data =  base.Serialize(data);
            BinaryPrimitives.WriteInt32BigEndian(data, (int) Hardware);
            Reserved1.Span.CopyTo(data.Slice(4));
            Mac.GetAddressBytes().CopyTo(data.Slice(0x08));
            BinaryPrimitives.WriteUInt32BigEndian(data.Slice(0x14), (uint) Capabilities);

            Plain = new byte[0x40];
            var plain = Plain.AsSpan();
            BinaryPrimitives.WriteUInt64BigEndian(plain, Magic);
            Mac2.GetAddressBytes().CopyTo(plain.Slice(8));
            BinaryPrimitives.WriteUInt16LittleEndian(plain.Slice(14), (ushort)Branding);
            Crc32 = CalculateCrc32(plain.Slice(0, 60));
            BinaryPrimitives.WriteUInt32BigEndian(plain.Slice(60), Crc32);
            Crypted = new byte[0x40];
            AesEncrypt();
            Crypted.CopyTo(data.Slice(0x18));

            BinaryPrimitives.WriteUInt32BigEndian(data.Slice(0x58), Protocol);
            Encoding.ASCII.GetBytes(SwVersion).CopyTo(data.Slice(0x70));
            Signature.Span.CopyTo(data.Slice(0x100));
            return data.Slice(0x110);
        }

        public void Sign(ReadOnlySpan<byte> sysAuth)
        {
            sysAuth = sysAuth.Slice(4);
            using (var md5 = MD5.Create())
            {
                var data = new byte[sysAuth.Length + Length - 0x10 + _signatureKey.Length];
                sysAuth.CopyTo(data);
                Serialize(data.AsSpan(sysAuth.Length));
                _signatureKey.CopyTo(data.AsSpan(sysAuth.Length + Length - 0x10));
                var hash = md5.ComputeHash(data);
                Signature = hash;
            }
        }

        private void AesDecrypt()
        {
            using (var aes = new AesManaged())
            {
                aes.KeySize = 256;
                aes.Padding = PaddingMode.None;
                aes.Key = AesKey;
                aes.Mode = CipherMode.ECB;
                
                using (var dec = aes.CreateDecryptor())
                using (var ms = new MemoryStream(Plain))
                using (var s = new CryptoStream(ms, dec, CryptoStreamMode.Write))
                {
                    s.Write(Crypted);
                }
            }
        }

        private void AesEncrypt()
        {
            using (var aes = new AesManaged())
            {
                aes.KeySize = 256;
                aes.Padding = PaddingMode.None;
                aes.Key = AesKey;
                aes.Mode = CipherMode.ECB;
                
                using (var enc = aes.CreateEncryptor())
                using (var ms = new MemoryStream(Crypted))
                using (var s = new CryptoStream(ms, enc, CryptoStreamMode.Write))
                {
                    s.Write(Plain);
                }
            }
        }

        private uint CalculateCrc32(ReadOnlySpan<byte> data)
        {
            var result = 0u;
            for (int i = 0; i < data.Length; i++)
            {
                var b = data[i];
                result = CrcTable[b ^ (result & 0xff)] ^ (result >> 8);
            }

            result = CrcTable[data.Length ^ (result & 0xff)] ^ (result >> 8);
            return BinaryPrimitives.ReverseEndianness(~result);
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Hardware({Hardware:G}) Reserved1({Reserved1.ToHex()}) MAC({Mac}) ");
            writer.Write($"Reserved2({Reserved2.ToHex()}) Capabilities({Capabilities}) ");
            writer.Write($"Magic({Magic:x16}) Mac2({Mac2}) Branding({Branding}) ");
            writer.Write($"Reserved3({Reserved3.ToHex()}) Crc({Crc32:x8}) Protocol({Protocol:x8}) Reserved4({Reserved4.ToHex()}) ");
            writer.Write($"SW Version({SwVersion}) Signature({Signature.ToHex()}) ");
        }
    }
}