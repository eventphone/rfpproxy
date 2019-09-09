using System;
using System.IO;
using RfpProxyLib.AaMiDe.Nwk.InformationElements.Proprietary.Aastra;

namespace RfpProxyLib.AaMiDe.Nwk.InformationElements.Proprietary
{
    public class AastraProprietaryContent : NwkIeProprietaryContent
    {
        public enum AastraType : byte
        {
            Firmware = 0x01,
            Firmware2 = 0x02,
            Update = 0x05,
            UpdateAck = 0x07,
        }

        public AastraType Type { get; }

        public AastraElement Content { get; }

        public ReadOnlyMemory<byte> Raw { get; }

        public override bool HasUnknown => false;//Content.HasUnknown || !Raw.IsEmpty;

        public AastraProprietaryContent(ReadOnlyMemory<byte> data)
        {
            Type = (AastraType) data.Span[0];
            var length = data.Span[1];
            data = data.Slice(2);
            switch (Type)
            {
                case AastraType.Firmware:
                    Content = new FirmwareAastraElement(data.Slice(0,length));
                    break;
                case AastraType.Firmware2:
                    Content = new Firmware2AastraElement(data.Slice(0,length));
                    break;
                case AastraType.Update:
                    Content = new UpdateAastraElement(data.Slice(0,length));
                    break;
                case AastraType.UpdateAck:
                    Content = new UpdateAckAastraElement(data.Slice(0,length));
                    break;
                default:
                    Content =new UnknownAastraElement(data.Slice(0,length));
                    break;
            }
            Raw = data.Slice(length);
        }

        public override void Log(TextWriter writer)
        {
            writer.WriteLine();
            writer.Write("\t\t\t");
            if (Enum.IsDefined(typeof(AastraType), Type))
                writer.Write(Type.ToString("G"));
            else
                writer.Write(Type.ToString("x"));
            writer.Write(": ");
            Content.Log(writer);
            if (!Raw.IsEmpty)
                writer.Write($" Reserved({Raw.ToHex()})");
        }
    }
}