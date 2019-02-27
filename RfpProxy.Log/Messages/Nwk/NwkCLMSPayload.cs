using System;

namespace RfpProxy.Log.Messages.Nwk
{
    public abstract class NwkCLMSPayload : NwkPayload
    {
        protected NwkCLMSPayload(NwkProtocolDiscriminator pd, byte ti, bool f) : base(pd, ti, f)
        {
        }

        public static NwkCLMSPayload Create(byte ti, bool f, ReadOnlyMemory<byte> data)
        {
            if ((data.Span[0] & 1) != 0)
            {
                throw new NotImplementedException("CLMS-VARIABLE");
            }
            else
            {
                //B-Format
                return new NwkCLMSFixedPayload(ti, f, data.Slice(1));
            }
        }
    }
}