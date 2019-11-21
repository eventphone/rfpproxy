namespace RfpProxy.AaMiDe.Nwk
{
    public enum NwkMMMessageType : byte
    {
        AuthenticationRequest = 0b0100_0000,//64
        AuthenticationReply = 0b0100_0001,//65
        KeyAllocate = 0b0100_0010,//66
        AuthenticationReject = 0b0100_0011,//67
        AccessRightsRequest = 0b0100_0100,//68
        AccessRightsAccept = 0b0100_0101,//69
        AccessRightsReject = 0b0100_0111,//71
        AccessRightsTerminateRequest = 0b0100_1000,//72
        AccessRightsTerminateAccept = 0b0100_1001,//73
        AccessRightsTerminateReject = 0b0100_1011,//75
        CipherRequest = 0b0100_1100,//76
        CipherSuggest = 0b0100_1110,//78
        CipherReject = 0b0100_1111,//79
        MMInfoRequest = 0b0101_0000,//80
        MMInfoAccept = 0b0101_0001,//81
        MMInfoSuggest = 0b0101_0010,//82
        MMInfoReject = 0b0101_0011,//83
        LocateRequest = 0b0101_0100,//84
        LocateAccept = 0b0101_0101,//85
        Detach = 0b0101_0110,//86
        LocateReject = 0b0101_0111,//87
        IdentityRequest = 0b0101_1000,//88
        IdentityReply = 0b0101_1001,//89
        MMIwu = 0b0101_1011,//91
        TemporaryIdentityAssign = 0b0101_1100,//92
        TemporaryIdentityAssignAck = 0b0101_1101,//93
        TemporaryIdentityAssignRej = 0b0101_1111,//95
        MMNotify = 0b0110_1110,//110
    }
}