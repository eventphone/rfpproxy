namespace RfpProxy.Log.Messages.Nwk.InformationElements.Proprietary.DeTeWe
{
    public enum DeTeWeType:byte
    {
        Language = 0x16,
        Display = 0x20,
        SendText = 0x21,
        DateTime = 0x23,
        HomeScreenText = 0x45,
        Mms = 0x55,
        Reserved1 = 0x59,
        BtEthAddr = 0x62,
    }
}