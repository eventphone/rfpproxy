using System;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages.Mt
{
    public abstract class DnmMtValue
    {
        public MtKey Type { get; }

        public virtual bool HasUnknown => !Raw.IsEmpty;

        public virtual ReadOnlyMemory<byte> Raw { get; }

        protected DnmMtValue(MtKey type, ReadOnlyMemory<byte> data)
        {
            Type = type;
            Raw = data;
        }

        public virtual void Log(TextWriter writer)
        {
            writer.WriteLine();
            writer.Write($"\t{Type,-23}:");
            if (!Raw.IsEmpty)
            {
                if (HasUnknown)
                    writer.Write(" Reserved");
                else
                    writer.Write(" Padding");
                writer.Write($"({Raw.ToHex()})");
            }
        }

        public static DnmMtValue Create(MtKey type, ReadOnlyMemory<byte> data)
        {
            switch (type)
            {
                default:
                    return new UnknownDnmMtValue(type, data);
            }
        }
    }
}