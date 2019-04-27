using System;
using System.IO;

namespace RfpProxy.Log.Messages.Dnm
{
    public abstract class DnmPayload
    {
        public virtual bool HasUnknown => !Raw.IsEmpty;

        public virtual ReadOnlyMemory<byte> Raw { get; }

        protected DnmPayload(ReadOnlyMemory<byte> data)
        {
            Raw = data;
        }

        public static DnmPayload Create(DnmLayer layer, DnmType type, ReadOnlyMemory<byte> data, NwkReassembler reassembler)
        {
            switch (layer)
            {
                case DnmLayer.Mac:
                    return CreateMac(type, data, reassembler);
                case DnmLayer.Lc:
                    return CreateLc(type, data, reassembler);
                default:
                    return new UnknownDnmPayload(data);
            }
        }

        private static DnmPayload CreateLc(DnmType type, ReadOnlyMemory<byte> data, NwkReassembler reassembler)
        {
            if (data.Length <= 1)
                return new EmptyLcPayload(data);
            switch (type)
            {
                case DnmType.LcDataReq:
                case DnmType.LcDataInd:
                    return new LcDataPayload(data, reassembler);
                default:
                    return new UnknowLcPayload(data);
            }
        }

        private static DnmPayload CreateMac(DnmType type, ReadOnlyMemory<byte> data, NwkReassembler reassembler)
        {
            switch (type)
            {
                case DnmType.MacConInd:
                    return new MacConIndPayload(data);
                case DnmType.MacDisInd:
                    reassembler.Clear();
                    return new MacDisIndPayload(data);
                case DnmType.MacDisReq:
                    return new EmptyDnmPayload(data);
                case DnmType.MacEncKeyReq:
                    return new MacEncKeyReqPayload(data);
                case DnmType.MacEncEksInd:
                    return new MacEncEksIndPayload(data);
                case DnmType.MacInfoInd:
                    return new MacInfoIndPayload(data);
                case DnmType.HoInProgressInd:
                    return new MacHoInProgressIndPayload(data);
                case DnmType.HoInProgressRes:
                    return new MacHoInProgressResPayload(data);
                case DnmType.HoFailedInd:
                    return new MacHoFailedIndPayload(data);
                default:
                    return new UnknownDnmPayload(data);
            }
        }

        public abstract void Log(TextWriter writer);
    }
}