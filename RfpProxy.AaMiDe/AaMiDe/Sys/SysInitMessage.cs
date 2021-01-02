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

        private readonly byte[] _signatureKey = HexEncoding.HexToByte(
            "e7adda3adb0521f3d3fbdf3a18ee8648" +
            "b47398b1570c2b45ef8d2a9180a1a32c" +
            "69284a9c97d444abf87f5c578f942821" +
            "4dd0183cba969dc5");

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
            None = 0b_00000000_00000000_00000000_00000000,
            Reserved1 = 0b_00000000_00000000_00000000_00000001,
            Reserved2 = 0b_00000000_00000000_00000000_00000010,
            Reserved3 = 0b_00000000_00000000_00000000_00000100,
            NormalTx = 0b_00000000_00000000_00000000_00001000,
            Indoor = 0b_00000000_00000000_00000000_00010000,
            Wlan = 0b_00000000_00000000_00000000_00100000,
            Reserved7 = 0b_00000000_00000000_00000000_01000000,
            Reserved8 = 0b_00000000_00000000_00000000_10000000,
            Encryption = 0b_00000000_00000000_00000001_00000000,
            FrequencyShift = 0b_00000000_00000000_00000010_00000000,
            LowTx = 0b_00000000_00000000_00000100_00000000,
            Reserved12 = 0b_00000000_00000000_00001000_00000000,
            Reserved13 = 0b_00000000_00000000_00010000_00000000,
            Reserved14 = 0b_00000000_00000000_00100000_00000000,
            Reserved15 = 0b_00000000_00000000_01000000_00000000,
            AdvancedFeature = 0b_00000000_00000000_10000000_00000000,
            Reserved17 = 0b_00000000_00000001_00000000_00000000,
            Reserved18 = 0b_00000000_00000010_00000000_00000000,
            Reserved19 = 0b_00000000_00000100_00000000_00000000,
            Reserved20 = 0b_00000000_00001000_00000000_00000000,
            Reserved21 = 0b_00000000_00010000_00000000_00000000,
            Reserved22 = 0b_00000000_00100000_00000000_00000000,
            Reserved23 = 0b_00000000_01000000_00000000_00000000,
            Reserved24 = 0b_00000000_10000000_00000000_00000000,
            Reserved25 = 0b_00000001_00000000_00000000_00000000,
            Reserved26 = 0b_00000010_00000000_00000000_00000000,
            Reserved27 = 0b_00000100_00000000_00000000_00000000,
            Reserved28 = 0b_00001000_00000000_00000000_00000000,
            Reserved29 = 0b_00010000_00000000_00000000_00000000,
            Reserved30 = 0b_00100000_00000000_00000000_00000000,
            Reserved31 = 0b_01000000_00000000_00000000_00000000,
            Reserved32 = 0b_10000000_00000000_00000000_00000000,
            ConfigurableTX = NormalTx | LowTx,
        }

        public RfpType Hardware { get; }

        public ReadOnlyMemory<byte> Reserved1 { get; }

        public PhysicalAddress Mac { get; }

        public ReadOnlyMemory<byte> Reserved2 { get; }

        public RfpCapabilities Capabilities { get; }

        public ReadOnlyMemory<byte> Crypted { get; }
        
        public byte[] Plain { get; }

        public uint Crc32 { get; }

        public ulong Magic { get; }

        public RfpBranding Branding { get; }

        public PhysicalAddress Mac2 { get; }

        public ReadOnlyMemory<byte> Reserved3 { get; }

        public uint Protocol { get; }

        public ReadOnlyMemory<byte> Reserved4 { get; }

        public string SwVersion { get; }

        public ReadOnlyMemory<byte> Signature { get; private set; }

        public override bool HasUnknown => true;

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(0x110);

        public override ushort Length => (ushort) (base.Length + 0x110u);

        public SysInitMessage(PhysicalAddress mac, RfpCapabilities capabilities) : base(MsgType.SYS_INIT)
        {
            Mac = mac;
            Hardware = RfpType.RFP31;
            Protocol = 0x080201u;
            Capabilities = capabilities;
            SwVersion = "SIP-DECT 9.0-eventphone";
        }

        public SysInitMessage(ReadOnlyMemory<byte> data):base(MsgType.SYS_INIT, data)
        {
            Hardware = (RfpType) BinaryPrimitives.ReadInt32BigEndian(base.Raw.Span);
            Reserved1 = base.Raw.Slice(0x04, 0x04);
            Mac = new PhysicalAddress(base.Raw.Slice(0x08, 0x06).ToArray());
            Reserved2 = base.Raw.Slice(0x0e, 0x06);
            Capabilities = (RfpCapabilities) BinaryPrimitives.ReadUInt32BigEndian(base.Raw.Slice(0x14).Span);
            Crypted = base.Raw.Slice(0x18, 0x40);
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

            SwVersion = base.Raw.Slice(0x70, 0x90).Span.CString();
            Signature = base.Raw.Slice(0x100, 0x10);
        }

        public override Span<byte> Serialize(Span<byte> data)
        {
            data =  base.Serialize(data);
            BinaryPrimitives.WriteInt32BigEndian(data, (int) Hardware);
            Mac.GetAddressBytes().CopyTo(data.Slice(0x08));
            BinaryPrimitives.WriteUInt32BigEndian(data.Slice(0x14), (uint) Capabilities);
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
                _signatureKey.CopyTo(data.AsMemory(sysAuth.Length + Length - 0x10));
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
                    s.Write(Crypted.Span);
                }
            }
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