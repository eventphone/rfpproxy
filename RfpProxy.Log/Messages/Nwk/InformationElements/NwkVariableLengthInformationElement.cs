﻿using System;
using System.IO;

namespace RfpProxy.Log.Messages.Nwk.InformationElements
{
    public abstract class NwkVariableLengthInformationElement:NwkInformationElement
    {
        public NwkVariableLengthElementType Type { get; }

        protected NwkVariableLengthInformationElement(NwkVariableLengthElementType type)
        {
            Type = type;
        }

        public static NwkVariableLengthInformationElement Create(NwkVariableLengthElementType type, ReadOnlyMemory<byte> data)
        {
            switch (type)
            {
                case NwkVariableLengthElementType.PortableIdentity:
                    return new NwkIePortableIdentity(data);
                case NwkVariableLengthElementType.FixedIdentity:
                    return new NwkIeFixedIdentity(data);
                case NwkVariableLengthElementType.Escape2Proprietary:
                    return new NwkIeEscape2Proprietary(data);
                case NwkVariableLengthElementType.MultiKeypad:
                    return new NwkIeMultiKeypad(data);
                case NwkVariableLengthElementType.CipherInfo:
                    return new NwkIeCipherInfo(data);
                case NwkVariableLengthElementType.InfoType:
                case NwkVariableLengthElementType.IdentityType:
                case NwkVariableLengthElementType.LocationAarea:
                case NwkVariableLengthElementType.NwkAssignedIdentity:
                case NwkVariableLengthElementType.AuthType:
                case NwkVariableLengthElementType.AllocationType:
                case NwkVariableLengthElementType.RAND:
                case NwkVariableLengthElementType.RES:
                case NwkVariableLengthElementType.RS:
                case NwkVariableLengthElementType.IWUAttributes:
                case NwkVariableLengthElementType.CallAttributes:
                case NwkVariableLengthElementType.ServiceChangeInfo:
                case NwkVariableLengthElementType.ConnectionAttributes:
                case NwkVariableLengthElementType.CallIdentity:
                case NwkVariableLengthElementType.ConnectionIdentity:
                case NwkVariableLengthElementType.Facility:
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
                case NwkVariableLengthElementType.RejectReason:
                case NwkVariableLengthElementType.SetupCapability:
                case NwkVariableLengthElementType.TerminalCapability:
                case NwkVariableLengthElementType.End2EndCompatibility:
                case NwkVariableLengthElementType.RateParameters:
                case NwkVariableLengthElementType.TransitDelay:
                case NwkVariableLengthElementType.WindowSize:
                case NwkVariableLengthElementType.CallingPartyNumber:
                case NwkVariableLengthElementType.CallingPartyName:
                case NwkVariableLengthElementType.CalledPartyNumber:
                case NwkVariableLengthElementType.CalledPartySubaddr:
                case NwkVariableLengthElementType.Duration:
                case NwkVariableLengthElementType.CalledPartyName:
                case NwkVariableLengthElementType.SegmentedInfo:
                case NwkVariableLengthElementType.Alphanumeric:
                case NwkVariableLengthElementType.IWU2IWU:
                case NwkVariableLengthElementType.ModelIdentifier:
                case NwkVariableLengthElementType.IWUPacket:
                case NwkVariableLengthElementType.CodecList:
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
        }
    }
}