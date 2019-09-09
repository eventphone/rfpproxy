using System.IO;

namespace RfpProxyLib.AaMiDe.Nwk.InformationElements
{
    public abstract class NwkInformationElement
    {
        public abstract bool HasUnknown { get; }

        public abstract void Log(TextWriter writer);
    }
}