using System;
using System.IO;

namespace RfpProxyLib.AaMiDe.Nwk.InformationElements
{
    public abstract class NwkDoubleByteInformationElement : NwkInformationElement
    {
        public NwkDoubleByteElementType Type { get; }

        protected NwkDoubleByteInformationElement(NwkDoubleByteElementType identifier)
        {
            Type = identifier;
        }

        public static NwkInformationElement Create(byte identifier, byte content)
        {
            var type = (NwkDoubleByteElementType) identifier;
            switch (type)
            {
                case NwkDoubleByteElementType.BasicService:
                    return new NwkIeBasicService(content);
                case NwkDoubleByteElementType.ReleaseReason:
                    return new NwkIeReleaseReason(content);
                case NwkDoubleByteElementType.Signal:
                    return new NwkIeSignal(content);
                case NwkDoubleByteElementType.TimerRestart:
                case NwkDoubleByteElementType.TestHookControl:
                case NwkDoubleByteElementType.SingleDisplay:
                case NwkDoubleByteElementType.SingleKeypad:
                case NwkDoubleByteElementType.Reserved:
                    return new NwkIeUnknowDouble(type, content);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void Log(TextWriter writer)
        {
            writer.Write($"\t\t{Type,-20}:");
        }
    }
}