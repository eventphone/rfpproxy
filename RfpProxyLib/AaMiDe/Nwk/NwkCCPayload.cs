using System;

namespace RfpProxyLib.AaMiDe.Nwk
{
    public class NwkCCPayload : NwkSFormatPayload
    {
        public NwkCCMessageType Type { get; }

        public NwkCCPayload(byte ti, bool f, ReadOnlyMemory<byte> data) : base(NwkProtocolDiscriminator.CC, ti, f, data.Slice(1))
        {
            Type = (NwkCCMessageType)data.Span[0];
            switch (Type)
            {
                case NwkCCMessageType.Reserved:
                case NwkCCMessageType.Alerting:
                case NwkCCMessageType.CallProc:
                case NwkCCMessageType.Setup:
                case NwkCCMessageType.Connect:
                case NwkCCMessageType.SetupAck:
                case NwkCCMessageType.ConnectAck:
                case NwkCCMessageType.ServiceChange:
                case NwkCCMessageType.ServiceAccept:
                case NwkCCMessageType.ServiceReject:
                case NwkCCMessageType.Release:
                case NwkCCMessageType.ReleaseCom:
                case NwkCCMessageType.IwuInfo:
                case NwkCCMessageType.Notify:
                case NwkCCMessageType.Info:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override string MessageType => Type.ToString("G");
    }
}