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

        [Fact]
        public void CanDecodeSysInitMessage()
        {
            var data = HexEncoding.HexToByte("01200104" +
                                             "0000000800070100" +
                                             "0030420f8227" +
                                             "0000000000000000" +
                                             "03fc" +
                                             "a9b9df00301bd287ea0373c9f7869951d6fa651ccbdf21e665488dd8d84e52e805da3272c066522501360cffe09efbad5393d713ad9a19874c2496ae5c629b69000701000000000000000000" +
                                             "5349502d4445435420372e312d434b313400000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000" +
                                             "3720690a8f6d49be" +
                                             "756b4815d44020ee");
            var message = AaMiDeMessage.Create(data);
            var init = Assert.IsType<SysInitMessage>(message);
            Assert.Equal("0030420F8227", init.Mac.ToString());
            Assert.Equal(0x3fcu, init.Capabilities);
            Assert.Equal("SIP-DECT 7.1-CK14", init.SwVersion);

            Log(message);
        }

        [Fact]
        public void CanDecodeAckMessage()
        {
            var data = HexEncoding.HexToByte("00010008" +
                                             "0120ffff01000000");
            var message = AaMiDeMessage.Create(data);
            var ack = Assert.IsType<AckMessage>(message);
            Assert.Equal(MsgType.SYS_INIT, ack.Message);

            Log(message);
        }

        [Fact]
        public void CanDecodeHeartBeatIntervalMessage()
        {
            var data = HexEncoding.HexToByte("01050004" +
                                             "0f000000");
            var message = AaMiDeMessage.Create(data);
            var interval = Assert.IsType<SysHeartbeatIntervalMessage>(message);
            Assert.Equal(TimeSpan.FromSeconds(15), interval.Interval);

            Log(message);
        }

        [Fact]
        public void CanDecodeSysIpOptionsMessage()
        {
            var data = HexEncoding.HexToByte("01010008" +
                                             "b9b8200706" +
                                             "000000");
            var message = AaMiDeMessage.Create(data);
            var options = Assert.IsType<SysIpOptionsMessage>(message);
            Assert.Equal(0xb9, options.VoiceTos);
            Assert.Equal(0xb8, options.SignalTos);
            Assert.Equal(32, options.Ttl);
            Assert.Equal(7, options.SignalVlanPriority);
            Assert.Equal(6, options.VoiceVlanPriority);

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