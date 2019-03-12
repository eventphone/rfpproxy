using System;
using System.IO;

namespace RfpProxy.Log.Messages.Nwk.InformationElements.Proprietary.DeTeWe
{
    public abstract class DeTeWeElement
    {
        public DeTeWeType Type { get; }
        
        public abstract bool HasUnknown { get; }

        protected DeTeWeElement(DeTeWeType type)
        {
            Type = type;
        }

        public virtual void Log(TextWriter writer)
        {
            writer.WriteLine();
            writer.Write("\t\t\t");
            if (Enum.IsDefined(typeof(DeTeWeType), Type))
                writer.Write(Type.ToString("G"));
            else
                writer.Write(Type.ToString("x"));
        }

        public static DeTeWeElement Create(DeTeWeType type, ReadOnlyMemory<byte> data)
        {
            switch (type)
            {
                case DeTeWeType.BtEthAddr:
                    return new BtEthAddrDeTeWeElement(data);
                case DeTeWeType.DateTime:
                    return new DateTimeDeTeWeElement(data);
                case DeTeWeType.Display:
                    return new DisplayDeTeWeElement(data);
                case DeTeWeType.HomeScreenText:
                    return new HomeScreenTextDeTeWeElement(data);
                case DeTeWeType.SendText:
                    return new SendTextDeTeWeElement(data);
                default:
                    return new UnknownDeTeWeElement(type, data);
            }
        }
    }
}