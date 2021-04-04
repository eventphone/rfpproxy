using System;
using System.Buffers.Binary;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using RfpProxy.AaMiDe.Sys;
using RfpProxyLib;
using Xunit;
using Xunit.Abstractions;

namespace RfpProxy.Test
{
    public class CryptoTest
    {
        private readonly ITestOutputHelper _output;

        private CryptedRfpConnection _connection;

        public CryptoTest(ITestOutputHelper output)
        {
            _output = output;
            
            _connection = new CryptedRfpConnection(null, null);
            
            var sysinit = "012d0020de535f6d6c10093004e042b9a198b435db25a8222436bd001b1f5f340218cffc ";
            var baseiv_tx_hex = sysinit.Substring(11*2, 8*2);
            var baseiv_rx_hex = sysinit.Substring(27*2, 8*2);

            var baseiv_rx = HexEncoding.HexToByte(baseiv_rx_hex);
            var baseiv_tx = HexEncoding.HexToByte(baseiv_tx_hex);
            
            _connection.InitOmmToRfpIv(baseiv_rx);
            _connection.InitRfpToOmmIv(baseiv_tx);
        }

        [Fact]
        public void CanDecrypt()
        {
            var hex = "d427a873ed113a2a57eb9dbfe6e0eb7d";
            var crypted = HexEncoding.HexToByte(hex);
            var plain = _connection.DecryptOmmToRfp(crypted, _connection.OmmToRfpIv);
            var plainhex = plain.ToHex();
            _output.WriteLine(plainhex);
            Assert.Equal("010c00000101000801010008b8b82006", plainhex);

            var recrypted = _connection.CryptOmmToRfp(plain);
            var rehex = recrypted.ToHex();
            Assert.Equal(hex, rehex);
        }

        [Fact]
        public void CanCalculateInitSignature()
        {
            var sysInit = "01200110 0000000d 00080100 0030421b 17370000 00000000 0001ef3c 2a0d8ce8 " +
                          "725371d0 d799a029 8dc02c73 4ac5b803 abc38663 b494de7b 2ffbe03d 70b616eb " +
                          "facf2e7d 85f61b29 5cba5c76 ea515501 b3c02b75 5862261b fc08ffde 00080201 " +
                          "00000000 00000000 00000000 00000000 00000000 5349502d 44454354 20382e31 " +
                          "5350312d 46413237 00000000 00000000 00000000 00000000 00000000 00000000 " +
                          "00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000 " +
                          "00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000 " +
                          "00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000 " +
                          "00000000";
            sysInit = sysInit.Replace(" ", String.Empty);
            var signature_key = "e7adda3adb0521f3d3fbdf3a18ee8648b47398b1570c2b45ef8d2a9180a1a32c69284a9c97d444abf87f5c578f9428214dd0183cba969dc5";
            var sysAuthenticate = "a367fa2bd9d02c246d639afccfb1569a3240dd4027434430b71b4a7fe8ba62b7";
            using (var md5 = MD5.Create())
            {
                var data = HexEncoding.HexToByte(sysAuthenticate + sysInit + signature_key);
                var hash = md5.ComputeHash(data);
                _output.WriteLine(hash.AsSpan().ToHex());
                Assert.Equal("43b305cc65719045766e14b7ba0ba319", hash.AsSpan().ToHex());
            }
        }

        [Fact]
        public void CanDecryptSysInit()
        {
            using (var aes = new AesManaged())
            {
                var key = "e705bc1a92412f3262c547f87946936997e690ada46fad25bbc626f6f5a5a6ce";
                var ciphered = "705bb6102d8f709f94ad5ba7382bdf0ce136e535c109c1e9b2ffed6c5ced5fe4" +
                               "0cc2c769d9e6b9597447531e2731f107f15931a66359b1b465d3ae192bbbb614";
                ciphered = ciphered.Replace(" ", String.Empty);
                aes.KeySize = 256;
                aes.Padding = PaddingMode.None;
                aes.Key = HexEncoding.HexToByte(key);
                aes.IV = new byte[128/8];
                aes.Mode = CipherMode.ECB;
                var data = HexEncoding.HexToByte(ciphered);
                var plain = new byte[data.Length];
                using (var dec = aes.CreateDecryptor())
                using (var ms = new MemoryStream(plain))
                using (var s = new CryptoStream(ms, dec, CryptoStreamMode.Write))
                {
                    s.Write(data);
                    _output.WriteLine(plain.AsSpan().ToHex());
                    _output.WriteLine(crc32(ms.ToArray()).ToString("x8"));
                    
                }
            }

            uint crc32(byte[] data)
            {
                var result = 0u;
                int i = 0;
                uint[] crc_table = {
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
                    0x6D66B4BC, 0xDA7B75B8, 0x035D36B5, 0xB440F7B1,
                };
                for (int j = 0; j < crc_table.Length; j++)
                {
                    crc_table[j] = (crc_table[j] & 0x000000FFU) << 24 | (crc_table[j] & 0x0000FF00U) << 8 |
                                       (crc_table[j] & 0x00FF0000U) >> 8 | (crc_table[j] & 0xFF000000U) >> 24;
                }

                do
                {
                    var index = data[i] ^ (result >> 24);
                    result = crc_table[index] ^ (result << 8);
                    i++;
                } while (i != 60);
                i = 60;
                do
                {
                    var index = (byte) (i ^ (byte) (result >> 24));
                    var v6 = crc_table[index] ^ (result << 8);
                    i >>= 8;
                    result = v6;
                } while (i != 0);
                return ~result;
            }
        }

        [Fact]
        public void InitKeys()
        {
            var table = new byte[]
            {
                0x4A, 0x10, 0x42, 0x63, 0x37, 0x0C, 0xE7,
                0x4A, 0x5A, 0x98, 0x4E, 0xA7, 0x03, 0x36,
                0xCE, 0x45, 0xBE, 0x0F, 0x0E, 0x61, 0xC0,
                0xAF, 0x6E, 0xAB, 0x26, 0xD2, 0xD2, 0x5D,
                0x4B, 0x8D, 0x0B, 0x0C, 0xE6, 0x2F, 0x05,
                0x31, 0x9A, 0xA5, 0x70, 0x40, 0xFB, 0xD4,
                0xA8, 0x4D, 0x53, 0xB2, 0x41, 0xDF, 0x3B,
                0xB5, 0x51, 0x9F, 0xC1, 0x1F, 0xC0, 0x99,

                0xAD, 0xBD, 0x98, 0x59, 0xEC, 0x09, 0xC6, 
                0xB9, 0x89, 0x63, 0x91, 0x9D, 0x1B, 0xD8,
                0x48, 0x0D, 0x0A, 0x7C, 0x96, 0xD0, 0x97,
                0xA3, 0x45, 0xEE, 0xC9, 0x5F, 0xF8, 0xCC, 
                0xCB, 0x2C, 0xA8, 0x20, 0x8F, 0x07, 0x4F,
                0xAD, 0x0D, 0x71, 0x34, 0xEB, 0x03, 0xAB,
                0xF4, 0x1A, 0xDC, 0x26, 0x69, 0xFE, 0x76, 
                0x65, 0x49, 0xA3, 0x7B, 0x89, 0x5D, 0x5C
            };
            var signature_key = new byte[56];
            for (int i = 0; i < 56; i++)
            {
                signature_key[i] = (byte) (table[i] ^ table[i + 56]);
            }
            _output.WriteLine(signature_key.AsSpan().ToHex());

            var omm_key = new byte[56];
            var rfp_key = new byte[56];

            byte[] rekeyTable = {

                0x38, 0x63, 0x74, 0xF2, 0xFC, 0xC3, 0xA5, 0x63,
                0x47, 0xE4, 0xEB, 0x35, 0x0F, 0xF8, 0x24, 0x6A,
                0x36, 0x8A, 0x4B, 0x3B, 0x28, 0x59, 0x04, 0x72,
                0x88, 0x3C, 0x8D, 0x38, 0xA9, 0x95, 0xE4, 0x4D,
                0xFD, 0x17, 0x8E, 0x72, 0x68, 0x14, 0x55, 0x87,
                0xB0, 0x50, 0x29, 0x33, 0x78, 0xF4, 0x90, 0xF6,
                0x80, 0x45, 0x64, 0xBC, 0xA2, 0x10, 0x3B, 0x28,
            
                0xA5, 0xAB, 0xF4, 0x35, 0x3E, 0x19, 0x35, 0x2F,
                0xC3, 0xF1, 0xFE, 0xFF, 0x3B, 0x47, 0x5A, 0xF6,
                0x67, 0x5E, 0x56, 0x90, 0x95, 0xD7, 0x9A, 0x75,
                0x33, 0xD7, 0xC8, 0x46, 0x5D, 0x34, 0x89, 0xC5,
                0xB2, 0x6A, 0x4A, 0xF6, 0x7C, 0xC5, 0xBF, 0xD7,
                0xAC, 0x27, 0x92, 0xC4, 0x44, 0xAC, 0xFC, 0xBB,
                0xE8, 0x0E, 0xD5, 0x3A, 0x30, 0x56, 0xF3, 0xBD,
            
                0xB3, 0xD3, 0xBB, 0xC3, 0x33, 0x80, 0x3C, 0x12,
                0x4B, 0x12, 0x5B, 0xE9, 0x21, 0x80, 0x39, 0xB0,
                0xC4, 0xE3, 0x98, 0x75, 0xF4, 0x2F, 0xE2, 0xC6,
                0x66, 0x0A, 0x19, 0xAA, 0x53, 0xE0, 0x05, 0x65,
                0xE3, 0x18, 0x46, 0x34, 0x0B, 0x86, 0xB8, 0xFF,
                0x9C, 0x7C, 0x00, 0xCE, 0x67, 0x96, 0x0C, 0x50,
                0x4C, 0x71, 0x41, 0xDF, 0x7A, 0x19, 0xB2, 0xDC,
            
                0x34, 0x33, 0x42, 0x70, 0xBA, 0xA7, 0xCB, 0x31,
                0x5E, 0x53, 0xA1, 0xF0, 0xE3, 0x62, 0xE7, 0xC6,
                0xED, 0x08, 0x0E, 0xBD, 0xAA, 0x53, 0x9B, 0x8B,
                0x48, 0xAF, 0x44, 0x4C, 0x5B, 0x04, 0xAB, 0x62,
                0x2C, 0xEC, 0x77, 0x96, 0x6C, 0x3E, 0xC1, 0xF5,
                0xAA, 0xBA, 0xE4, 0xD2, 0x46, 0x6E, 0x39, 0x5C,
                0x3B, 0xF6, 0x5F, 0xC9, 0xFD, 0x81, 0xC1, 0xC3,
            };
            for (int i = 0; i < 56; i++)
            {
                omm_key[i] = (byte) (rekeyTable[i] ^ rekeyTable[i + 56]);
                rfp_key[i] = (byte) (rekeyTable[i + 112] ^ rekeyTable[i + 168]);
            }
            _output.WriteLine(omm_key.AsSpan().ToHex());
            _output.WriteLine(rfp_key.AsSpan().ToHex());

            var aes_key = new byte[32];
            table = new byte[]
            {
                0x42, 0x05, 0xBC, 0x1A, 0x92, 0xE4, 0x2F, 0x32, 0x62, 0xC5, 0xE2, 0xF8, 0x79, 0x46, 0x93, 0xCC,
                0x97, 0xE6, 0x90, 0xAD, 0x01, 0x6F, 0xAD, 0x25, 0xBB, 0x63, 0x26, 0xF6, 0xF5, 0xA5, 0x03, 0xCE
            };
            for(int i = 0; i < 32; i++)
            {
                var v22 = table[i];
                if ( i == 5 * (i/ 5) )
                    v22 = (byte) (v22 ^ 0xA5u);
                aes_key[i] = v22;
            }
            _output.WriteLine(aes_key.AsSpan().ToHex());
        }

        [Fact]
        public void CanInitIV()
        {
            var omm_iv = HexEncoding.HexToByte("7165E6DF425AC4FA");
            BinaryPrimitives.WriteUInt32LittleEndian(omm_iv, BinaryPrimitives.ReadUInt32BigEndian(omm_iv));
            BinaryPrimitives.WriteUInt32LittleEndian(omm_iv.AsSpan(4), BinaryPrimitives.ReadUInt32BigEndian(omm_iv.AsSpan(4)));
            ulong byte_88030F0 = 0x6e_15_de_7f_12_a7_09_1f;
            ulong byte_88030DF = 0xb1_f3_bb_0e_e8_63_53_5d;
            var omm_init_iv = byte_88030DF ^ byte_88030F0;
            _output.WriteLine("{0:x16}", byte_88030F0);
            _output.WriteLine("{0:x16}", byte_88030DF);
            _output.WriteLine("{0:x16}", omm_init_iv);
            Assert.Equal(omm_iv.AsSpan().ToHex(), omm_init_iv.ToString("x16"));

            var rfp_iv = HexEncoding.HexToByte("4B36E868C134C2E9");
            BinaryPrimitives.WriteUInt32LittleEndian(rfp_iv, BinaryPrimitives.ReadUInt32BigEndian(rfp_iv));
            BinaryPrimitives.WriteUInt32LittleEndian(rfp_iv.AsSpan(4), BinaryPrimitives.ReadUInt32BigEndian(rfp_iv.AsSpan(4)));
            ulong byte_88030E7 = 0xb1_f7_31_44_c7_13_95_d0;
            ulong byte_88030F7 = 0xd9_1f_07_0f_2e_d1_a1_11;
            var rfp_init_iv = byte_88030E7 ^ byte_88030F7;
            _output.WriteLine("{0:x16}", byte_88030E7);
            _output.WriteLine("{0:x16}", byte_88030F7);
            _output.WriteLine("{0:x16}", rfp_init_iv);
            Assert.Equal(rfp_iv.AsSpan().ToHex(), rfp_init_iv.ToString("x16"));
        }

        [Fact]
        public void CanDecryptRFPA()
        {
            var rfpa_crypted = "45C2DD259C4B368DC86F972E62180635A3241099DB76FE8C0B10922D0838EE254C88543C4981B58236122E9AC35297038CBD8EDE4926F2D6519167EA2A64FD0E";
            var rfpa_plain = "6e9dda8b0e300ce29d41990c1c4e43b9e13ef2374e491511699684fbbbb283c325a72b5e6e558365063e37f694213b80309f3b1053bb398bc4ea78a1baa8705d";
            var mac = "0030421B1737\0";

            mac = mac.ToLowerInvariant();

            var crypted = HexEncoding.HexToByte(rfpa_crypted);
            HexEncoding.SwapEndianess(crypted);

            var bf = new BlowFish(Encoding.ASCII.GetBytes(mac));
            var plain = bf.Decrypt_ECB(crypted);

            HexEncoding.SwapEndianess(plain.Span);

            _output.WriteLine(rfpa_crypted);
            _output.WriteLine(crypted.AsSpan().ToHex());
            _output.WriteLine(plain.ToHex());

            Assert.Equal(rfpa_plain, plain.ToHex());
        }

        [Fact]
        public void CanDecryptWithIndividualKey()
        {
            var key = "6e9dda8b0e300ce29d41990c1c4e43b9e13ef2374e491511699684fbbbb283c325a72b5e6e558365063e37f694213b80309f3b1053bb398bc4ea78a1baa8705d";
            var omm_key = key.Substring(0, 56*2);
            var key2 = key.Substring(8*2);
            var omm_bf = new BlowFish(HexEncoding.HexToByte(omm_key));
            var rfp_bf = new BlowFish(HexEncoding.HexToByte(key2));
            var auth = new SysAuthenticateMessage(HexEncoding.HexToByte("012d00205356c49560e597d6445f49dce9a8219eb3f10cf055e24b291c867ce128421361"));
            var ommdata = "81add1586bdc7bd9e9c4f3836eaa70877a51a8e4288ca34df49c602496d64924" +
                         "5213eb56376060949eda9f373dc3ab7a806bf97a9f072579a5ed79c2a67a8a78" +
                         "c9bd1f291247db8879add3ce9849ecdf3fcec198ddc4b8105b99bba78ad41df2" +
                         "52b5485036532e8a66c9ad2be5b9d3194bbd8b7c5dc351ceb82f30668f5ef60d" +
                         "2039db815242095f5c7bb23f257e47e8013aa8df7aea53a25b77cfe437b92a91" +
                         "053e31dbc8e99adc88311c6b8800275d56ee4d8be1988688391d113c753654e2" +
                         "4ada0060822754fb801ae266ab7081db0dc91f933bbb592977d69fd5f794dde0" +
                         "32d0384597e0e3f3b97ea0accd41d9d3baf76b4477706a12cb827242c2183c5d" +
                         "bb677584d824c52fe389d2ac54f27fc97675ab201e44475c97b62ceec665ab01" +
                         "c10faae08a2aa955b59e11f8c844171d73c078e89045c4c5a4dc06090ed1fc62" +
                         "fb75ccbc302d064ae2d7ceedf577af8afcd38b612f05f671eebac58a7dc594b1" +
                         "d947e47f59838a7a430cd6984d226c0af06867849abcd81e1154c2c5461a520f" +
                         "5acd5b2193687305a8d3cd49d684ad86300aaa44326e059b98e66fbdc4d4316c" +
                         "e8c99216356424bc977294f7ffe9ebb782f91b5cc5ca253153716b199944555c" +
                         "8f842dc37d8ac1e1";

            var rfpdata = "9f723e0ee29e7a5848556fcc94f8c446d2acdad3bd472ceb37440c8c94554e31";

            var iv = HexEncoding.HexToByte("dfe66571fac45a42");
            BlowFish.XorBlock(iv, auth.OmmIv.Span);
            var plain = omm_bf.Decrypt_CBC(iv, HexEncoding.HexToByte(ommdata));
            _output.WriteLine(plain.ToHex());

            iv = HexEncoding.HexToByte("68e8364be9c234c1");
            BlowFish.XorBlock(iv, auth.RfpIv.Span);
            plain = rfp_bf.Decrypt_CBC(iv, HexEncoding.HexToByte(rfpdata));
            _output.WriteLine(plain.ToHex());
        }
    }
}
