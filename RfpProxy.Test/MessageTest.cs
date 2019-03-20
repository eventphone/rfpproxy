using System;
using System.IO;
using System.Linq;
using RfpProxy.Log.Messages;
using RfpProxy.Log.Messages.Dnm;
using RfpProxy.Log.Messages.Nwk;
using RfpProxy.Log.Messages.Rfpc;
using RfpProxy.Log.Messages.Sync;
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
            //TODO Assert.False(rfpc.HasUnknown);
        }

        [Fact]
        public void CanDecodeRfpActiveInd()
        {
            var rfpc = Decode<DnmRfpcMessage>("0301003c783002070300000211030003010c040510000000000d039003ff0602ce0007050011000000260200002701002a01012b010129050800801000280100");

            Assert.Equal(DnmRfpcType.ActiveInd, rfpc.DnmType);

            Log(rfpc);
            //TODO Assert.False(rfpc.HasUnknown);
        }

        [Fact]
        public void CanDecodeRfpReadyInd()
        {
            var rfpc = Decode<DnmRfpcMessage>("0301001178010101010207020102031132dc03010c");

            Assert.Equal(DnmRfpcType.ReadyInd, rfpc.DnmType);

            Log(rfpc);
            //TODO Assert.False(rfpc.HasUnknown);
        }

        [Fact]
        public void CanDecodeRfpSariListReq()
        {
            var rfpc = Decode<DnmRfpcMessage>("0301000878050504102af12c");

            Assert.Equal(DnmRfpcType.SariListReq, rfpc.DnmType);

            Log(rfpc);
            Assert.False(rfpc.HasUnknown);
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
            Assert.False(snmp.HasUnknown);
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
            //TODO Assert.False(auth.HasUnknown);
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
            //TODO Assert.False(init.HasUnknown);
        }

        [Fact]
        public void CanDecodeAckMessage()
        {
            var ack = Decode<AckMessage>("00010008" +
                                             "0120ffff01000000");
            Assert.Equal(MsgType.SYS_INIT, ack.Message);

            Log(ack);
            //TODO Assert.False(ack.HasUnknown);
        }

        [Fact]
        public void CanDecodeHeartBeatIntervalMessage()
        {
            var interval = Decode<SysHeartbeatIntervalMessage>("01050004" +
                                             "0f000000");
            Assert.Equal(TimeSpan.FromSeconds(15), interval.Interval);

            Log(interval);
            Assert.False(interval.HasUnknown);
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
            Assert.False(options.HasUnknown);
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
            Assert.False(http.HasUnknown);
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
            Assert.False(syslog.HasUnknown);
        }

        [Fact]
        public void CanDecodeSysCorefileUrlMessage()
        {
            var corefile = Decode<SysCorefileUrlMessage>("01160020" +
                                             "746674703a2f2f3137322e32302e352e362f616263646566676800332e310030");
            Assert.Equal("tftp://172.20.5.6/abcdefgh", corefile.Url);

            Log(corefile);
            Assert.False(corefile.HasUnknown);
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
            Assert.False(passwd.HasUnknown);
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
            Assert.False(rping.HasUnknown);
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
            Assert.False(rtdelay.HasUnknown);
        }

        [Fact]
        public void CanDecodeSysResetMessage()
        {
            var reset = Decode<SysResetMessage>("01210004" +
                                                "02000000");
            Assert.Equal(SysResetMessage.ResetType.Reset, reset.Reset);

            Log(reset);
            Assert.False(reset.HasUnknown);
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
            Assert.False(heartbeat.HasUnknown);
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
            Assert.False(led.HasUnknown);
        }

        [Fact]
        public void CanDecodeLcDataReqMessage()
        {
            var lc = Decode<DnmMessage>("030100087905080003216101");

            Log(lc);
            Assert.False(lc.HasUnknown);
        }

        [Fact]
        public void CanDecodeNwkCCSetupMessage()
        {
            var dnm = Decode<DnmMessage>("0301002679060c00212102790305050880b01002869965170606a0a0102af12ce0807b06810031160101");
            var lc = Assert.IsType<LcDataPayload>(dnm.Payload);
            var nwk = Assert.IsType<NwkCCPayload>(lc.Payload);
            Log(dnm);
            Assert.Equal(NwkCCMessageType.Setup, nwk.Type);
            Assert.False(dnm.HasUnknown);
        }

        [Fact]
        public void CanDecodeNwkCCSetupServiceMessage()
        {
            var dnm = Decode<DnmMessage>("0301002b79060600262102890305050880b01002869965170606a0a0102af12ce0b02c02188b7b06810031560106f0");
            var lc = Assert.IsType<LcDataPayload>(dnm.Payload);
            var nwk = Assert.IsType<NwkCCPayload>(lc.Payload);
            Log(dnm);
            Assert.Equal(NwkCCMessageType.Setup, nwk.Type);
            //todo Assert.False(dnm.HasUnknown);
        }

        [Fact]
        public void CanDecodeNwkCCSetupAckMessage()
        {
            var dnm = Decode<DnmMessage>("0301000a7905061005234009830d");
            var lc = Assert.IsType<LcDataPayload>(dnm.Payload);
            var nwk = Assert.IsType<NwkCCPayload>(lc.Payload);
            Log(dnm);
            Assert.Equal(NwkCCMessageType.SetupAck, nwk.Type);
            Assert.False(dnm.HasUnknown);
        }

        [Fact]
        public void CanDecodeNwkMMCipherRequestMessage()
        {
            var dnm = Decode<DnmMessage>("0301000e7905060009234219054c19028198");
            var lc = Assert.IsType<LcDataPayload>(dnm.Payload);
            var nwk = Assert.IsType<NwkMMPayload>(lc.Payload);
            Log(dnm);
            Assert.Equal(NwkMMMessageType.CipherRequest, nwk.Type);
            Assert.False(dnm.HasUnknown);
        }

        [Fact]
        public void CanDecodeNwkCCConnectMessage()
        {
            var dnm = Decode<DnmMessage>("0301000a79050610052344098307");
            var lc = Assert.IsType<LcDataPayload>(dnm.Payload);
            var nwk = Assert.IsType<NwkCCPayload>(lc.Payload);
            Log(dnm);
            Assert.Equal(NwkCCMessageType.Connect, nwk.Type);
            Assert.False(dnm.HasUnknown);
        }

        [Fact]
        public void CanDecodeNwkCCInfoMessage()
        {
            var dnm = Decode<DnmMessage>("030100197905060014234645837b7b0d81003120088110044e616d6500");
            var lc = Assert.IsType<LcDataPayload>(dnm.Payload);
            var nwk = Assert.IsType<NwkCCPayload>(lc.Payload);
            Log(dnm);
            Assert.Equal(NwkCCMessageType.Info, nwk.Type);
            //todo Assert.False(dnm.HasUnknown);
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
            Log(dnm);
            Assert.Equal(NwkMMMessageType.AuthenticationRequest, nwk.Type);
            Assert.False(dnm.HasUnknown);
        }

        [Fact]
        public void CanDecodeFragmentedNwkMessage()
        {
            var dnm = Decode<DnmMessage>("0301004779050510422340ff85550505a09400027b07015f720271b47709c08100315101205a007b69810031520102110e180400016e645c355801701d0a8e3c040531358b0b0100600623");
            var lc = Assert.IsType<LcDataPayload>(dnm.Payload);
            Assert.IsType<NwkFragmentedPayload>(lc.Payload);
            Log(dnm);
            Assert.False(dnm.HasUnknown);

            dnm = Decode<DnmMessage>("0301004779050500422342ff23232323231c03534f531c074d414e444f574e630100451f900d506f43207a6976696c6c69616e0a4556454e5450484f4e4504343530320d003b0200002308");
            lc = Assert.IsType<LcDataPayload>(dnm.Payload);
            Assert.IsType<NwkFragmentedPayload>(lc.Payload);
            Log(dnm);
            Assert.False(dnm.HasUnknown);

            dnm = Decode<DnmMessage>("03010010790505100b234421c019022715372300");
            lc = Assert.IsType<LcDataPayload>(dnm.Payload);
            var nwk = Assert.IsType<NwkMMPayload>(lc.Payload);
            Log(dnm);
            Assert.Equal(NwkMMMessageType.LocateAccept, nwk.Type);
            //todo Assert.False(dnm.HasUnknown);
        }

        [Fact]
        public void CanDecodeNwkMMAuthenticationReplyMessage()
        {
            var dnm = Decode<DnmMessage>("03010012790608000d21c42185410d04948d45b0f0f0");
            var lc = Assert.IsType<LcDataPayload>(dnm.Payload);
            var nwk = Assert.IsType<NwkMMPayload>(lc.Payload);
            Log(dnm);
            Assert.Equal(NwkMMMessageType.AuthenticationReply, nwk.Type);
            Assert.False(dnm.HasUnknown);
        }

        [Fact]
        public void CanDecodeNwkCCReleaseMessage()
        {
            var dnm = Decode<DnmMessage>("0301000d7906090008210011034de200f0");
            var lc = Assert.IsType<LcDataPayload>(dnm.Payload);
            var nwk = Assert.IsType<NwkCCPayload>(lc.Payload);
            Log(dnm);
            Assert.Equal(NwkCCMessageType.Release, nwk.Type);
            Assert.False(dnm.HasUnknown);
        }

        [Fact]
        public void CanDecodeNwkCCReleaseComMessage()
        {
            var dnm = Decode<DnmMessage>("0301000c7905061007130011835ae220");
            var lc = Assert.IsType<LcDataPayload>(dnm.Payload);
            var nwk = Assert.IsType<NwkCCPayload>(lc.Payload);
            Log(dnm);
            Assert.Equal(NwkCCMessageType.ReleaseCom, nwk.Type);
            Assert.False(dnm.HasUnknown);
        }
        
        [Fact]
        public void CanDecodeNwkCLMSMessage()
        {
            var dnm = Decode<DnmMessage>("0301001279060a000d21061d060030421fa992f0f0f0");
            var lc = Assert.IsType<LcDataPayload>(dnm.Payload);
            var nwk = Assert.IsType<NwkCLMSFixedPayload>(lc.Payload);
            Log(dnm);
            //TODO Assert.False(dnm.HasUnknown);
        }

        [Fact]
        public void CanDecodeNwkLCEPageResponseMessage()
        {
            var dnm = Decode<DnmMessage>("0301001c7906090017a110510071050880b01002869965170606a0a0102af12c");
            var lc = Assert.IsType<LcDataPayload>(dnm.Payload);
            var nwk = Assert.IsType<NwkLCESFormatPayload>(lc.Payload);
            Log(dnm);
            Assert.False(dnm.HasUnknown);
            Assert.Equal(NwkLCEMessageType.PageResponse, nwk.Type);
        }

        [Fact]
        public void CanDecodeNwkMMLocateRequestMessage()
        {
            var dnm = Decode<DnmMessage>("0301004979060900442122ff0554050880b0100286996517060781a8902af12c2507015f631135151a012006648113000400000090048f78030031417c0490020084771dc081009401170bf0f0");
            var lc = Assert.IsType<LcDataPayload>(dnm.Payload);
            Assert.IsType<NwkFragmentedPayload>(lc.Payload);
            Log(dnm);
            Assert.False(dnm.HasUnknown);

            dnm = Decode<DnmMessage>("0301004979060900442124ff4e474d414353464532300009372e302e5350313100007b2881003101027f0111095a380c050f7c7d77af160101550380480064010454010e62060030421fa9f0f0");
            lc = Assert.IsType<LcDataPayload>(dnm.Payload);
            Assert.IsType<NwkFragmentedPayload>(lc.Payload);
            Log(dnm);
            Assert.False(dnm.HasUnknown);

            dnm = Decode<DnmMessage>("0301000d790609000821260592f0f0f0f0");
            lc = Assert.IsType<LcDataPayload>(dnm.Payload);
            var nwk = Assert.IsType<NwkMMPayload>(lc.Payload);
            Log(dnm);
            Assert.Equal(NwkMMMessageType.LocateRequest, nwk.Type);
            //todo Assert.False(dnm.HasUnknown);
        }

        [Fact]
        public void CanDecodeNwkMMInfoSuggestMessage()
        {
            var dnm = Decode<DnmMessage>("0301000d79050810082320150552010180");
            var lc = Assert.IsType<LcDataPayload>(dnm.Payload);
            var nwk = Assert.IsType<NwkMMPayload>(lc.Payload);
            Log(dnm);
            Assert.Equal(NwkMMMessageType.MMInfoSuggest, nwk.Type);
            Assert.False(dnm.HasUnknown);
        }

        [Fact]
        public void CanDecodeSyncLength()
        {
            var sync = Decode<UnknownSyncMessage>("0302000b7d330810000ad00ad00200");
            Assert.Equal(8, sync.PayloadLength);
            Assert.Equal(sync.PayloadLength, sync.Reserved.Length);
            Log(sync);
        }

        [Fact]
        public void CanDecodeSyncSetFrequencyMessage()
        {
            var sync = Decode<SetFrequencySyncMessage>("030200057d18020a5d");
            Log(sync);
            Assert.False(sync.HasUnknown);
            Assert.Equal(0xa5du, sync.Frequency);
        }

        [Fact]
        public void CanDecodeSyncResetMacIndMessage()
        {
            var sync = Decode<EmptySyncMessage>("03020003 7d1b00");
            Log(sync);
            Assert.False(sync.HasUnknown);
            Assert.Equal(SyncMessageType.ResetMacInd, sync.SyncType);
        }

        [Fact]
        public void CanDecodeSyncResetMacCfmMessage()
        {
            var sync = Decode<EmptySyncMessage>("03020003 7d2100");
            Log(sync);
            Assert.False(sync.HasUnknown);
            Assert.Equal(SyncMessageType.ResetMacCfm, sync.SyncType);
        }

        [Fact]
        public void CanDecodeSyncGetReqRssiCompIndMessage()
        {
            var sync = Decode<EmptySyncMessage>("03020003 7d0e00");
            Log(sync);
            Assert.False(sync.HasUnknown);
            Assert.Equal(SyncMessageType.GetReqRssiCompInd, sync.SyncType);
        }

        [Fact]
        public void CanDecodeSyncSystemSearchIndMessage()
        {
            var sync = Decode<SystemSearchIndSyncMessage>("03020004 7d1e0103");
            Log(sync);
            Assert.Equal(SyncMessageType.SystemSearchInd, sync.SyncType);
            //TODO Assert.False(sync.HasUnknown);
        }

        [Fact]
        public void CanDecodeSyncSystemSearchCfmMessage()
        {
            var sync = Decode<SystemSearchCfmSyncMessage>("03020003 7d1f00");
            Log(sync);
            Assert.False(sync.HasUnknown);
            Assert.Equal(SyncMessageType.SystemSearchCfm, sync.SyncType);

            sync = Decode<SystemSearchCfmSyncMessage>("03020004 7d1f0100");
            Log(sync);
            Assert.Empty(sync.Rssi);

            sync = Decode<SystemSearchCfmSyncMessage>("03020008 7d1f0501 00004100");
            Log(sync);
            var (rfpn,rssi) = Assert.Single(sync.Rssi);
            Assert.Equal(0, rfpn);
            Assert.Equal(65, rssi);
        }

        [Fact]
        public void CanDecodeSyncGetReqRssiCompCfmMessage()
        {
            var sync = Decode<GetReqRssiCompCfmSyncMessage>("03020005 7d0f0210 00");
            Log(sync);
            Assert.Equal(SyncMessageType.GetReqRssiCompCfm, sync.SyncType);
            //TODO Assert.False(sync.HasUnknown);
        }

        [Fact]
        public void CanDecodeSyncStartMacMasterIndMessage()
        {
            var sync = Decode<EmptySyncMessage>("03020003 7d1c00");
            Log(sync);
            Assert.Equal(SyncMessageType.StartMacMasterInd, sync.SyncType);
            Assert.False(sync.HasUnknown);
        }

        [Fact]
        public void CanDecodeSyncStartMacMasterCfmMessage()
        {
            var sync = Decode<EmptySyncMessage>("03020003 7d2200");
            Log(sync);
            Assert.Equal(SyncMessageType.StartMacMasterCfm, sync.SyncType);
            Assert.False(sync.HasUnknown);
        }

        [Fact]
        public void CanDecodeSyncFreqCtrlModeIndMessage()
        {
            var sync = Decode<FreqCtrlModeIndSyncMessage>("03020004 7d150102");
            Log(sync);
            //TODO Assert.False(sync.HasUnknown);
        }

        [Fact]
        public void CanDecodeSyncFreqCtrlModeCfmMessage()
        {
            var sync = Decode<FreqCtrlModeCfmSyncMessage>("03020009 7d160602 08800880 10");
            Log(sync);
            Assert.Equal(2176, sync.Ppm);
            Assert.Equal(2176, sync.Avg);
            //TODO Assert.False(sync.HasUnknown);

            sync = Decode<FreqCtrlModeCfmSyncMessage>("03020009 7d160601 0a300a30 04");
            Log(sync);
            //TODO Assert.False(sync.HasUnknown);
        }

        [Fact]
        public void CanDecodeOffsetIndSyncMessage()
        {
            var sync = Decode<OffsetIndSyncMessage>("0302000b7d2c0801004cfffd430701");
            Log(sync);
            Assert.Equal(-3, sync.Offset);
            Assert.Equal(67, sync.Rssi);
            Assert.Equal(0x07, sync.QtSyncCheck);
            //TODO Assert.False(sync.HasUnknown);
        }

        [Fact]
        public void CanDecodeNwkMMLocateRejectMessage()
        {
            var dnm = Decode<DnmMessage>("0301000d79050d10081300158557600106");
            var lc = Assert.IsType<LcDataPayload>(dnm.Payload);
            var nwk = Assert.IsType<NwkMMPayload>(lc.Payload);
            Log(dnm);
            Assert.Equal(NwkMMMessageType.LocateReject, nwk.Type);
            Assert.False(dnm.HasUnknown);
        }

        [Fact]
        public void CanDecodeNwkMMKeyAllocateMessage()
        {
            var dnm = Decode<DnmMessage>("0301002279050a101d13006905420b0201880c089e6f24f242c0b13b0e089e6f24f242c0253b");
            var lc = Assert.IsType<LcDataPayload>(dnm.Payload);
            var nwk = Assert.IsType<NwkMMPayload>(lc.Payload);
            Log(dnm);
            Assert.Equal(NwkMMMessageType.KeyAllocate, nwk.Type);
            Assert.False(dnm.HasUnknown);
        }

        [Fact]
        public void CanDecodeNwkCCSetupMessage2()
        {
            var dnm = Decode<DnmMessage>("0301003579050c10302320b50305050880b01002869965170606a0a0102af12ce080e4406c1380393935363932393820444543542054656d70");
            var lc = Assert.IsType<LcDataPayload>(dnm.Payload);
            var nwk = Assert.IsType<NwkCCPayload>(lc.Payload);
            Log(dnm);
            Assert.Equal(NwkCCMessageType.Setup, nwk.Type);
            Assert.False(dnm.HasUnknown);
        }

        [Fact]
        public void CanDecodeNwkCISSFacilityMessage()
        {
            var dnm = Decode<DnmMessage>("0301002c790501002713229164621c2091a11d02013706060400856901033010a10480023830a2030a0101a303020100");
            var lc = Assert.IsType<LcDataPayload>(dnm.Payload);
            var nwk = Assert.IsType<NwkCISSPayload>(lc.Payload);
            Log(dnm);
            Assert.Equal(NwkCISSMessageType.CISSFacility, nwk.Type);
            dnm = Decode<DnmMessage>("0301003d7905021038238cd564627731c0810071100007011f01090307e30c02ffffffffffffffffffffffffffffffffffffffffffffffff000521383636360121");
            lc = Assert.IsType<LcDataPayload>(dnm.Payload);
            nwk = Assert.IsType<NwkCISSPayload>(lc.Payload);
            Log(dnm);
            Assert.Equal(NwkCISSMessageType.CISSFacility, nwk.Type);
            Assert.False(dnm.HasUnknown);
        }

        [Fact]
        public void CanDecodeRfpcStatisticsDataCfm()
        {
            var rfpc = Decode<DnmRfpcMessage>("0301004078171034b4bd3f0c7b05ee35ce03080000000000110300000000000000000000000000000000730000000000000049ba0c008f77e103f92a1606000000000000");
            var stats = rfpc.Values.OfType<StatisticDataRfpcValue>().Single();
            Log(rfpc);
            Assert.False(stats.HasUnknown);
            Assert.Equal(65107855u, stats.GoodFrames);
            //TODO Assert.False(rfpc.HasUnknown);
        }

        [Fact]
        public void CanDecodeRfpcStatisticsDataReq()
        {
            var rfpc = Decode<DnmRfpcMessage>("0301000878160f0400000000");
            var stats = rfpc.Values.OfType<StatisticDataResetRfpcValue>().Single();
            Log(rfpc);
            Assert.False(stats.HasUnknown);
            Assert.False(stats.Reset);
            Assert.False(rfpc.HasUnknown);
        }

        [Fact]
        public void CanDecodeSiemensProprietaryNwkIE()
        {
            var dnm = Decode<DnmMessage>("0301003a79060100351102c10544050780a802d51243680a0301480063092535080030039082827b13810002060110290b6f7df87d00ff9e803b0214f0f0");
            var lc = Assert.IsType<LcDataPayload>(dnm.Payload);
            var nwk = Assert.IsType<NwkMMPayload>(lc.Payload);
            Log(dnm);
            Assert.Equal(NwkMMMessageType.AccessRightsRequest, nwk.Type);
            //TODO Assert.False(dnm.HasUnknown);
        }

        [Fact]
        public void CanDecodeUpdateHomescreenNwkIe()
        {
            var dnm = Decode<DnmMessage>("03010022790501101d23206964627b16810031450d820b4556454e545048c3964e4539020045");
            var lc = Assert.IsType<LcDataPayload>(dnm.Payload);
            var nwk = Assert.IsType<NwkCISSPayload>(lc.Payload);
            Log(dnm);
            Assert.Equal(NwkCISSMessageType.CISSFacility, nwk.Type);
            //TODO Assert.False(dnm.HasUnknown);
        }

        [Fact]
        public void CanDecodeCCReleaseComMessageWithoutIe()
        {
            var dnm = Decode<DnmMessage>("0301 000d 79060d0008210a11035a0000f0");
            Log(dnm);
        }

        [Fact]
        public void CanDecodeMacInfoIndMessage()
        {
            var dnm = Decode<DnmMessage>("03010034 7a1c010002c9004672616d65734f4b3d323839204672616d65734d555445443d302042484f3d302042484f4661696c65643d3000");
            var info = Assert.IsType<MacInfoIndPayload>(dnm.Payload);
            Log(dnm);
            Assert.False(info.HasUnknown);
        }

        [Fact]
        public void CanDecodeMediaStatisticsMessage()
        {
            var stats = Decode<StatisticsMediaMessage>(
                "0205 0024" +
                "f919 0000" +
                "0200" +
                "00007200" +
                "00004047" +
                "0000560000002135000000000000be310000ac141702");
            Log(stats);
            //TODO Assert.False(stats.HasUnknown);
        }

        private T Decode<T>(string hex) where T:AaMiDeMessage
        {
            var data = HexEncoding.HexToByte(hex.Replace(" ", String.Empty));
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