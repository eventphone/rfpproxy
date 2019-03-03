using System;
using System.IO;
using RfpProxy.Log.Messages;
using RfpProxy.Log.Messages.Dnm;
using RfpProxy.Log.Messages.Nwk;
using RfpProxyLib;
using Xunit;
using Xunit.Abstractions;

namespace RfpProxy.Test
{
    public class MessageTest
    {
        private readonly ITestOutputHelper _output;

        private readonly AaMiDeReassembler _reassembler;

        public MessageTest(ITestOutputHelper output)
        {
            _output = output;
            _reassembler = new AaMiDeReassembler();
        }

        [Fact]
        public void CanDecodeRfpInitReq()
        {
            var rfpc = Decode<DnmRfpcMessage>(("03010023" +
                                             "7802" +
                                             "04 05 102af12c26" +
                                             "06 02 ca42" +
                                             "07 05 0010000000" +
                                             "26 02 0000" +
                                             "0d 03 1003ff" +
                                             "27 01 1f  28 01 00").Replace(" ", String.Empty));

            Assert.Equal(DnmRfpcType.InitReq, rfpc.DnmType);

            Log(rfpc);
        }

        [Fact]
        public void CanDecodeSysSnmpMessage()
        {
            var snmp = Decode<SnmpRfpUpdateMessage>("0501014c" +
                                             "ac141701" +
                                             "62626262626262626262626262626262626262626262626262626262626262626262626262626262009f92b0f09f92b0f09f92b0f09f92b0f09f92b0f09f92b0f09f92b0f09f92b0f09f92b0f09f92b000" +
                                             "646562756744454354313233009f92b0f09f92b0f09f92b0f09f92b0f09f92b0f09f92b0f09f92b00000000000000000000000000000000000000000000000000000000000000000000000000000000000" +
                                             "524650203432202830303a33303a34323a30463a38323a323729000000000000000000000000000000000000000000000000890000001839361690be751600000000000000000000000000000000000000" +
                                             "6161616161616161616161616161616161616161009f92b0f09f92b0f09f92b0f09f92b0f09f92b000" +
                                             "6363636363636363636363636363636363636363000000000000000000000000000000000000000000010000");
            Assert.Equal("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb", snmp.Contact);
            Assert.Equal("debugDECT123", snmp.Location);
            Assert.Equal("RFP 42 (00:30:42:0F:82:27)", snmp.Name);
            Assert.Equal("aaaaaaaaaaaaaaaaaaaa", snmp.RoCommunity);
            Assert.Equal("cccccccccccccccccccc", snmp.TrapCommunity);
            Assert.Equal("172.20.23.1", snmp.Server.ToString());
            Assert.True(snmp.TrapEnabled);
            
            Log(snmp);
        }

        [Fact]
        public void CanDecodeSysAuthenticateMessage()
        {
            var auth = Decode<SysAuthenticateMessage>("012d0020" +
                                             "57b858bb227549d7" +
                                             "2215096317457eee" +
                                             "f01aa118ab156ad5" +
                                             "e8b35e55ab2b30a0");
            Assert.Equal("2215096317457eee", auth.RfpIv.ToHex());
            Assert.Equal("e8b35e55ab2b30a0", auth.OmmIv.ToHex());

            Log(auth);
        }

        [Fact]
        public void CanDecodeSysInitMessage()
        {
            var init = Decode<SysInitMessage>("01200104" +
                                             "0000000800070100" +
                                             "0030420f8227" +
                                             "0000000000000000" +
                                             "03fc" +
                                             "a9b9df00301bd287ea0373c9f7869951d6fa651ccbdf21e665488dd8d84e52e805da3272c066522501360cffe09efbad5393d713ad9a19874c2496ae5c629b69000701000000000000000000" +
                                             "5349502d4445435420372e312d434b313400000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000" +
                                             "3720690a8f6d49be" +
                                             "756b4815d44020ee");
            Assert.Equal("0030420F8227", init.Mac.ToString());
            Assert.Equal(0x3fcu, init.Capabilities);
            Assert.Equal("SIP-DECT 7.1-CK14", init.SwVersion);

            Log(init);
        }

        [Fact]
        public void CanDecodeAckMessage()
        {
            var ack = Decode<AckMessage>("00010008" +
                                             "0120ffff01000000");
            Assert.Equal(MsgType.SYS_INIT, ack.Message);

            Log(ack);
        }

        [Fact]
        public void CanDecodeHeartBeatIntervalMessage()
        {
            var interval = Decode<SysHeartbeatIntervalMessage>("01050004" +
                                             "0f000000");
            Assert.Equal(TimeSpan.FromSeconds(15), interval.Interval);

            Log(interval);
        }

        [Fact]
        public void CanDecodeSysIpOptionsMessage()
        {
            var options = Decode<SysIpOptionsMessage>("01010008" +
                                             "b9b8200706" +
                                             "000000");
            Assert.Equal(0xb9, options.VoiceTos);
            Assert.Equal(0xb8, options.SignalTos);
            Assert.Equal(32, options.Ttl);
            Assert.Equal(7, options.SignalVlanPriority);
            Assert.Equal(6, options.VoiceVlanPriority);

            Log(options);
        }

        [Fact]
        public void CanDecodeSysHttpSetMessage()
        {
            var http = Decode<SysHttpSetMessage>("01090008" +
                                             "ac140103" +
                                             "01bb" +
                                             "022f");
            Assert.Equal("172.20.1.3", http.Ip.ToString());
            Assert.Equal(443, http.Port);

            Log(http);
        }

        [Fact]
        public void CanDecodeSysSyslogMessage()
        {
            var syslog = Decode<SysSyslogMessage>("01070008" +
                                             "ac141701" +
                                             "0202" +
                                             "676a");
            Assert.Equal("172.20.23.1", syslog.Ip.ToString());
            Assert.Equal(514, syslog.Port);

            Log(syslog);
        }

        [Fact]
        public void CanDecodeSysCorefileUrlMessage()
        {
            var corefile = Decode<SysCorefileUrlMessage>("01160020" +
                                             "746674703a2f2f3137322e32302e352e362f616263646566676800332e310030");
            Assert.Equal("tftp://172.20.5.6/abcdefgh", corefile.Url);

            Log(corefile);
        }

        [Fact]
        public void CanDecodeSysPasswdMessage()
        {
            var passwd = Decode<SysPasswdMessage>("010a0108" +
                                             "014e" +
                                             "726f6f74006a2f50747353547a4d78387247517264597273316846326a4752664771464a61486b543943793861764952426b497249416a4c742e5063477448596a" +
                                             "6a755071316f6c65694767375748645a3569746c432f00574f786f48464e3071474d6977426c6e2e79466a5541676e50544f72304d41596d374e334f6d36453255" +
                                             "6f6d6d006f686933524346372f67635058656f6a4461746367484f6f59534957542f4748692e4d39436449454a786674625463713456536d7073614e4c75767176" +
                                             "6a755071316f6c65694767375748645a3569746c432f00764a6b474467363874466e356d6358776d445173616f615270394f4d43376a3753555168415972337166" +
                                             "0000");
            Assert.Equal("root", passwd.RootUser);
            Assert.Equal("juPq1oleiGg7WHdZ5itlC/", passwd.RootPassword);
            Assert.Equal("omm", passwd.AdminUser);
            Assert.Equal("juPq1oleiGg7WHdZ5itlC/", passwd.AdminPassword);
            Assert.True(passwd.IsRemoteAccessEnabled);

            Log(passwd);
        }

        [Fact]
        public void CanDecodeSysRPingMessage()
        {
            var rping = Decode<SysRPingMessage>("010e000c" +
                                             "ac141701" +
                                             "00000023" +
                                             "00000000");
            Assert.Equal("172.20.23.1", rping.Ip.ToString());
            Assert.Equal(TimeSpan.FromMilliseconds(0x23), rping.Rtt);

            Log(rping);
        }

        [Fact]
        public void CanDecodeSysRoundtripDelayMessage()
        {
            var rtdelay = Decode<SysRoundtripDelayMessage>("01170010" +
                                             "e0142a0c216bb98c" +
                                             "e0142a0c2ef544bb");
            Assert.Equal(2019, rtdelay.Time2.Year);
            Assert.Equal(2, rtdelay.Time2.Month);
            Assert.Equal(17, rtdelay.Time2.Day);
            Assert.Equal(18, rtdelay.Time2.Hour);
            Assert.Equal(44, rtdelay.Time2.Minute);
            Assert.Equal(28, rtdelay.Time2.Second);
            Assert.NotEqual(rtdelay.Time1, rtdelay.Time2);

            Log(rtdelay);
        }

        [Fact]
        public void CanDecodeSysResetMessage()
        {
            var reset = Decode<SysResetMessage>("01210004" +
                                                "02000000");
            Assert.Equal(SysResetMessage.ResetType.Reset, reset.Reset);

            Log(reset);
        }

        [Fact]
        public void CanDecodeHeartbeatMessage()
        {
            var heartbeat = Decode<HeartbeatMessage>("00030008" + 
                                                     "0c6b560004a60000");
            Assert.Equal(0, heartbeat.Uptime.Days);
            Assert.Equal(1, heartbeat.Uptime.Hours);
            Assert.Equal(34, heartbeat.Uptime.Minutes);

            Log(heartbeat);
        }

        [Fact]
        public void CanDecodeSysLedMessage()
        {
            var led = Decode<SysLedMessage>("01020004" +
                                            "08010000");
            Assert.False(led.Led1);
            Assert.False(led.Led2);
            Assert.False(led.Led3);
            Assert.True(led.Led4);
            Assert.Equal(SysLedMessage.ColorScheme.Green, led.Color);

            Log(led);
        }

        [Fact]
        public void CanDecodeLcDataReqMessage()
        {
            var lc = Decode<DnmMessage>("030100087905080003216101");

            Log(lc);
        }

        [Fact]
        public void CanDecodeNwkCCSetupMessage()
        {
            var dnm = Decode<DnmMessage>("0301002679060c00212102790305050880b01002869965170606a0a0102af12ce0807b06810031160101");
            var lc = Assert.IsType<LcDataPayload>(dnm.Payload);
            var nwk = Assert.IsType<NwkCCPayload>(lc.Payload);
            Assert.Equal(NwkCCMessageType.Setup, nwk.Type);
            Assert.False(dnm.HasUnknown);
            Log(dnm);
        }

        [Fact]
        public void CanDecodeNwkCCSetupServiceMessage()
        {
            var dnm = Decode<DnmMessage>("0301002b79060600262102890305050880b01002869965170606a0a0102af12ce0b02c02188b7b06810031560106f0");
            var lc = Assert.IsType<LcDataPayload>(dnm.Payload);
            var nwk = Assert.IsType<NwkCCPayload>(lc.Payload);
            Assert.Equal(NwkCCMessageType.Setup, nwk.Type);
            Log(dnm);
            Assert.False(dnm.HasUnknown);
        }

        [Fact]
        public void CanDecodeNwkCCSetupAckMessage()
        {
            var dnm = Decode<DnmMessage>("0301000a7905061005234009830d");
            var lc = Assert.IsType<LcDataPayload>(dnm.Payload);
            var nwk = Assert.IsType<NwkCCPayload>(lc.Payload);
            Assert.Equal(NwkCCMessageType.SetupAck, nwk.Type);
            Log(dnm);
            Assert.False(dnm.HasUnknown);
        }

        [Fact]
        public void CanDecodeNwkMMCipherRequestMessage()
        {
            var dnm = Decode<DnmMessage>("0301000e7905060009234219054c19028198");
            var lc = Assert.IsType<LcDataPayload>(dnm.Payload);
            var nwk = Assert.IsType<NwkMMPayload>(lc.Payload);
            Assert.Equal(NwkMMMessageType.CipherRequest, nwk.Type);
            Log(dnm);
            Assert.False(dnm.HasUnknown);
        }

        [Fact]
        public void CanDecodeNwkCCConnectMessage()
        {
            var dnm = Decode<DnmMessage>("0301000a79050610052344098307");
            var lc = Assert.IsType<LcDataPayload>(dnm.Payload);
            var nwk = Assert.IsType<NwkCCPayload>(lc.Payload);
            Assert.Equal(NwkCCMessageType.Connect, nwk.Type);
            Log(dnm);
            Assert.False(dnm.HasUnknown);
        }

        [Fact]
        public void CanDecodeNwkCCInfoMessage()
        {
            var dnm = Decode<DnmMessage>("030100197905060014234645837b7b0d81003120088110044e616d6500");
            var lc = Assert.IsType<LcDataPayload>(dnm.Payload);
            var nwk = Assert.IsType<NwkCCPayload>(lc.Payload);
            Assert.Equal(NwkCCMessageType.Info, nwk.Type);
            Log(dnm);
            Assert.False(dnm.HasUnknown);
        }

        [Fact]
        public void CanDecodeEmptyNwkMessage()
        {
            var dnm = Decode<DnmMessage>("030100087906090003238101");
            Log(dnm);
            Assert.False(dnm.HasUnknown);
        }

        [Fact]
        public void CanDecodeNwkMMAuthenticationRequestMessage()
        {
            var dnm = Decode<DnmMessage>("03010023790506001e234a6d05400a030118180c08cf0b74164913db200e08ad256f18dff1af8d");
            var lc = Assert.IsType<LcDataPayload>(dnm.Payload);
            var nwk = Assert.IsType<NwkMMPayload>(lc.Payload);
            Assert.Equal(NwkMMMessageType.AuthenticationRequest, nwk.Type);
            Log(dnm);
            Assert.False(dnm.HasUnknown);
        }

        [Fact]
        public void CanDecodeFragmentedNwkMessage()
        {
            var dnm = Decode<DnmMessage>("0301004779050510422340ff85550505a09400027b07015f720271b47709c08100315101205a007b69810031520102110e180400016e645c355801701d0a8e3c040531358b0b0100600623");
            var lc = Assert.IsType<LcDataPayload>(dnm.Payload);
            var fragment = Assert.IsType<NwkFragmentedPayload>(lc.Payload);
            Log(dnm);
            Assert.False(dnm.HasUnknown);

            dnm = Decode<DnmMessage>("0301004779050500422342ff23232323231c03534f531c074d414e444f574e630100451f900d506f43207a6976696c6c69616e0a4556454e5450484f4e4504343530320d003b0200002308");
            lc = Assert.IsType<LcDataPayload>(dnm.Payload);
            fragment = Assert.IsType<NwkFragmentedPayload>(lc.Payload);
            Log(dnm);
            Assert.False(dnm.HasUnknown);

            dnm = Decode<DnmMessage>("03010010790505100b234421c019022715372300");
            lc = Assert.IsType<LcDataPayload>(dnm.Payload);
            var nwk = Assert.IsType<NwkMMPayload>(lc.Payload);
            Log(dnm);
            Assert.Equal(NwkMMMessageType.LocateAccept, nwk.Type);
            Assert.False(dnm.HasUnknown);
        }

        [Fact]
        public void CanDecodeNwkMMAuthenticationReplyMessage()
        {
            var dnm = Decode<DnmMessage>("03010012790608000d21c42185410d04948d45b0f0f0");
            var lc = Assert.IsType<LcDataPayload>(dnm.Payload);
            var nwk = Assert.IsType<NwkMMPayload>(lc.Payload);
            Assert.Equal(NwkMMMessageType.AuthenticationReply, nwk.Type);
            Log(dnm);
            Assert.False(dnm.HasUnknown);
        }

        [Fact]
        public void CanDecodeNwkCCReleaseMessage()
        {
            var dnm = Decode<DnmMessage>("0301000d7906090008210011034de200f0");
            var lc = Assert.IsType<LcDataPayload>(dnm.Payload);
            var nwk = Assert.IsType<NwkCCPayload>(lc.Payload);
            Assert.Equal(NwkCCMessageType.Release, nwk.Type);
            Log(dnm);
            Assert.False(dnm.HasUnknown);
        }
        
        [Fact]
        public void CanDecodeNwkCLMSMessage()
        {
            var dnm = Decode<DnmMessage>("0301001279060a000d21061d060030421fa992f0f0f0");
            var lc = Assert.IsType<LcDataPayload>(dnm.Payload);
            var nwk = Assert.IsType<NwkCLMSFixedPayload>(lc.Payload);
            Log(dnm);
            Assert.False(dnm.HasUnknown);
        }

        [Fact]
        public void CanDecodeNwkMessage()
        {
            var dnm = Decode<DnmMessage>("0301001c7906090017a110510071050880b01002869965170606a0a0102af12c");
            var lc = Assert.IsType<LcDataPayload>(dnm.Payload);

            Log(dnm);
            Assert.False(dnm.HasUnknown);
        }

        [Fact]
        public void CanDecodeNwkMMLocateRequestMessage()
        {
            var dnm = Decode<DnmMessage>("0301004979060900442122ff0554050880b0100286996517060781a8902af12c2507015f631135151a012006648113000400000090048f78030031417c0490020084771dc081009401170bf0f0");
            var lc = Assert.IsType<LcDataPayload>(dnm.Payload);
            var fragment = Assert.IsType<NwkFragmentedPayload>(lc.Payload);
            Log(dnm);
            Assert.False(dnm.HasUnknown);

            dnm = Decode<DnmMessage>("0301004979060900442124ff4e474d414353464532300009372e302e5350313100007b2881003101027f0111095a380c050f7c7d77af160101550380480064010454010e62060030421fa9f0f0");
            lc = Assert.IsType<LcDataPayload>(dnm.Payload);
            fragment = Assert.IsType<NwkFragmentedPayload>(lc.Payload);
            Log(dnm);
            Assert.False(dnm.HasUnknown);

            dnm = Decode<DnmMessage>("0301000d790609000821260592f0f0f0f0");
            lc = Assert.IsType<LcDataPayload>(dnm.Payload);
            var nwk = Assert.IsType<NwkMMPayload>(lc.Payload);
            Log(dnm);
            Assert.Equal(NwkMMMessageType.LocateRequest, nwk.Type);
            Assert.False(dnm.HasUnknown);
        }
        private T Decode<T>(string hex) where T:AaMiDeMessage
        {
            var data = HexEncoding.HexToByte(hex);
            var message = AaMiDeMessage.Create(data, _reassembler);
            return Assert.IsType<T>(message);
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