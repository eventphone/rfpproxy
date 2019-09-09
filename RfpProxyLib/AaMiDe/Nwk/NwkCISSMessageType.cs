namespace RfpProxyLib.AaMiDe.Nwk
{
    public enum NwkCISSMessageType : byte
    {
        CISSReleaseCom = 0b0101_1010, //90
        CISSFacility = 0b0110_0010, //98
        CISSRegister = 0b0110_0100, //100

        CRSSHold = 0b0010_0100, //36
        CRSSHoldAck = 0b0010_1000, //40
        CRSSHoldReject = 0b0011_0000, //48
        CRSSRetrieve = 0b0011_0001, //49
        CRSSRetrieveAck = 0b0011_0011, //51
        CRSSRetrieveReject = 0b0011_0111, //55
        CRSSFacility = 0b0110_0010, //98
    }
}