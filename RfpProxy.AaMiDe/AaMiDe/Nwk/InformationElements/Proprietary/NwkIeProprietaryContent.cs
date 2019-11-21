using System;
using System.IO;

namespace RfpProxy.AaMiDe.Nwk.InformationElements.Proprietary
{
    public abstract class NwkIeProprietaryContent
    {
        public abstract bool HasUnknown { get; }

        public abstract void Log(TextWriter writer);

        public static NwkIeProprietaryContent Create(ushort emc, ReadOnlyMemory<byte> data)
        {
            switch (emc)
            {
                case 0x0031:
                    return new DeTeWeProprietaryContent(data);
                case 0x0002:
                    return new SiemensProprietaryContent(data);
                case 0x0094:
                    return new AastraProprietaryContent(data);
                default:
                    return new UnknownProprietaryContent(data);
            }
        }
    }
}