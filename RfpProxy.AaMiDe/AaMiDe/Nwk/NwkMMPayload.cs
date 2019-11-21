using System;

namespace RfpProxy.AaMiDe.Nwk
{
    public class NwkMMPayload : NwkSFormatPayload
    {
        public NwkMMMessageType Type { get; }

        public NwkMMPayload(byte ti, bool f, ReadOnlyMemory<byte> data) : base(NwkProtocolDiscriminator.MM, ti, f, data.Slice(1))
        {
            Type = (NwkMMMessageType) data.Span[0];
            switch (Type)
            {
                case NwkMMMessageType.AuthenticationRequest:
                case NwkMMMessageType.AuthenticationReply:
                case NwkMMMessageType.KeyAllocate:
                case NwkMMMessageType.AuthenticationReject:
                case NwkMMMessageType.AccessRightsRequest:
                case NwkMMMessageType.AccessRightsAccept:
                case NwkMMMessageType.AccessRightsReject:
                case NwkMMMessageType.AccessRightsTerminateRequest:
                case NwkMMMessageType.AccessRightsTerminateAccept:
                case NwkMMMessageType.AccessRightsTerminateReject:
                case NwkMMMessageType.CipherRequest:
                case NwkMMMessageType.CipherSuggest:
                case NwkMMMessageType.CipherReject:
                case NwkMMMessageType.MMInfoRequest:
                case NwkMMMessageType.MMInfoAccept:
                case NwkMMMessageType.MMInfoSuggest:
                case NwkMMMessageType.MMInfoReject:
                case NwkMMMessageType.LocateRequest:
                case NwkMMMessageType.LocateAccept:
                case NwkMMMessageType.Detach:
                case NwkMMMessageType.LocateReject:
                case NwkMMMessageType.IdentityRequest:
                case NwkMMMessageType.IdentityReply:
                case NwkMMMessageType.MMIwu:
                case NwkMMMessageType.TemporaryIdentityAssign:
                case NwkMMMessageType.TemporaryIdentityAssignAck:
                case NwkMMMessageType.TemporaryIdentityAssignRej:
                case NwkMMMessageType.MMNotify:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override string MessageType => Type.ToString("G");
    }
}