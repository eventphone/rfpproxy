using System;

namespace RfpProxy.AaMiDe.Nwk
{
    public class NwkLCESFormatPayload : NwkSFormatPayload
    {
        public NwkLCEMessageType Type { get; }

        public NwkLCESFormatPayload(byte ti, bool f, ReadOnlyMemory<byte> data) : base(NwkProtocolDiscriminator.LCE, ti, f, data.Slice(1))
        {
            Type = (NwkLCEMessageType)data.Span[0];
            switch (Type)
            {
                case NwkLCEMessageType.PageReject:
                case NwkLCEMessageType.PageResponse:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override string MessageType => Type.ToString("G");
    }
}