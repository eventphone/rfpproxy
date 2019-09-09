using System;
using System.Buffers.Binary;
using System.IO;

namespace RfpProxyLib.AaMiDe.Rfpc
{
    public sealed class ExtendedCapabilities2RfpcValue : DnmRfpcValue
    {
        [Flags]
        public enum ExtendedMacCapability : ushort
        {
            LongSlot640Support=0b1000_0000_0000,
            LongSlot672Support=0b0100_0000_0000,
            EuTypeMuxAndChannelIpfBasicProceduresSupported=0b0010_0000_0000,
            ChannelIpfAdvancedProceduresSupported=0b0001_0000_0000,
            ChannelSipfSupported=0b0000_1000_0000,
            ChannelGfSupported=0b0000_0100_0000,
            UleWrsDelayedPagingSupport=0b0000_0010_0000,
            NoEmissionModePreferredCN=0b0000_0000_0001,
        }

        [Flags]
        public enum ExtendedHigherLayerCapabilities
        {
            NgDectWidebandVoice = 0b1000_0000_0000_0000_0000_0000,
            NgDectExtendedWidebandVoiceSupported = 0b0000_0100_0000_0000_0000_0000,
            NgDectFpExtendedWidebandVoice = 0b0000_0010_0000_0000_0000_0000,
            NoEmissionModeSupported = 0b0000_0000_0001_0000_0000_0000,
            NgDect5Supported = 0b0000_0000_0000_1000_0000_0000,
            ReKeyingAndEarlyEncryptionSupported = 0b0000_0000_0000_0000_0010_0000,
            Dsaa2Supported = 0b0000_0000_0000_0000_0001_0000,
            Dsc2Supported = 0b0000_0000_0000_0000_0000_1000,
            LightDataServicesSupported = 0b0000_0000_0000_0000_0000_0100,
        }

        public enum DprsDataCategory : byte
        {
            NotSupported = 0b0000,
            Cat1 = 0b0001,
            Cat2 = 0b0010,
            Cat3 = 0b0011,
            Cat4 = 0b0100,
            Cat5 = 0b0101,
        }

        [Flags]
        public enum ExtendedVoiceServices : byte
        {
            PermanentCLIR = 0b0001_0000,
            ThirdPartyConference = 0b0000_1000,
            IntrusionCall = 0b0000_0100,
            CallDeflection = 0b0000_0010,
            MultipleLines = 0b0000_0001,
        }

        public ExtendedMacCapability MacCapabilities { get; }

        public ExtendedHigherLayerCapabilities HigherLayerCapabilities { get; }

        public DprsDataCategory DataCategory { get; }

        public ExtendedVoiceServices ExtendedVoiceServicesSupported { get; }

        public override ReadOnlyMemory<byte> Raw => base.Raw.Slice(5);

        public ExtendedCapabilities2RfpcValue(ReadOnlyMemory<byte> data):base(RfpcKey.ExtendedCapabilities2, data)
        {
            var span = data.Span;
            MacCapabilities = (ExtendedMacCapability) (((span[0] & 0xf) << 8) | span[1]);
            var bits = BinaryPrimitives.ReadInt32BigEndian(span.Slice(1));
            HigherLayerCapabilities = (ExtendedHigherLayerCapabilities) (bits & 0x00841FFF);
            DataCategory = (DprsDataCategory) ((bits >> 19) & 0xf);
            ExtendedVoiceServicesSupported = (ExtendedVoiceServices) ((bits >> 13) & 0x1f);
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" MacCapabilities({MacCapabilities:G}) HigherLayerCapabilities({HigherLayerCapabilities:G}) DPRSCat({DataCategory}) ExtendedVoiceServices({ExtendedVoiceServicesSupported})");
        }
    }
}