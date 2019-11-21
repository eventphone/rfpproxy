using System.IO;

namespace RfpProxy.AaMiDe.Nwk.InformationElements
{
    public sealed class NwkIeUnknowDouble : NwkDoubleByteInformationElement
    {
        public byte Reserved { get; }

        public override bool HasUnknown => true;

        public NwkIeUnknowDouble(NwkDoubleByteElementType type, byte data):base(type)
        {
            Reserved = data;
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" Reserved({Reserved:x2})");
        }
    }
}