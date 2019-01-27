namespace RfpProxyLib.Messages
{
    public enum MessageDirection:byte
    {
        FromOmm = 0,
        ToRfp = FromOmm,
        FromRfp = 1,
        ToOmm = FromRfp
    }
}