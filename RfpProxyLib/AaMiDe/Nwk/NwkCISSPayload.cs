using System;

namespace RfpProxyLib.AaMiDe.Nwk
{
    public class NwkCISSPayload : NwkSFormatPayload
    {
        public NwkCISSMessageType Type { get; }

        public NwkCISSPayload(byte ti, bool f, ReadOnlyMemory<byte> data) : base(NwkProtocolDiscriminator.CISS, ti, f, data.Slice(1))
        {
            Type = (NwkCISSMessageType) data.Span[0];
            switch (Type)
            {
                case NwkCISSMessageType.CISSReleaseCom:
                case NwkCISSMessageType.CISSFacility:
                case NwkCISSMessageType.CISSRegister:
                case NwkCISSMessageType.CRSSHold:
                case NwkCISSMessageType.CRSSHoldAck:
                case NwkCISSMessageType.CRSSHoldReject:
                case NwkCISSMessageType.CRSSRetrieve:
                case NwkCISSMessageType.CRSSRetrieveAck:
                case NwkCISSMessageType.CRSSRetrieveReject:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override string MessageType => Type.ToString("G");
    }
}