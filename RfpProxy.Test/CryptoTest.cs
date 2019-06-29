using System;
using System.Buffers.Binary;
using System.IO;
using System.Security.Cryptography;
using RfpProxyLib;
using Xunit;
using Xunit.Abstractions;

namespace RfpProxy.Test
{
    public class CryptoTest
    {
        private readonly ITestOutputHelper _output;

        private byte[] _ivTx;
        private byte[] _ivRx;
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
        public void CanDecryptAfterRekey()
        {
            _connection = new CryptedRfpConnection(null, null);
            var sysinit = "012d00209bc21bab186210cd8d619849d192e75f28e79d0744696a53ba172a916a48d877";
            var baseiv_tx_hex = sysinit.Substring(11*2, 8*2);
            var baseiv_tx = HexEncoding.HexToByte(baseiv_tx_hex);
            _connection.InitRfpToOmmIv(baseiv_tx);

            var blowfish = new BlowFish("87E0F9B38927F7231541FA19C2E2DE7629EB96C85E7C794D2EA55DE608E4AE07CFF431A267B8790A36C6E41C21F8350C77871E168798731F");
            var plain = blowfish.Decrypt_CBC(_connection.RfpToOmmIv.Span, HexEncoding.HexToByte("f2133e35cb05e2ab"));
            Assert.Equal("0117001000000000", plain.ToHex());

            Decrypt(blowfish, "19bd7633111aee81", "c2ecf097d56210a8c9688232a2c11c33", "030100087906070003236101eeeeeeee");
            Decrypt(blowfish, "c9688232a2c11c33", "d6f0cf994acd7ccc", "0301000479070710");
            Decrypt(blowfish, "d6f0cf994acd7ccc", "a428979598d57473", "0301000479070700");
            
            var rekeyed = _connection.Rekey(HexEncoding.HexToByte("a4289795"));
            Decrypt(rekeyed, "a428979598d57473", "04aa5061fabfc098", "0301000879060700");
        }

        [Fact]
        public void CanDecryptWithRekey()
        {
            var blowfish = new BlowFish("87E0F9B38927F7231541FA19C2E2DE7629EB96C85E7C794D2EA55DE608E4AE07CFF431A267B8790A36C6E41C21F8350C77871E168798731F");
            var sysinit = "012d00209bc21bab186210cd8d619849d192e75f28e79d0744696a53ba172a916a48d877";
            var baseiv_tx_hex = sysinit.Substring(11*2, 8*2);
            var baseiv_tx = HexEncoding.HexToByte(baseiv_tx_hex);
            _connection = new CryptedRfpConnection(null, null);
            _connection.InitRfpToOmmIv(baseiv_tx);
            var iv = _connection.RfpToOmmIv.Span;

            var data = File.ReadAllBytes("rekey.bin").AsSpan();
            var type = BinaryPrimitives.ReadInt16BigEndian(data);
            var length = BinaryPrimitives.ReadInt16BigEndian(data.Slice(2));
            Assert.True(type <= 0x8FF, "invalid type");
            Assert.True(length <= data.Length, "invalid length");
            data = data.Slice(4).Slice(length);
            int packetcounter = 1;
            while (data.Length > 8)
            {
                var block = data.Slice(0, 8);
                var header = blowfish.Decrypt_CBC(iv, block);
                type = BinaryPrimitives.ReadInt16BigEndian(header.Span);
                length = BinaryPrimitives.ReadInt16BigEndian(header.Span.Slice(2));

                Assert.True(type <= 0x8FF, $"invalid type at {packetcounter}");
                Assert.True(length <= data.Length, $"invalid length at {packetcounter}");
                var lengthOf8 = (length + 4 + 7) & ~7;
                var packet = data.Slice(0, lengthOf8);
                iv = data.Slice(packet.Length - 8, 8);
                data = data.Slice(lengthOf8);
                packetcounter++;
                if (packetcounter > 2500)
                {
                    packetcounter = 0;
                    blowfish = _connection.Rekey(packet.Slice(0, 4));
                }
            }
            Assert.Equal(0, data.Length);
            _output.WriteLine($"type: 0x{type:x4} length: 0x{length:x4}");
        }

        private void Decrypt(BlowFish blowfish, string iv, string data, string plain)
        {
            var ivb = HexEncoding.HexToByte(iv);
            var plainb = blowfish.Decrypt_CBC(ivb, HexEncoding.HexToByte(data));
            Assert.Equal(plain, plainb.ToHex());
        }

        [Fact]
        public void RandR()
        {
            Assert.Equal(1430195325u, StdLib.RandR(3));
            Assert.Equal(681191333u, StdLib.RandR(42));
            Assert.Equal(2063792036u, StdLib.RandR(99));
            uint r = 42;
            Assert.Equal(681191333u, StdLib.RandR(ref r));
            Assert.NotEqual(42u, r);
        }

        [Fact]
        public void CanCalculateInitSignature()
        {
            var sysInit = "01200104 00000004 00070100 00304212 eb330000 00000000 000003dc 809d5e35" +
                       "4b6d6b6b 5360bc0a b470a9f4 0c510b8d d58eb3f7 ebf8894e c768f5b6 9d08e45b" +
                       "13fbb03f b22b75a2 eca3156b a1330ff4 2f1fc8a6 8bfc5302 91fb189b 00070100" +
                       "00000000 00000000 5349502d 44454354 20372e31 2d434b31 34000000 00000000" +
                       "00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000" +
                       "00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000" +
                       "00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000000" +
                       "00000000 00000000 00000000 00000000 00000000 00000000";
            sysInit = sysInit.Replace(" ", String.Empty);
            var signature_key = "096ba9879890b67a1893e397b977f3af3757b9edba6786e4ae267a80c8409707672522c07bc3ea2cfd4a58e6f88a80c7ca8d2b2eb2c20fad";
            var sysAuthenticate = "1c42339d625cf222ecce069e6098d252b2f2148381fc466f2d67a1a587aaf570";
            using (var md5 = MD5.Create())
            {
                var data = HexEncoding.HexToByte(sysAuthenticate + sysInit + signature_key);
                var hash = md5.ComputeHash(data);
                Assert.Equal("fbd0c9b2a76cade42bb263adcfe5317b", hash.AsSpan().ToHex());
                _output.WriteLine(hash.AsSpan().ToHex());
            }
        }

        [Fact]
        public void InitKeys()
        {
            var table = new byte[]
            {
                0x4F, 0x4F, 0xA6, 0xD0, 0x72, 0x2E, 0x65,
                0x7B, 0x1C, 0x50, 0x27, 0xB3, 0x85, 0x06,
                0x9E, 0xF4, 0xFB, 0x1A, 0xA9, 0x1F, 0x2F,
                0x92, 0x07, 0x64, 0x4D, 0xF8, 0xBB, 0x84,
                0xF0, 0xAF, 0x18, 0x97, 0xAF, 0xA1, 0x54,
                0xD1, 0xC1, 0x8F, 0x07, 0x30, 0x76, 0xCC,
                0x1D, 0xD9, 0x58, 0xA9, 0x64, 0x92, 0x62,
                0x9F, 0xE4, 0x19, 0x48, 0x99, 0x34, 0x66,

                0x46, 0x24, 0x0F, 0x57, 0xEA, 0xBE, 0xD3,
                0x01, 0x04, 0xC3, 0xC4, 0x24, 0x3C, 0x71,
                0x6D, 0x5B, 0xCC, 0x4D, 0x10, 0xF2, 0x95,
                0xF5, 0x81, 0x80, 0xE3, 0xDE, 0xC1, 0x04,
                0x38, 0xEF, 0x8F, 0x90, 0xC8, 0x84, 0x76,
                0x11, 0xBA, 0x4C, 0xED, 0x1C, 0x8B, 0x86,
                0x45, 0x3F, 0xA0, 0x23, 0xE4, 0x55, 0xA8,
                0x12, 0xCF, 0x37, 0xFA, 0x5B, 0x3B, 0xCB
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
        }
    }
}
