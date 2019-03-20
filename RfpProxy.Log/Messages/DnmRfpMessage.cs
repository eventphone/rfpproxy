using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RfpProxy.Log.Messages.Rfpc;

namespace RfpProxy.Log.Messages
{
    public enum DnmRfpcType : byte
    {
        ReadyInd = 0x01,
        InitReq = 0x02,
        InitCfm = 0x03,
        SariListReq = 0x05,
        ChangeHigherLayerCapabilitiesReq = 0x06,
        ChangeHigherLayerCapabilitiesCfm = 0x07,
        ChangeStatusInfoReq = 0x08,
        ChangeStatusInfoCfm = 0x09,
        ActivateReq = 0x0f,
        ActivateCfm = 0x10,
        DeactivateReq = 0x11,
        DeactivateCfm = 0x12,
        ResetReq = 0x13,
        StatisticsDataReq = 0x16,
        StatisticsDataCfm = 0x17,
        ErrorInd = 0x18,
        ToRfpInd = 0x20,
        ToRfpReq = 0x21,
        TopoDataReq = 0x22,
        TopoDataInd = 0x23,
        BmcRestartReq = 0x24,
        ChangeMasterReq = 0x25,
        InfoInd = 0x26,
        ActiveInd = 0x30,
        ActiveRes = 0x31,
        PagingQueueOverflowInd = 0x32
    }

    public enum RfpcKey : byte
    {
        NumberOfUpn = 0x01,
        Revision = 0x02,
        NumberOfBearer = 0x03,
        RFPI = 0x04,
        SARI = 0x05,
        HigherLayerCapabilities = 0x06,
        ExtendedCapabilities = 0x07,
        StatusInfo = 0x08,
        MacCapabilities = 0x0d,
        StatisticDataReset = 0x0f,
        StatisticData = 0x10,
        ErrorCause = 0x11,
        RfpFu6WindowSize = 0x12,
        RfpToRfp = 0x14,
        RfpTopo = 0x15,
        LastError = 0x20,
        PabxData = 0x21,
        MoniData = 0x22,
        LastErrorExt = 0x23,
        FpgaRevision = 0x24,
        RfpString = 0x25,
        RfpSiteLocation = 0x26,
        RfpPli = 0x27,
        ReflectingEnvironment = 0x28,
        ExtendedCapabilities2 = 0x29,
        FrequencyBand = 0x2a,
        RfPower = 0x2b,
    }

    public sealed class DnmRfpcMessage : AaMiDeMessage
    {
        public DnmLayer Layer { get; }

        public DnmRfpcType DnmType { get; }

        public List<DnmRfpcValue> Values { get; }

        public override bool HasUnknown => Values.Any(x=>x.HasUnknown);

        protected override ReadOnlyMemory<byte> Raw { get; }

        public DnmRfpcMessage(ReadOnlyMemory<byte> data) : base(MsgType.DNM, data)
        {
            var span = base.Raw.Span;
            Layer = (DnmLayer) span[0];
            DnmType = (DnmRfpcType) span[1];
            Values = new List<DnmRfpcValue>();

            var payload = base.Raw.Slice(2);
            while (payload.Length > 0)
            {
                var key = (RfpcKey) payload.Span[0];
                var length = payload.Span[1];
                var value = payload.Slice(2, length);
                Values.Add(DnmRfpcValue.Create(key, value));
                payload = payload.Slice(2).Slice(length);
            }
            Raw = payload;
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Layer({Layer,-3:G}) Type({DnmType,-20:G})");
            foreach (var value in Values)
            {
                value.Log(writer);
            }
        }
    }
}