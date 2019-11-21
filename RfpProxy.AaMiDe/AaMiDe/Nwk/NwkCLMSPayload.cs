using System;

namespace RfpProxy.AaMiDe.Nwk
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
                if ((data.Span[1] & 0x08) != 0)
                {
                    //address
                    throw new NotImplementedException();
                }
                else
                {
                    return new NwkCLMSFixedDataPlayload(ti, f, data.Slice(1));
                }
            }
        }
    }
}