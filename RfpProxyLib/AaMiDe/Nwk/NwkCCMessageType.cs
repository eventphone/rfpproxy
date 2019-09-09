namespace RfpProxyLib.AaMiDe.Nwk
{
    public enum NwkCCMessageType : byte
    {
        Reserved = 0,
        Alerting = 0b0000_0001, //1
        CallProc = 0b0000_0010, //2
        Setup = 0b0000_0101, //5
        Connect = 0b0000_0111, //7
        SetupAck = 0b0000_1101, //13
        ConnectAck = 0b0000_1111, //15
        ServiceChange = 0b0010_0000, //32
        ServiceAccept = 0b0010_0001, //33
        ServiceReject = 0b0010_0011, //35
        Release = 0b0100_1101, //77
        ReleaseCom = 0b0101_1010, //90
        IwuInfo = 0b0110_0000, //96
        Notify = 0b0110_1110, //110
        Info = 0b0111_1011, //123
    }
}