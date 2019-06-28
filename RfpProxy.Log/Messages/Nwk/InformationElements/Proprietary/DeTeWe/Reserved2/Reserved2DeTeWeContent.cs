using System;
using System.IO;

namespace RfpProxy.Log.Messages.Nwk.InformationElements.Proprietary.DeTeWe.Reserved2
{
    public abstract class Reserved2DeTeWeContent
    {
        public Reserved2ContentDeTeWeType Type { get; }
        
        public virtual ReadOnlyMemory<byte> Raw { get; }

        public virtual bool HasUnknown => !Raw.IsEmpty;

        protected Reserved2DeTeWeContent(Reserved2ContentDeTeWeType type, ReadOnlyMemory<byte> data)
        {
            Type = type;
            Raw = data;
        }

        public static Reserved2DeTeWeContent Create(Reserved2ContentDeTeWeType type, ReadOnlyMemory<byte> data)
        {
            switch (type)
            {
                case Reserved2ContentDeTeWeType.Text:
                    return new Reserved2DeTeWeTextContent(data);
                default:
                    return new UnknownReserved2DeTeWeContent(type, data);
            }
        }

        public virtual void Log(TextWriter writer)
        {
            writer.Write("\t\t\t\t");
            if (Enum.IsDefined(typeof(Reserved2ContentDeTeWeType), Type))
                writer.Write(Type);
            else
                writer.Write(Type.ToString("x"));
        }
    }
}