using System;
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

            var baseiv_rx = BlowFish.HexToByte(baseiv_rx_hex);
            var baseiv_tx = BlowFish.HexToByte(baseiv_tx_hex);
            
            _connection.InitOmmToRfpIv(baseiv_rx);
            _connection.InitRfpToOmmIv(baseiv_tx);
        }

        [Fact]
        public void CanDecrypt()
        {
            var hex = "d427a873ed113a2a57eb9dbfe6e0eb7d";
            var crypted = BlowFish.HexToByte(hex);
            var plain = _connection.DecryptOmmToRfp(crypted, _connection.OmmToRfpIv);
            var plainhex = BlowFish.ByteToHex(plain.Span);
            _output.WriteLine(plainhex);
            Assert.Equal("010c00000101000801010008b8b82006", plainhex);

            var recrypted = _connection.CryptOmmToRfp(plain);
            var rehex = BlowFish.ByteToHex(recrypted.Span);
            Assert.Equal(hex, rehex);
        }
    }
}
