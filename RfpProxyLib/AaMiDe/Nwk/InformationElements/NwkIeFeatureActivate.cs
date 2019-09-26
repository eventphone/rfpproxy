using System;
using System.IO;

namespace RfpProxyLib.AaMiDe.Nwk.InformationElements
{
    public sealed class NwkIeFeatureActivate : NwkVariableLengthInformationElement
    {
        public enum FeatureType
        {
            RegisterRecall = 0b000_0001,

            /// <summary>
            /// indication from the PT to the FT that the call shall be immediately rerouted.
            /// </summary>
            ExternalHandoverSwitch = 0b000_1111,

            /// <summary>
            /// request to enter outgoing call queue
            /// </summary>
            QueueEntryRequest = 0b010_0000,

            /// <summary>
            /// indication to the user of the subscriber number allocated to the user,
            /// e.g. during a temporary registration on a visited network.
            /// </summary>
            IndicationOfSubscriberNumber = 0b011_0000,
            FeatureKey = 0b100_0010,

            /// <summary>
            /// the ability to select a specific line (internal or external) on which to make or receive a call.
            /// </summary>
            SpecificLineSelection = 0b100_0100,

            /// <summary>
            /// the ability to select a specific trunk carrier for a call through a global network.
            /// </summary>
            SpecificTrunkCarrierSelection = 0b100_0111,

            /// <summary>
            /// the ability to connect or disconnect FP echo control functions, depending on
            /// e.g. the type of service and call routing information.
            /// </summary>
            ControlOfEchoControlFunctions = 0b100_1000,

            /// <summary>
            /// indication to the user of the call charge or call tariff. It may be used to invoke activation of this feature for all calls or on call-by-call basis.
            /// </summary>
            CostInformation = 0b110_0000,
        }

        public override bool HasUnknown { get; }

        public override ReadOnlyMemory<byte> Raw { get; }

        public FeatureType Feature { get; }

        public byte Parameter { get; }

        public NwkIeFeatureActivate(ReadOnlyMemory<byte> data):base(NwkVariableLengthElementType.FeatureActivate, data)
        {
            var span = data.Span;
            Feature = (FeatureType) (span[0] & 0x7f);
            switch (Feature)
            {
                case FeatureType.RegisterRecall:
                case FeatureType.ExternalHandoverSwitch:
                case FeatureType.QueueEntryRequest:
                case FeatureType.IndicationOfSubscriberNumber:
                    Raw = data.Slice(1);
                    HasUnknown = !Raw.IsEmpty;
                    break;
                case FeatureType.FeatureKey:
                case FeatureType.SpecificLineSelection:
                case FeatureType.SpecificTrunkCarrierSelection:
                case FeatureType.ControlOfEchoControlFunctions:
                case FeatureType.CostInformation:
                default:
                    Parameter = span[1];
                    Raw = data.Slice(2);
                    HasUnknown = !Raw.IsEmpty;
                    break;
            }
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" Feature({Feature})");
            switch (Feature)
            {
                case FeatureType.FeatureKey:
                case FeatureType.SpecificLineSelection:
                case FeatureType.SpecificTrunkCarrierSelection:
                case FeatureType.ControlOfEchoControlFunctions:
                case FeatureType.CostInformation:
                    writer.Write($" Parameter({Parameter})");
                    break;
            }
            if (HasUnknown)
                writer.Write($" Reserved({Raw.ToHex()})");
        }
    }
}