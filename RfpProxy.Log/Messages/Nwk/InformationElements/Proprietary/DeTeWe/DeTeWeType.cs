namespace RfpProxy.Log.Messages.Nwk.InformationElements.Proprietary.DeTeWe
{
    public enum DeTeWeType:byte
    {
        Display = 0x20,
        SendText = 0x21,
        DateTime = 0x23,
        HomeScreenText = 0x45,
        Reserved1 = 0x59,
        BtEthAddr = 0x62,
    }
}