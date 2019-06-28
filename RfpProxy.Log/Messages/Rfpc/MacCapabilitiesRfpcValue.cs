using System;
using System.IO;

namespace RfpProxy.Log.Messages.Rfpc
{
    public sealed class MacCapabilitiesRfpcValue : DnmRfpcValue
    {
        /// <summary>
        /// ETSI EN 300 175-3 V2.7.1 Table 7.14
        /// </summary>
        [Flags]
        public enum MacCapabilities //: uint
        {
            ExtendedFPInfo = 0b1000_0000_0000_0000_0000,
            DoubleDuplexBearerConnections = 0b0100_0000_0000_0000_0000,
            Reserved = 0b0010_0000_0000_0000_0000,
            DoubleSlot = 0b0001_0000_0000_0000_0000,
            HalfSlot = 0b0000_1000_0000_0000_0000,
            FullSlot = 0b0000_0100_0000_0000_0000,
            FrequencyControl = 0b0000_0010_0000_0000_0000,
            PageRepetition = 0b0000_0001_0000_0000_0000,
            COSetupOnDummyAllowed = 0b0000_0000_1000_0000_0000,
            CLUplink = 0b0000_0000_0100_0000_0000,
            CLDownlink = 0b0000_0000_0010_0000_0000,
            BasicAFieldSetUp = 0b0000_0000_0001_0000_0000,
            AdvancedAFieldSetUp = 0b0000_0000_0000_1000_0000,
            BFieldSetUp = 0b0000_0000_0000_0100_0000,
            CfMessages = 0b0000_0000_0000_0010_0000,
            InaMinimumDelay = 0b0000_0000_0000_0001_0000,
            InbNormalDelay = 0b0000_0000_0000_0000_1000,
            IpmErrorDetection = 0b0000_0000_0000_0000_0100,
            IpmrErrorCorrection = 0b0000_0000_0000_0000_0010,
            MultibearerConnections = 0b0000_0000_0000_0000_0001,
        }

        public MacCapabilities Capabilities { get; }

        public override bool HasUnknown => (int)Capabilities > 0xfffff;

        public override ReadOnlyMemory<byte> Raw => base.Raw.Slice(3);

        public MacCapabilitiesRfpcValue(ReadOnlyMemory<byte> data):base(RfpcKey.MacCapabilities, data)
        {
            var span = data.Span;
            Capabilities = (MacCapabilities) (((span[0] & 0xf) << 16) | (span[1] << 8) | (span[2]));
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" {Capabilities:G}");
        }
    }
}