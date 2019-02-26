namespace RfpProxy.Log.Messages.Nwk
{
    public enum NwkCOMSMessageType : byte
    {
        Setup = 0b0000_0101, //5
        Connect = 0b0000_0111, //7
        Notify = 0b0000_1000, //8
        Release = 0b0100_1101, //77
        ReleaseCom = 0b0101_1010, //90
        Info = 0b0111_1011, //123
        Ack = 0b0111_1000, //120
    }
}