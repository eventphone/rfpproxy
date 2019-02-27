using System.IO;

namespace RfpProxy.Log.Messages.Nwk.InformationElements
{
    public sealed class NwkIeReleaseReason : NwkDoubleByteInformationElement
    {
        public enum ReleaseReason : byte
        {
            Normal = 0x00,
            UnexpectedMessage = 0x01,
            UnknownTransactionIdentifier = 0x02,
            MandatoryInformationElementMissing = 0x03,
            InvalidInformationElementContents = 0x04,
            IncompatibleService = 0x05,
            ServiceNotImplemented = 0x06,
            NegotiationNotSupported = 0x07,
            InvalidIdentity = 0x08,
            AuthenticationFailed = 0x09,
            UnknownIdentity = 0x0a,
            NegotiationFailed = 0x0b,
            TimerExpiry = 0x0d,
            PartialRelease = 0x0e,
            Unknown = 0x0f,
            UserDetached = 0x10,
            UserNotInRange = 0x11,
            UserUnknown = 0x12,
            UserAlreadyActive = 0x13,
            UserBusy = 0x14,
            UserRejection = 0x15,
            UserCallModify = 0x16,
            ExternalHandoverNotSupported = 0x21,
            NetworkParametersMissing = 0x22,
            ExternalHandoverRelease = 0x23,
            Overload = 0x31,
            InsufficientResources = 0x32,
            InsufficientBearersAvailable = 0x33,
            IwuCongestion = 0x34,
            SecurityAttackAssumed = 0x40,
            EncryptionActivatio = 0x41,
        }

        public ReleaseReason Reason { get; }

        public override bool HasUnknown => false;

        public NwkIeReleaseReason(byte content):base(NwkDoubleByteElementType.ReleaseReason)
        {
            Reason = (ReleaseReason) content;
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" Reason({Reason:G})");
        }
    }
}