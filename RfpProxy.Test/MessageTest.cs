using System;
using System.IO;
using RfpProxy.Log.Messages;
using RfpProxyLib;
using Xunit;
using Xunit.Abstractions;

namespace RfpProxy.Test
{
    public class MessageTest
    {
        private readonly ITestOutputHelper _output;

        public MessageTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CanDecodeRfpInitReq()
        {
            var data = HexEncoding.HexToByte(("03010023" +
                                             "7802" +
                                             "04 05 102af12c26" +
                                             "06 02 ca42" +
                                             "07 05 0010000000" +
                                             "26 02 0000" +
                                             "0d 03 1003ff" +
                                             "27 01 1f  28 01 00").Replace(" ", String.Empty));

            var message = AaMiDeMessage.Create(data);
            var rfpc = Assert.IsType<DnmRfpcMessage>(message);
            Assert.Equal(DnmRfpcType.InitReq, rfpc.DnmType);

            Log(message);
        }

        [Fact]
        public void CanDecodeSysSnmpMessage()
        {
            var data = HexEncoding.HexToByte("0501014c" +
                                             "ac141701" +
                                             "62626262626262626262626262626262626262626262626262626262626262626262626262626262009f92b0f09f92b0f09f92b0f09f92b0f09f92b0f09f92b0f09f92b0f09f92b0f09f92b0f09f92b000" +
                                             "646562756744454354313233009f92b0f09f92b0f09f92b0f09f92b0f09f92b0f09f92b0f09f92b00000000000000000000000000000000000000000000000000000000000000000000000000000000000" +
                                             "524650203432202830303a33303a34323a30463a38323a323729000000000000000000000000000000000000000000000000890000001839361690be751600000000000000000000000000000000000000" +
                                             "6161616161616161616161616161616161616161009f92b0f09f92b0f09f92b0f09f92b0f09f92b000" +
                                             "6363636363636363636363636363636363636363000000000000000000000000000000000000000000010000");
            var message = AaMiDeMessage.Create(data);
            var snmp = Assert.IsType<SnmpRfpUpdateMessage>(message);
            Assert.Equal("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb", snmp.Contact);
            Assert.Equal("debugDECT123", snmp.Location);
            Assert.Equal("RFP 42 (00:30:42:0F:82:27)", snmp.Name);
            Assert.Equal("aaaaaaaaaaaaaaaaaaaa", snmp.RoCommunity);
            Assert.Equal("cccccccccccccccccccc", snmp.TrapCommunity);
            Assert.Equal("172.20.23.1", snmp.Server.ToString());
            Assert.True(snmp.TrapEnabled);
            
            Log(message);
        }

        [Fact]
        public void CanDecodeSysAuthenticateMessage()
        {
            var data = HexEncoding.HexToByte("012d0020" +
                                             "57b858bb227549d7" +
                                             "2215096317457eee" +
                                             "f01aa118ab156ad5" +
                                             "e8b35e55ab2b30a0");
            var message = AaMiDeMessage.Create(data);
            var auth = Assert.IsType<SysAuthenticateMessage>(message);
            Assert.Equal("2215096317457eee", HexEncoding.ByteToHex(auth.RfpIv.Span));
            Assert.Equal("e8b35e55ab2b30a0", HexEncoding.ByteToHex(auth.OmmIv.Span));

            Log(message);
        }

        private void Log(AaMiDeMessage message)
        {
            using (var writer = new StringWriter())
            {
                message.Log(writer);
                _output.WriteLine(writer.ToString());
            }
        }
    }
}