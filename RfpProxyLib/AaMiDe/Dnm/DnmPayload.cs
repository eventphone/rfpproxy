using RfpProxyLib.AaMiDe.Mac;
using System;
using System.IO;

namespace RfpProxyLib.AaMiDe.Dnm
{
    public abstract class DnmPayload
    {
        public virtual bool HasUnknown => !Raw.IsEmpty;

        public virtual ReadOnlyMemory<byte> Raw { get; }

        protected DnmPayload(ReadOnlyMemory<byte> data)
        {
            Raw = data;
        }

        public static DnmPayload Create(DnmLayer layer, DnmType type, ReadOnlyMemory<byte> data, MacConnection connection)
        {
            switch (layer)
            {
                case DnmLayer.Mac:
                    return CreateMac(type, data, connection);
                case DnmLayer.Lc:
                    return CreateLc(type, data, connection);
                default:
                    return new UnknownDnmPayload(data);
            }
        }

        private static DnmPayload CreateLc(DnmType type, ReadOnlyMemory<byte> data, MacConnection connection)
        {
            if (data.Length <= 1)
                return new EmptyLcPayload(data);
            switch (type)
            {
                case DnmType.LcDataReq:
                case DnmType.LcDataInd:
                    return new LcDataPayload(data, connection.Reassembler);
                default:
                    return new UnknowLcPayload(data);
            }
        }

        private static DnmPayload CreateMac(DnmType type, ReadOnlyMemory<byte> data, MacConnection connection)
        {
            switch (type)
            {
                case DnmType.MacConInd:
                    var macConInd = new MacConIndPayload(data);
                    connection.Open(macConInd);
                    return macConInd;
                case DnmType.MacDisInd:
                    connection.Close();
                    return new MacDisIndPayload(data);
                case DnmType.MacDisReq:
                    connection.Close();
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