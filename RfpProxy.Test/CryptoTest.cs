using System;
using System.Buffers.Binary;
using System.IO;
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
            var plainhex = HexEncoding.ByteToHex(plain.Span);
            _output.WriteLine(plainhex);
            Assert.Equal("010c00000101000801010008b8b82006", plainhex);

            var recrypted = _connection.CryptOmmToRfp(plain);
            var rehex = HexEncoding.ByteToHex(recrypted.Span);
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
            Assert.Equal("0117001000000000", HexEncoding.ByteToHex(plain.Span));

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
            Assert.Equal(plain, HexEncoding.ByteToHex(plainb.Span));
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
    }
}
