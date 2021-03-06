﻿using System;
using System.IO;
using RfpProxy.AaMiDe.Mac;
using RfpProxy.AaMiDe.Mt;
using RfpProxy.AaMiDe.Rfpc;

namespace RfpProxy.AaMiDe.Dnm
{
    public enum DnmType : byte
    {
        MacConInd = 0x01,
        MacDisReq = 0x02,
        MacDisInd = 0x03,
        LcDataReq = 0x05,
        LcDataInd = 0x06,
        LcDtrInd = 0x07,
        MacPageReq = 0x08,
        MacEncKeyReq = 0x09,
        MacEncEksInd = 0x0a,
        HoInProgressInd = 0x0b,
        HoInProgressRes = 0x0c,
        HoFailedInd = 0x0d,
        HoFailedReq = 0x0e,
        DlcRfpErrorInd = 0x14,
        MacConExtInd = 0x15,
        HoInProgressExtInd = 0x16,
        MacModReq = 0x17,
        MacModCnf = 0x18,
        MacModInd = 0x19,
        MacModRej = 0x1a,
        MacRecordAudio = 0x1b,
        MacInfoInd = 0x1c,
        MacGetDefCkeyInd = 0x1d,
        MacGetDefCkeyRes = 0x1e,
        MacClearDefCkeyReq = 0x1f,
        MacGetCurrCkeyIdReq = 0x20,
        MacGetCurrCkeyIdCnf = 0x21,
    }

    public enum DnmLayer : byte
    {
        Rfpc = 0x78,
        Lc = 0x79,
        Mac = 0x7a,
        Unknown1 = 0x7B,
        Mt = 0x7C,
        Sync = 0x7d,
    }

    public sealed class DnmMessage : AaMiDeMessage
    {
        public DnmLayer Layer { get; }

        public DnmType DnmType { get; }

        /// <summary>
        /// MAC Connection Endpoint Identification 
        /// </summary>
        public byte MCEI { get; }

        public DnmPayload Payload { get; }

        protected override ReadOnlyMemory<byte> Raw => ReadOnlyMemory<byte>.Empty;

        public override bool HasUnknown => Payload.HasUnknown;

        public DnmMessage(ReadOnlyMemory<byte> data, RfpConnectionTracker reassembler) : base(MsgType.DNM, data)
        {
            var span = base.Raw.Span;
            Layer = (DnmLayer) span[0];
            DnmType = (DnmType) span[1];
            MCEI = span[2];
            Payload = DnmPayload.Create(Layer, DnmType, base.Raw.Slice(3), reassembler.Get(MCEI));
        }

        public static AaMiDeMessage CreateDnm(ReadOnlyMemory<byte> data, RfpConnectionTracker reassembler)
        {
            var layer = (DnmLayer) data.Span[4];
            switch (layer)
            {
                case DnmLayer.Rfpc:
                    return new DnmRfpcMessage(data);
                case DnmLayer.Mt:
                    return new DnmMtMessage(data);
                case DnmLayer.Lc:
                case DnmLayer.Mac:
                    var type = (DnmType) data.Span[5];
                    switch (type)
                    {
                        case DnmType.MacPageReq:
                            return new MacPageReqMessage(data);
                        case DnmType.MacClearDefCkeyReq:
                            return new MacClearDefCkeyReqPayload(data);
                        default:
                            return new DnmMessage(data, reassembler);
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Layer({Layer,-3:G}) Type({DnmType,-20:G}) MCEI(0x{MCEI:x2})");
            Payload.Log(writer);
        }
    }
}