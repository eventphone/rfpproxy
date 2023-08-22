namespace RfpProxy.AaMiDe.Nwk.InformationElements.Proprietary.DeTeWe
{
    public enum DeTeWeType:byte
    {
        Language = 0x16,
        Unknown1C = 0x1C,
        Display = 0x20,
        SendText = 0x21,
        DateTime = 0x23,
        HomeScreenText = 0x45,
        Reserved2 = 0x55,
        PrepareDial = 0x56,
        Reserved1 = 0x59,
        BtEthAddr = 0x62,
        Display2 = 0xa0,
    }
}