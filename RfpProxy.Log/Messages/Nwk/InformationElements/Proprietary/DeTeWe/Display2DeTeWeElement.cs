using System;
using System.IO;
using System.Text;
using RfpProxyLib;

namespace RfpProxy.Log.Messages.Nwk.InformationElements.Proprietary.DeTeWe
{
    public class Display2DeTeWeElement : DeTeWeElement
    {
        public ReadOnlyMemory<byte> Reserved { get; }

        public string Text { get; }

        public override bool HasUnknown => true;

        public Display2DeTeWeElement(ReadOnlyMemory<byte> data):base(DeTeWeType.Display2)
        {
            if (data.Span[0] == 0x81)
            {
                Reserved = data.Slice(0, 1);
                Text = Encoding.UTF8.GetString(data.Span.Slice(1));
            }
            else
            {
                Reserved = data;
            }
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"({Reserved.ToHex()})");
            if (Text != null)
                writer.Write($" Text({Text})");
        }
    }
}