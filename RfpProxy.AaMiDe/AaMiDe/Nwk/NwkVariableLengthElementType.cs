﻿namespace RfpProxy.AaMiDe.Nwk
{
    public enum NwkVariableLengthElementType : byte
    {
        InfoType = 0b00000001 ,
        IdentityType = 0b00000010 ,
        PortableIdentity = 0b00000101 ,
        FixedIdentity = 0b00000110 ,
        LocationArea = 0b00000111 ,
        NwkAssignedIdentity = 0b00001001 ,
        AuthType = 0b00001010 ,
        AllocationType = 0b00001011 ,
        RAND = 0b00001100 ,
        RES = 0b00001101 ,
        RS = 0b00001110 ,
        IWUAttributes = 0b00010010 ,
        CallAttributes = 0b00010011 ,
        ServiceChangeInfo = 0b00010110 ,
        ConnectionAttributes = 0b00010111 ,
        CipherInfo = 0b00011001 ,
        CallIdentity = 0b00011010 ,
        ConnectionIdentity = 0b00011011 ,
        Facility = 0b00011100 ,
        ProgressIndicator = 0b00011110 ,
        MMSGenericHeader = 0b00100000 ,
        MMSObjectHeader = 0b00100001 ,
        MMSExtendedHeader = 0b00100010 ,
        TimeDate = 0b00100011 ,
        MultiDisplay = 0b00101000 ,
        MultiKeypad = 0b00101100 ,
        FeatureActivate = 0b00111000 ,
        FeatureIndicate = 0b00111001 ,
        NetworkParameter = 0b01000001 ,
        ExtHOindicator = 0b01000010 ,
        ZAPfield = 0b01010010 ,
        ServiceClass = 0b01010100 ,
        Key = 0b01010110 ,
        RejectReason = 0b01100000 ,
        SetupCapability = 0b01100010 ,
        TerminalCapability = 0b01100011 ,
        End2EndCompatibility = 0b01100100 ,
        RateParameters = 0b01100101 ,
        TransitDelay = 0b01100110 ,
        WindowSize = 0b01100111 ,
        CallingPartyNumber = 0b01101100 ,
        CallingPartyName = 0b01101101 ,
        CalledPartyNumber = 0b01110000 ,
        CalledPartySubaddr = 0b01110001 ,
        Duration = 0b01110010 ,
        CalledPartyName = 0b01110011 ,
        SegmentedInfo = 0b01110101 ,
        Alphanumeric = 0b01110110 ,
        IWU2IWU = 0b01110111 ,
        ModelIdentifier = 0b01111000 ,
        IWUPacket = 0b01111010 ,
        Escape2Proprietary = 0b01111011 ,
        CodecList = 0b01111100 ,
        EventsNotification = 0b01111101 ,
        CallInformation = 0b01111110 ,
        EscapeForExtension = 0b01111111
    }
}