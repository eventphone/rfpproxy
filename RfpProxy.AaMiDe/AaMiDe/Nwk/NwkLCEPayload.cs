using System;

namespace RfpProxy.AaMiDe.Nwk
{
    public abstract class NwkLCEPayload : NwkPayload
    {
        protected NwkLCEPayload(NwkProtocolDiscriminator pd, byte ti, bool f):base(pd, ti, f)
        {
        }

        public static NwkPayload Create(byte ti, bool f, ReadOnlyMemory<byte> data)
        {
            var type = data.Span[0];
            if (Enum.IsDefined(typeof(NwkLCEMessageType), type))
            {
                return new NwkLCESFormatPayload(ti, f, data);
            }
            else
            {
                throw new NotImplementedException("LceRequestPage");
            }
        }
    }
}