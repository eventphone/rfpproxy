using System;
using System.IO;

namespace RfpProxy.Log.Messages.Nwk.InformationElements
{
    public sealed class NwkIeRejectReason : NwkVariableLengthInformationElement
    {
        public enum Reason : byte
        {
            UnknownTPUI = 0x01,
            UnknownIPUI = 0x02,
            UnknownNetworkAssignedIdentity = 0x03,
            IpeiNotAccepted = 0x05,
            IpuiNotAccepted = 0x06,
            AuthenticationFailed = 0x10,
            NoAuthenticationAlgorithm = 0x11,
            AuthenticationAlgorithmNotSupported = 0x12,
            AuthenticationKeyNotSupported = 0x13,
            UpiNotEntered = 0x14,
            NoCipherAlgorithm = 0x17,
            CipherAlgorithmNotSupported = 0x18,
            CipherKeyNotSupported = 0x19,
            IncompatibleService = 0x20,
            FalseLceReply = 0x21,
            LateLceReply = 0x22,
            InvalidTpui = 0x23,
            TpuiAssignmentLimitsUnacceptable = 0x24,
            InsufficientMemory = 0x2f,
            Overload = 0x30,
            TestCallBackNormalEnBloc = 0x40,
            TestCallBackNormalPiecewise = 0x41,
            TestCallBackEmergencyEnBloc = 0x42,
            TestCallBackEmergencyPiecewise = 0x43,
            InvalidMessage = 0x5f,
            InformationElementError = 0x60,
            InvalidInformationElementContents = 0x64,
            TimerExpiry = 0x70,
            PlmnNotAllowed = 0x76,
            LocationAreaNotAllowed = 0x80,
            NationalRoamingNotAllowedInThisLocationArea = 0x81,
        }

        public Reason RejectReason { get; }

        public override ReadOnlyMemory<byte> Raw => base.Raw.Slice(1);

        public NwkIeRejectReason(ReadOnlyMemory<byte> data):base(NwkVariableLengthElementType.RejectReason, data)
        {
            RejectReason = (Reason) data.Span[0];
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" Reason({RejectReason:G})");
        }
    }
}