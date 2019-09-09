using System;
using System.Collections.Generic;
using System.IO;

namespace RfpProxyLib.AaMiDe.Nwk.InformationElements
{
    public sealed class NwkIeInfoType : NwkVariableLengthInformationElement
    {
        public enum ParameterType : byte
        {
            LocateSuggest = 0b0000000,
            AccessRightsModifySuggest = 0b0000001,
            PPAuthenticationFailure = 0b0000100,
            DynamicParametersAllocation = 0b0000110,
            ExternalHandoverParameters = 0b0001000,
            LocationArea = 0b0001001,
            HandOverReference = 0b0001010,
            Multiframe_PscnSynchronizedExternalHandoverCandidate = 0b0001011,
            ExternalHandoverCandidate = 0b0001100,
            MultiframeSynchronizedExternalHandoverCandidate = 0b0001101,
            NonSynchronizedExternalHandoverCandidate = 0b0001110,
            Multiframe_Pscn_MultiframeNumberSynchronizedExternalHandoverCandidate = 0b0001111,
            OldFixedPartIdentity = 0b0010000,
            OldNetworkAssignedIdentity = 0b0010001,
            OldNetworkAssignedLocationArea = 0b0010010,
            OldNetworkAssignedHandoverReference = 0b0010011,
            Billing = 0b0100000,
            Debiting = 0b0100001,
            CkTransfer = 0b0100010,
            HandoverFailed = 0b0100011,
            OAMCall = 0b0100100,
            DistributedCommunicationDownload = 0b0100101,
            EthernetAddress = 0b0110000,
            TockenRingAddress = 0b0110001,
            IPv4Address = 0b0110010,
            IPv6Address = 0b0110011,
            IdentityAllocation = 0b0110100,
        }

        public IList<ParameterType> InfoTypes { get; }

        public override ReadOnlyMemory<byte> Raw { get; }

        public NwkIeInfoType(ReadOnlyMemory<byte> data) : base(NwkVariableLengthElementType.InfoType, data)
        {
            InfoTypes = new List<ParameterType>();
            var span = data.Span;
            while(true)
            {
                InfoTypes.Add((ParameterType)(span[0] & 0x7f));
                if (span[0] >= 128)
                {
                    Raw = data.Slice(1);
                    break;
                }
                span = span.Slice(1);
                data = data.Slice(1);
            };
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" InfoType({String.Join(", ", InfoTypes)})");
        }
    }
}