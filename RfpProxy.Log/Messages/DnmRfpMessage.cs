using System;
using System.Collections.Generic;
using System.IO;
using RfpProxyLib;

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
        MacCapabilities = 0x0d,
        StatisticDataReset = 0x0f,
        RfpFu6WindowSize = 0x12,
        RfpSiteLocation = 0x26,
        RfpPli = 0x27,
        ReflectingEnvironment = 0x28,
    }

    public sealed class DnmRfpcMessage : AaMiDeMessage
    {
        public DnmLayer Layer { get; }

        public DnmRfpcType DnmType { get; }

        public Dictionary<RfpcKey, ReadOnlyMemory<byte>> Values { get; }

        public DnmRfpcMessage(ReadOnlyMemory<byte> data) : base(MsgType.DNM, data)
        {
            var span = base.Raw.Span;
            Layer = (DnmLayer) span[0];
            DnmType = (DnmRfpcType) span[1];
            Values = new Dictionary<RfpcKey, ReadOnlyMemory<byte>>();

            var payload = Raw;
            while (payload.Length > 0)
            {
                var key = (RfpcKey) payload.Span[0];
                var length = payload.Span[1];
                var value = payload.Slice(2, length);
                Values.Add(key, value);
                payload = payload.Slice(2).Slice(length);
            }
        }

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(2);

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Layer({Layer,-3:G}) Type({DnmType,-20:G})");
            foreach (var kvp in Values)
            {
                writer.Write($" {kvp.Key}={kvp.Value.ToHex()}");
            }
        }
    }
}