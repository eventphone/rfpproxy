using System;
using System.IO;

namespace RfpProxy.Log.Messages.Dnm
{
    public abstract class DnmPayload
    {
        public virtual ReadOnlyMemory<byte> Raw { get; }

        protected DnmPayload(ReadOnlyMemory<byte> data)
        {
            Raw = data;
        }

        public static DnmPayload Create(DnmLayer layer, DnmType type, ReadOnlyMemory<byte> data)
        {
            switch (layer)
            {
                case DnmLayer.Mac:
                    return CreateMac(type, data);
                case DnmLayer.Lc:
                    return CreateLc(type, data);
                default:
                    return new UnknownDnmPayload(data);
            }
        }

        private static DnmPayload CreateLc(DnmType type, ReadOnlyMemory<byte> data)
        {
            switch (type)
            {
                case DnmType.LcDataReq:
                case DnmType.LcDataInd:
                    return new LcDataPayload(data);
                default:
                    return new UnknowLcPayload(data);
            }
        }

        private static DnmPayload CreateMac(DnmType type, ReadOnlyMemory<byte> data)
        {
            switch (type)
            {
                case DnmType.MacConInd:
                    return new MacConIndPayload(data);
                case DnmType.MacDisInd:
                    return new MacDisIndPayload(data);
                case DnmType.MacDisReq:
                    return new EmptyDnmPayload(data);
                case DnmType.MacEncKeyReq:
                    return new MacEncKeyReqPayload(data);
                case DnmType.MacEncEksInd:
                    return new MacEncEksIndPayload(data);
                default:
                    return new UnknownDnmPayload(data);
            }
        }

        public abstract void Log(TextWriter writer);
    }
}