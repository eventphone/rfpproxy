using System;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages.Nwk.InformationElements
{
    public abstract class NwkVariableLengthInformationElement:NwkInformationElement
    {
        public NwkVariableLengthElementType Type { get; }

        public override bool HasUnknown => !Raw.IsEmpty;

        public virtual ReadOnlyMemory<byte> Raw { get; }

        protected NwkVariableLengthInformationElement(NwkVariableLengthElementType type, ReadOnlyMemory<byte> data)
        {
            Type = type;
            Raw = data;
        }

        public static NwkVariableLengthInformationElement Create(NwkVariableLengthElementType type, ReadOnlyMemory<byte> data)
        {
            switch (type)
            {
                case NwkVariableLengthElementType.AllocationType:
                    return new NwkIeAllocationType(data);
                case NwkVariableLengthElementType.AuthType:
                    return new NwkIeAuthType(data);
                case NwkVariableLengthElementType.CallingPartyNumber:
                    return new NwkIeCallingPartyNumber(data);
                case NwkVariableLengthElementType.CipherInfo:
                    return new NwkIeCipherInfo(data);
                case NwkVariableLengthElementType.CodecList:
                    return new NwkIeCodecList(data);
                case NwkVariableLengthElementType.Duration:
                    return new NwkIeDuration(data);
                case NwkVariableLengthElementType.Escape2Proprietary:
                    return new NwkIeEscape2Proprietary(data);
                case NwkVariableLengthElementType.Facility:
                    return new NwkIeFacility(data);
                case NwkVariableLengthElementType.FixedIdentity:
                    return new NwkIeFixedIdentity(data);
                case NwkVariableLengthElementType.InfoType:
                    return new NwkIeInfoType(data);
                case NwkVariableLengthElementType.IWU2IWU:
                    return new NwkIeIwu2Iwu(data);
                case NwkVariableLengthElementType.LocationArea:
                    return new NwkIeLocationArea(data);
                case NwkVariableLengthElementType.ModelIdentifier:
                    return new NwkIeModelIdentifier(data);
                case NwkVariableLengthElementType.MultiKeypad:
                    return new NwkIeMultiKeypad(data);
                case NwkVariableLengthElementType.PortableIdentity:
                    return new NwkIePortableIdentity(data);
                case NwkVariableLengthElementType.RAND:
                    return new NwkIeRand(data);
                case NwkVariableLengthElementType.RejectReason:
                    return new NwkIeRejectReason(data);
                case NwkVariableLengthElementType.RES:
                    return new NwkIeRes(data);
                case NwkVariableLengthElementType.RS:
                    return new NwkIeRs(data);
                case NwkVariableLengthElementType.TerminalCapability:
                    return new NwkIeTerminalCapability(data);
                case NwkVariableLengthElementType.IdentityType:
                case NwkVariableLengthElementType.NwkAssignedIdentity:
                case NwkVariableLengthElementType.IWUAttributes:
                case NwkVariableLengthElementType.CallAttributes:
                case NwkVariableLengthElementType.ServiceChangeInfo:
                case NwkVariableLengthElementType.ConnectionAttributes:
                case NwkVariableLengthElementType.CallIdentity:
                case NwkVariableLengthElementType.ConnectionIdentity:
                case NwkVariableLengthElementType.ProgressIndicator:
                case NwkVariableLengthElementType.MMSGenericHeader:
                case NwkVariableLengthElementType.MMSObjectHeader:
                case NwkVariableLengthElementType.MMSExtendedHeader:
                case NwkVariableLengthElementType.TimeDate:
                case NwkVariableLengthElementType.MultiDisplay:
                case NwkVariableLengthElementType.FeatureActivate:
                case NwkVariableLengthElementType.FeatureIndicate:
                case NwkVariableLengthElementType.NetworkParameter:
                case NwkVariableLengthElementType.ExtHOindicator:
                case NwkVariableLengthElementType.ZAPfield:
                case NwkVariableLengthElementType.ServiceClass:
                case NwkVariableLengthElementType.Key:
                case NwkVariableLengthElementType.SetupCapability:
                case NwkVariableLengthElementType.End2EndCompatibility:
                case NwkVariableLengthElementType.RateParameters:
                case NwkVariableLengthElementType.TransitDelay:
                case NwkVariableLengthElementType.WindowSize:
                case NwkVariableLengthElementType.CallingPartyName:
                case NwkVariableLengthElementType.CalledPartyNumber:
                case NwkVariableLengthElementType.CalledPartySubaddr:
                case NwkVariableLengthElementType.CalledPartyName:
                case NwkVariableLengthElementType.SegmentedInfo:
                case NwkVariableLengthElementType.Alphanumeric:
                case NwkVariableLengthElementType.IWUPacket:
                case NwkVariableLengthElementType.EventsNotification:
                case NwkVariableLengthElementType.CallInformation:
                case NwkVariableLengthElementType.EscapeForExtension:
                    return new NwkIeUnknown(type, data);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public override void Log(TextWriter writer)
        {
            writer.Write($"\t\t{Type,-20}:");
            if (HasUnknown && !Raw.IsEmpty)
            {
                writer.Write($" Reserved({Raw.ToHex()})");
            }
        }
    }
}