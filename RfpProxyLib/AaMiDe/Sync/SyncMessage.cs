using System;
using System.Buffers.Binary;
using System.IO;

namespace RfpProxyLib.AaMiDe.Sync
{
    public abstract class SyncMessage : AaMiDeMessage
    {
        public SyncMessageType SyncType { get; }

        public byte PayloadLength { get; }

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(3);

        protected SyncMessage(SyncMessageType type, ReadOnlyMemory<byte> data) : base(MsgType.SYNC, data)
        {
            SyncType = type;
            PayloadLength = base.Raw.Span[2];
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            if (Enum.IsDefined(typeof(SyncMessageType), SyncType))
                writer.Write($"{SyncType:G}");
            else
                writer.Write($"{SyncType:x}");
            writer.Write($"({PayloadLength}):");
        }

        public static SyncMessage Create(ReadOnlyMemory<byte> data)
        {
            var type = (SyncMessageType)BinaryPrimitives.ReadUInt16BigEndian(data.Span.Slice(4));
            switch (type)
            {
                case SyncMessageType.SetFrequency:
                    return new SetFrequencySyncMessage(data);
                case SyncMessageType.PhaseOfsWithRssiInd:
                    return new OffsetIndSyncMessage(data);
                case SyncMessageType.ResetMacInd:
                case SyncMessageType.ResetMacCfm:
                case SyncMessageType.GetReqRssiCompInd:
                case SyncMessageType.StartMacMasterInd:
                case SyncMessageType.StartMacMasterCfm:
                case SyncMessageType.StartMacSlaveModeCfm:
                    return new EmptySyncMessage(type, data);
                case SyncMessageType.GetReqRssiCompCfm:
                    return new GetReqRssiCompCfmSyncMessage(data);
                case SyncMessageType.SystemSearchInd:
                    return new SystemSearchIndSyncMessage(data);
                case SyncMessageType.SystemSearchCfm:
                    return new SystemSearchCfmSyncMessage(data);
                case SyncMessageType.FreqCtrlModeInd:
                    return new FreqCtrlModeIndSyncMessage(data);
                case SyncMessageType.FreqCtrlModeCfm:
                    return new FreqCtrlModeCfmSyncMessage(data);
                case SyncMessageType.SetReportLimit:
                    return new SetReportLimitSyncMessage(data);
                case SyncMessageType.StartMacSlaveModeInd:
                    return new StartMacSlaveModeIndSyncMessage(data);
                case (SyncMessageType)32042:
                case (SyncMessageType)32025:
                    return new UnknownSyncMessage(type, data);//todo
                default:
                    return new UnknownSyncMessage(type, data);
            }
        }
    }
}