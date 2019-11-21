namespace RfpProxy.AaMiDe.Nwk
{
    public abstract class NwkCLMSFixedPayload : NwkCLMSPayload
    {
        protected NwkCLMSFixedPayload(byte ti, bool f) : base(NwkProtocolDiscriminator.CLMS, ti, f)
        {
        }
    }
}