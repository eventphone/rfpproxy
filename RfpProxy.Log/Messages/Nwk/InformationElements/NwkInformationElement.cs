using System.IO;

namespace RfpProxy.Log.Messages.Nwk.InformationElements
{
    public abstract class NwkInformationElement
    {
        public abstract bool HasUnknown { get; }

        public abstract void Log(TextWriter writer);
    }
}