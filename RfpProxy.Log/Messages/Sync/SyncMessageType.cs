namespace RfpProxy.Log.Messages.Sync
{
    public enum SyncMessageType : ushort
    {
        GetReqRssiCompInd = 0x7d0e,
        GetReqRssiCompCfm = 0x7d0f,
        FreqCtrlModeInd = 0x7d15,
        FreqCtrlModeCfm = 0x7d16,
        SetFrequency = 0x7d18,
        ResetMacInd = 0x7d1b,
        StartMacMasterInd = 0x7d1c,
        SystemSearchInd = 0x7d1e,
        SystemSearchCfm = 0x7d1f,
        ResetMacCfm = 0x7d21,
        StartMacMasterCfm = 0x7d22,
        PhaseOfsWithRssiInd = 0x7d2c,
    }
}