using System;

namespace RfpProxy.Log.Messages.Nwk
{
    public class NwkCOMSPayload : NwkSFormatPayload
    {
        public NwkCOMSMessageType Type { get; }

        public NwkCOMSPayload(byte ti, bool f, ReadOnlyMemory<byte> data) : base(NwkProtocolDiscriminator.COMS, ti, f, data.Slice(1))
        {
            Type = (NwkCOMSMessageType) data.Span[0];
            switch (Type)
            {
                case NwkCOMSMessageType.Setup:
                case NwkCOMSMessageType.Connect:
                case NwkCOMSMessageType.Notify:
                case NwkCOMSMessageType.Release:
                case NwkCOMSMessageType.ReleaseCom:
                case NwkCOMSMessageType.Info:
                case NwkCOMSMessageType.Ack:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override string MessageType => Type.ToString("G");
    }
}