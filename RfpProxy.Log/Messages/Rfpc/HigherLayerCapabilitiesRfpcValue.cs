using System;
using System.Buffers.Binary;
using System.IO;

namespace RfpProxy.Log.Messages.Rfpc
{
    public sealed class HigherLayerCapabilitiesRfpcValue : DnmRfpcValue
    {
        /// <summary>
        /// ETSI EN 300 175-5 Annex F
        /// </summary>
        [Flags]
        public enum HigherLayerCapabilities : ushort
        {
            AdpcmG726VoiceService = 0b1000_0000_0000_0000, 
            GapBasicSpeech = 0b0100_0000_0000_0000,
            NonVoiceCircuitSwitchedService = 0b0010_0000_0000_0000,
            NonVoicePacketSwitchedService = 0b0001_0000_0000_0000,
            DSAARequired = 0b0000_1000_0000_0000,
            DSCSupported = 0b0000_0100_0000_0000,
            LocationRegistrationSupported = 0b0000_0010_0000_0000,
            SimServicesAvailable = 0b0000_0001_0000_0000,
            NonStaticFP = 0b0000_0000_1000_0000,
            CissServicesAvailable = 0b0000_0000_0100_0000,
            ClmsServiceAvailable = 0b0000_0000_0010_0000,
            ComsServiceAvailable = 0b0000_0000_0001_0000,
            AccessRightsRequestsSupported = 0b0000_0000_0000_1000,
            ExternalHandoverSupported = 0b0000_0000_0000_0100,
            ConnectionHandoverSupported = 0b0000_0000_0000_0010,
            Reserved = 0b0000_0000_0000_0001,
        }

        public HigherLayerCapabilities Capabilities { get; }

        public override ReadOnlyMemory<byte> Raw => base.Raw.Slice(2);

        public HigherLayerCapabilitiesRfpcValue(ReadOnlyMemory<byte> data):base(RfpcKey.HigherLayerCapabilities, data)
        {
            Capabilities = (HigherLayerCapabilities) BinaryPrimitives.ReadUInt16BigEndian(data.Span);
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" {Capabilities:G}");
        }
    }
}