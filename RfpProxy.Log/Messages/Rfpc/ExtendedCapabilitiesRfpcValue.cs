using System;
using System.Buffers.Binary;
using System.IO;

namespace RfpProxy.Log.Messages.Rfpc
{
    public sealed class ExtendedCapabilitiesRfpcValue : DnmRfpcValue
    {
        public enum SynchronizationField : byte
        {
            Standard = 0b0000,
            Prolonged = 0b0001,
        }

        /// <summary>
        /// ETSI EN 300 175-5 Annex F.2
        /// </summary>
        [Flags]
        public enum ExtendedHigherLayerCapabilities : int
        {
            IsdnDataServices                    = 0b0000_0000_0000_0000_0001,
            DprsClass2                          = 0b0000_0000_0000_0000_0010,
            DprsClass3OrClass4                  = 0b0000_0000_0000_0000_0100,
            DataServiceProfileD                 = 0b0000_0000_0000_0000_1000,
            Lrms                                = 0b0000_0000_0000_0001_0000,
            AsymmetricBearersSupported          = 0b0000_0000_0000_0010_0000,
            EmergencyCallSupported              = 0b0000_0000_0000_0100_0000,
            LocationRegistrationWithTpuiAllowed = 0b0000_0000_0000_1000_0000,
            SynchronizationToGpsAchieved        = 0b0000_0000_0001_0000_0000,
            IsdnIntermediateSystem              = 0b0000_0000_0010_0000_0000,
            RapPart1Profile                     = 0b0000_0000_0100_0000_0000,
            V24                                 = 0b0000_0000_1000_0000_0000,
            Ppp                                 = 0b0000_0001_0000_0000_0000,
            Ip                                  = 0b0000_0010_0000_0000_0000,
            TokenRing                           = 0b0000_0100_0000_0000_0000,
            Ethernet                            = 0b0000_1000_0000_0000_0000,
            IpRoamingUnrestrictedSupported      = 0b0001_0000_0000_0000_0000,
            DprsSupported                       = 0b0010_0000_0000_0000_0000,
            BasicOdapSupported                  = 0b0100_0000_0000_0000_0000,
            FMmsInterworkingProfileSupported    = 0b1000_0000_0000_0000_0000,
        }

        public byte WirelessRelayStations { get; }

        public SynchronizationField Synchronization { get; }

        public bool FrequencyReplacementSupported { get; }

        public bool MacSuspendResume { get; }

        public bool IpqServicesSupported { get; }

        public bool ExtendedFPInfo2 { get; }

        public ExtendedHigherLayerCapabilities Capabilities { get; }

        public override bool HasUnknown => WirelessRelayStations != 0;

        public override ReadOnlyMemory<byte> Raw => base.Raw.Slice(5);

        public ExtendedCapabilitiesRfpcValue(ReadOnlyMemory<byte> data):base(RfpcKey.ExtendedCapabilities, data)
        {
            var span = data.Span;
            WirelessRelayStations = (byte) (((span[0] & 0xf) << 2) | (span[1] >> 6));
            Synchronization = (SynchronizationField) ((span[1] & 0b0011_0000) >> 4);
            FrequencyReplacementSupported = (span[1] & 0b1000) != 0;
            MacSuspendResume = (span[1] & 0b0100) != 0;
            IpqServicesSupported = (span[1] & 0b0010) != 0;
            ExtendedFPInfo2 = (span[1] & 0b0001) != 0;
            Capabilities = (ExtendedHigherLayerCapabilities) (BinaryPrimitives.ReadInt32BigEndian(span.Slice(1)) & 0x7fffff);
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" WirelessRelayStations({WirelessRelayStations:x2}) Synchronization({Synchronization:G})");
            writer.Write($" FrequencyReplacementSupported({FrequencyReplacementSupported}) MacSuspendResume({MacSuspendResume}) IpqServicesSupported({IpqServicesSupported}) ExtendedFPInfo2({ExtendedFPInfo2})");
            writer.Write($" HigherLayerCapabilities({Capabilities:G})");
        }
    }
}