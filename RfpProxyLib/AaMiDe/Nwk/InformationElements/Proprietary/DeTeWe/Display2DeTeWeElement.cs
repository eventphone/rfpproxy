using System;
using System.IO;
using System.Text;

namespace RfpProxyLib.AaMiDe.Nwk.InformationElements.Proprietary.DeTeWe
{
    public class Display2DeTeWeElement : DeTeWeElement
    {
        public string Text { get; }

        public override bool HasUnknown => true;

        public override ReadOnlyMemory<byte> Raw { get; }

        public Display2DeTeWeElement(ReadOnlyMemory<byte> data):base(DeTeWeType.Display2, data)
        {
            if (data.Span[0] == 0x81)
            {
                Raw = ReadOnlyMemory<byte>.Empty;
                Text = Encoding.UTF8.GetString(data.Span.Slice(1));
            }
            else
            {
                Raw = data;
            }
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            if (Text != null)
                writer.Write($": Text({Text})");
        }
    }
}