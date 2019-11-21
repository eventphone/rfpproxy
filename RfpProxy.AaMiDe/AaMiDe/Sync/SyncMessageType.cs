namespace RfpProxy.AaMiDe.Sync
{
    public enum SyncMessageType : ushort
    {
        GetReqRssiCompInd = 0x7d0e,
        GetReqRssiCompCfm = 0x7d0f,
        FreqCtrlModeInd = 0x7d15,
        FreqCtrlModeCfm = 0x7d16,
        PhaseOffsetInd = 0x7d17,
        SetFrequency = 0x7d18,
        SetReportLimit = 0x7d1a,
        ResetMacInd = 0x7d1b,
        StartMacMasterInd = 0x7d1c,
        StartMacSlaveModeInd = 0x7d1d,
        SystemSearchInd = 0x7d1e,
        SystemSearchCfm = 0x7d1f,
        MacStartedInd = 0x7d20,
        ResetMacCfm = 0x7d21,
        StartMacMasterCfm = 0x7d22,
        StartMacMasterRej = 0x7d23,
        StartMacSlaveModeCfm = 0x7d24,
        StartMacSlaveRej = 0x7d25,
        SystemSearchRej = 0x7d26,
        ReadyForSyncInd = 0x7d27,
        GetActiveChannelCfm = 0x7d29,
        PhaseOfsWithRssiInd = 0x7d2c,
        ResetMacIfIdleCfm = 0x7d2f,
        UnknownReadyForSync = 0x7d32,
        UnknownStandby = 0x7d33,
    }
}