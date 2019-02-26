namespace RfpProxy.Log.Messages.Nwk.InformationElements
{
    public enum NwkDoubleByteElementType : byte
    {
        BasicService = 0b0000,
        ReleaseReason = 0b0010,
        Signal = 0b0100,
        TimerRestart = 0b0101,
        TestHookControl = 0b0110,
        SingleDisplay = 0b1000,
        SingleKeypad = 0b1001,
        Reserved = 0b1111,
    }
}