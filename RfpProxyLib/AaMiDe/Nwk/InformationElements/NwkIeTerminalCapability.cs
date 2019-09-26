using System;
using System.IO;

namespace RfpProxyLib.AaMiDe.Nwk.InformationElements
{
    public sealed class NwkIeTerminalCapability : NwkVariableLengthInformationElement
    {
        public enum ToneCapabilities:byte
        {
            NotApplicable = 0b000,
            NoTone = 0b001,
            DialToneOnly = 0b010,
            /// <summary>
            /// ITU-T Recommendation E.182 [42] tones supported
            /// </summary>
            E182 = 0b011,
            /// <summary>
            /// Complete DECT tones supported
            /// </summary>
            Complete = 0b100,
        }

        public enum DisplayCapabilities : byte
        {
            NotApplicable = 0b0000,
            NoDisplay=0b0001,
            Numeric = 0b0010,
            NumericPlus = 0b0011,
            Alphanumeric = 0b0100,
            Full = 0b0101,
        }

        public enum EchoParameters : byte
        {
            NotApplicable = 0b000,
            MinimumTCLw = 0b001,
            /// <summary>
            /// TCLw > 46 dB (Full TCLw)
            /// </summary>
            TCLw46 = 0b010,
            /// <summary>
            /// TCLw > 55 dB (VoIP compatible TCLw)
            /// </summary>
            TCLw55 = 0b011,
        }

        public enum AmbientNoiseRejectionCapabilities : byte
        {
            NotApplicable = 0b00,
            NoNoiseRejection = 0b01,
            NoiseRejectionProvided = 0b10,
        }

        public enum AdaptiveVolumeControl:byte
        {
            NotApplicable = 0b00,
            NoPP = 0b01,
            PP = 0b10,
            DisableFP = 0b11
        }

        [Flags]
        public enum SlotCapabilities : byte
        {
            Half = 0b0000_0001,
            Long640 = 0b0000_0010,
            Long672 = 0b0000_0100,
            Full = 0b0000_1000,
            Double = 0b0001_0000,
            Reserved1 = 0b0010_0000,
            Reserved2 = 0b0100_0000,
        }

        public enum ScrollingBehaviourType : byte
        {
            NotSpecified = 0b0000_0000,
            Type1 = 0b0000_0001,
            Type2 = 0b0000_0010,
        }

        [Flags]
        public enum ProfileIndicator1 : byte
        {
            CAP = 0b0000_0001,
            GAP = 0b0000_0010,
            DectGsmInterworking = 0b0000_0100,
            ISDN = 0b0000_1000,
            LRMS = 0b0001_0000,
            DprsStream = 0b0010_0000,
            DprsAsymmetricBearers = 0b0100_0000,
        }

        [Flags]
        public enum ProfileIndicator2 : byte
        {
            DprsM5 = 0b0000_0001,
            DataServiceProfileD = 0b0000_0010,
            IsdnIntermediateAccess = 0b0000_0100,
            UmtsGsmBearer = 0b0000_1000,
            UmtsGsmSMS = 0b0001_0000,
            UmtsGsmFax = 0b0010_0000,
            RAP1 = 0b0100_0000
        }

        [Flags]
        public enum ProfileIndicator3 : byte
        {
            DectGsmDualMode = 0b0000_0001,
            Wrs  = 0b0000_0010,
            SmsOverLrms = 0b0000_0100,
            DMAP = 0b0000_1000,
            MultiportCTA = 0b0001_0000,
            Ethernet  = 0b0010_0000,
            TokenRing = 0b0100_0000,
        }

        [Flags]
        public enum ProfileIndicator4 : byte
        {
            IP = 0b0000_0001,
            PPP = 0b0000_0010,
            V24 = 0b0000_0100,
            CF = 0b0000_1000,
            IPQ = 0b0001_0000,
            RAP2 = 0b0010_0000,
            GenericMediaEncapsulationTransport = 0b0100_0000,
        }

        [Flags]
        public enum ProfileIndicator5 : byte
        {
            BZFiel2LevelModulation = 0b0000_0001,
            BZFiel4LevelModulation = 0b0000_0010,
            BZFiel8LevelModulation = 0b0000_0100,
            BZFiel16LevelModulation = 0b0000_1000,
            AFiel2LevelModulation = 0b0001_0000,
            AFiel4LevelModulation = 0b0010_0000,
            AFiel8LevelModulation = 0b0100_0000,
        }

        [Flags]
        public enum ProfileIndicator6 : byte
        {
            DECTUMTSInterworking = 0b0000_0001,
            DECTUMTSInterworkingGPRS = 0b0000_0010,
            BasicODAP = 0b0000_0100,
            FmmsInterworking = 0b0000_1000,
            ChannelGF = 0b0001_0000,
            FastHoppingRadio = 0b0010_0000,
            NoEmissionMode = 0b0100_0000
        }

        [Flags]
        public enum ProfileIndicator7 : byte
        {
            BZFiel64LevelModulation = 0b0000_0001,
            WidebandVoice = 0b0000_0010,
            Reserved = 0b0000_0100,
            NGDectPart3 = 0b0000_0110,
            HeadsetManagement = 0b0000_1000,
            ReKeyingAndDefaultCipherKey = 0b0001_0000,
            AssociatedMelody = 0b0010_0000,
            NGDectPart5 = 0b0100_0000
        }

        [Flags]
        public enum ProfileIndicator8 : byte
        {
            EUTypeIpf = 0b0000_0001,
            ChannelIpfAdvanced = 0b0000_0010,
            ChannelSipf = 0b0000_0100,
            PacketDataCat1 = 0b0000_1000,
            PacketDataCat2 = 0b0001_0000,
            PacketDataCat3 = 0b0001_1000,
            PacketDataCat4_8PSK = 0b0010_0000,
            PacketDataCat4_64QAM = 0b0010_1000,
        }

        [Flags]
        public enum ProfileIndicator9 : byte
        {
            DprsClass3 = 0b0000_0001,
            DprsClass4 = 0b0000_0010,
            LightDataServices= 0b0010_0000,
        }

        [Flags]
        public enum ProfileIndicator10 : byte
        {
            DateAndTimeRecovery = 0b0000_0001
        }

        public enum ControlCodes : byte
        {
            NotSpecified = 0b0000,
            ClearDisplay_0CH = 0b0001,
            Coding001Plus_08HTo0BH_0DH = 0b0010,
            Coding010Plus_02H_03H_06H_07H_19H_1AH = 0b0011,
            Coding011Plus_0EH_0FH = 0b0100,
        }

        [Flags]
        public enum CharacterSets : byte
        {
            Latin1 = 0b0000_0001,
            Latin9 = 0b0000_0010,
            Latin5 = 0b0000_0100,
            Greek = 0b0000_1000,
        }

        public enum BlindSlotIndication : byte
        {
            No = 0b0000,
            BothAdjacent = 0b0001,
            EverySecond = 0b0010,
            Custom = 0b0011,
        }

        public ToneCapabilities Tone { get; }

        public DisplayCapabilities Display { get; }

        public EchoParameters Echo { get; }

        public AmbientNoiseRejectionCapabilities NRej { get; }

        public AdaptiveVolumeControl AVol { get; }

        public SlotCapabilities SlotTypes { get; }

        public ushort DisplayCharCount { get; }

        public byte DisplayLines { get; }

        public byte CharsPerLine { get; }

        public ScrollingBehaviourType ScrollBehaviour { get; }

        public ProfileIndicator1 Profile1 { get; }

        public ProfileIndicator2 Profile2 { get; }

        public ProfileIndicator3 Profile3 { get; }

        public ProfileIndicator4 Profile4 { get; }

        public ProfileIndicator5 Profile5 { get; }

        public ProfileIndicator6 Profile6 { get; }

        public ProfileIndicator7 Profile7 { get; }

        public ProfileIndicator8 Profile8 { get; }

        public ProfileIndicator9 Profile9 { get; }

        public ProfileIndicator10 Profile10 { get; }

        public bool Dsaa2 { get; }

        public bool Dsc2 { get; }

        public ControlCodes ControlCode { get; }

        public CharacterSets Charsets { get; }

        public BlindSlotIndication BlindSlot { get; }

        public bool Sp0 { get; }

        public bool Sp1 { get; }

        public bool Sp2 { get; }

        public bool Sp3 { get; }

        public bool Sp4 { get; }

        public bool Sp5 { get; }

        public bool Sp6 { get; }

        public bool Sp7 { get; }

        public bool Sp8 { get; }

        public bool Sp9 { get; }

        public bool Sp10 { get; }

        public bool Sp11 { get; }

        public override bool HasUnknown { get; }

        public NwkIeTerminalCapability(ReadOnlyMemory<byte> data):base(NwkVariableLengthElementType.TerminalCapability, data)
        {
            var span = data.Span;
            Tone = (ToneCapabilities)((span[0] & 0x70) >> 4);
            Display = (DisplayCapabilities)(span[0] & 0xf);
            if (span[0] >= 128)
                goto OctetGroup4;
            span = span.Slice(1);

            Echo = (EchoParameters)((span[0] & 0x70) >> 4);
            NRej = (AmbientNoiseRejectionCapabilities)((span[0] & 0xc) >> 2);
            AVol = (AdaptiveVolumeControl)(span[0] & 0x3);
            if (span[0] >= 128)
                goto OctetGroup4;
            span = span.Slice(1);

            SlotTypes = (SlotCapabilities)(span[0] & 0x7f);
            if (span[0] >= 128)
                goto OctetGroup4;
            span = span.Slice(1);

            DisplayCharCount = (ushort)(span[0] & 0x7f);
            if (span[0] >= 128)
                goto OctetGroup4;
            span = span.Slice(1);

            DisplayCharCount = (ushort)((DisplayCharCount << 7) | (span[0] & 0x7f));
            if (span[0] >= 128)
                goto OctetGroup4;
            span = span.Slice(1);

            DisplayLines = (byte)(span[0] & 0x7f);
            if (span[0] >= 128)
                goto OctetGroup4;
            span = span.Slice(1);

            CharsPerLine = (byte)(span[0] & 0x7f);
            if (span[0] >= 128)
                goto OctetGroup4;
            span = span.Slice(1);

            ScrollBehaviour = (ScrollingBehaviourType)(span[0] & 0x7f);
            if (span[0] >= 128)
                goto OctetGroup4;
            span = span.Slice(1);
            
            HasUnknown = true;
            while (span[0] <= 128)
            {
                span = span.Slice(1);
            }
            OctetGroup4:
            span = span.Slice(1);

            Profile1 = (ProfileIndicator1)(span[0] & 0x7f);
            if (span[0] >= 128)
                goto OctetGroup5;
            span = span.Slice(1);

            Profile2 = (ProfileIndicator2)(span[0] & 0x7f);
            if (span[0] >= 128)
                goto OctetGroup5;
            span = span.Slice(1);

            Profile3 = (ProfileIndicator3)(span[0] & 0x7f);
            if (span[0] >= 128)
                goto OctetGroup5;
            span = span.Slice(1);

            Profile4 = (ProfileIndicator4)(span[0] & 0x7f);
            if (span[0] >= 128)
                goto OctetGroup5;
            span = span.Slice(1);

            Profile5 = (ProfileIndicator5)(span[0] & 0x7f);
            if (span[0] >= 128)
                goto OctetGroup5;
            span = span.Slice(1);

            Profile6 = (ProfileIndicator6)(span[0] & 0x7f);
            if (span[0] >= 128)
                goto OctetGroup5;
            span = span.Slice(1);

            Profile7 = (ProfileIndicator7)(span[0] & 0x7f);
            if (span[0] >= 128)
                goto OctetGroup5;
            span = span.Slice(1);

            Profile8 = (ProfileIndicator8)(span[0] & 0x7f);
            if (span[0] >= 128)
                goto OctetGroup5;
            span = span.Slice(1);

            Profile9 = (ProfileIndicator9)(span[0] & 0x7f);
            if (span[0] >= 128)
                goto OctetGroup5;
            span = span.Slice(1);

            Profile10 = (ProfileIndicator10)(span[0] & 0x7f);
            if (span[0] >= 128)
                goto OctetGroup5;
            span = span.Slice(1);

            HasUnknown = true;
            while (span[0] <= 128)
            {
                span = span.Slice(1);
            }
            OctetGroup5:
            span = span.Slice(1);
            Dsaa2 = (span[0] & 0x40) != 0;
            Dsc2 = (span[0] & 0x20) != 0;
            ControlCode = (ControlCodes) (span[0] & 0b0111);
            if (span[0] >= 128)
                goto OctetGroup6;
            span = span.Slice(1);
            Charsets = (CharacterSets) (span[0] & 0x7f);
            if (span[0] >= 128)
                goto OctetGroup6;
            span = span.Slice(1);

            HasUnknown = true;
            while (span[0] <= 128)
            {
                span = span.Slice(1);
            }
            OctetGroup6:
            span = span.Slice(1);
            if (span.IsEmpty)
                return;
            BlindSlot = (BlindSlotIndication) ((span[0] >> 5) & 0x3);
            Sp0 = (span[0] & 0x10) != 0;
            Sp1 = (span[0] & 0x08) != 0;
            Sp2 = (span[0] & 0x04) != 0;
            Sp3 = (span[0] & 0x02) != 0;
            Sp4 = (span[0] & 0x01) != 0;
            
            if (span[0] >= 128)
                return;
            span = span.Slice(1);
            
            Sp5 = (span[0] & 0x40) != 0;
            Sp6 = (span[0] & 0x20) != 0;
            Sp7 = (span[0] & 0x10) != 0;
            Sp8 = (span[0] & 0x08) != 0;
            Sp9 = (span[0] & 0x04) != 0;
            Sp10 = (span[0] & 0x02) != 0;
            Sp11 = (span[0] & 0x01) != 0;

            if (span[0] <= 128)
                HasUnknown = true;
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" Tone({Tone:G}) Display({Display:G}) Echo({Echo:G}) N-REJ({NRej == AmbientNoiseRejectionCapabilities.NoiseRejectionProvided}) A-Vol({AVol})");
            writer.Write($" Slot({SlotTypes}) DisplayChars({DisplayCharCount}) DisplayLines({DisplayLines}) CharsPerLine({CharsPerLine})");
            if (ScrollBehaviour != 0)
                writer.Write($" Scroll({ScrollBehaviour})");
            if (Profile1 != 0)
                writer.Write($" Profile1({Profile1})");
            if (Profile2 != 0)
                writer.Write($" Profile2({Profile2})");
            if (Profile3 != 0)
                writer.Write($" Profile3({Profile3})");
            if (Profile4 != 0)
                writer.Write($" Profile4({Profile4})");
            if (Profile5 != 0)
                writer.Write($" Profile5({Profile5})");
            if (Profile6 != 0)
                writer.Write($" Profile6({Profile6})");
            if (Profile7 != 0)
                writer.Write($" Profile7({Profile7})");
            if (Profile8 != 0)
                writer.Write($" Profile8({Profile8})");
            if (Profile9 != 0)
                writer.Write($" Profile9({Profile9})");
            if (Profile10 != 0)
                writer.Write($" Profile10({Profile10})");
            writer.Write($" DSAA2({Dsaa2}) DSC2({Dsc2}) ControlCodes({ControlCode})");
            if (Charsets != 0)
                writer.Write($" Charsets({Charsets})");
            writer.Write($" BlindSlot({BlindSlot})");
            if (BlindSlot == BlindSlotIndication.Custom)
                writer.Write($" Sp0-11({(Sp0?'1':'0')}{(Sp1?'1':'0')}{(Sp2?'1':'0')}{(Sp3?'1':'0')}{(Sp4?'1':'0')}{(Sp5?'1':'0')}{(Sp6?'1':'0')}{(Sp7?'1':'0')}{(Sp8?'1':'0')}{(Sp9?'1':'0')}{(Sp10?'1':'0')}{(Sp11?'1':'0')})");
        }
    }
}