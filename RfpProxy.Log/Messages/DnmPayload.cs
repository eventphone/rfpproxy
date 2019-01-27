using System;
using System.IO;

namespace RfpProxy.Log.Messages
{
    public abstract class DnmPayload
    {
        public ReadOnlyMemory<byte> Raw { get; }

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
                default:
                    return new UnknownDnmPayload(data);
            }
        }

        private static DnmPayload CreateMac(DnmType type, ReadOnlyMemory<byte> data)
        {
            switch (type)
            {
                case DnmType.MacDisReq:
                    return new EmptyDnmPayload(data);
                default:
                    return new UnknownDnmPayload(data);
            }
        }

        public abstract void Log(TextWriter writer);
    }
}