namespace RfpProxy.Log.Messages.Sync
{
    public enum SyncMessageType : ushort
    {
        GetReqRssiCompInd = 0x7d0e,
        GetReqRssiCompCfm = 0x7d0f,
        FreqCtrlModeInd = 0x7d15,
        FreqCtrlModeCfm = 0x7d16,
        SetFrequency = 0x7d18,
        SetReportLimit = 0x7d1a,
        ResetMacInd = 0x7d1b,
        StartMacMasterInd = 0x7d1c,
        StartMacSlaveModeInd = 0x7d1d,
        SystemSearchInd = 0x7d1e,
        SystemSearchCfm = 0x7d1f,
        ResetMacCfm = 0x7d21,
        StartMacMasterCfm = 0x7d22,
        StartMacSlaveModeCfm= 0x7d24,
        PhaseOfsWithRssiInd = 0x7d2c,
    }
}