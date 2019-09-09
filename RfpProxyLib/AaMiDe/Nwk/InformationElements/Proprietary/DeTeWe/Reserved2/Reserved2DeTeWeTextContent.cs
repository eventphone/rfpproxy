using System;
using System.IO;
using System.Text;

namespace RfpProxyLib.AaMiDe.Nwk.InformationElements.Proprietary.DeTeWe.Reserved2
{
    public class Reserved2DeTeWeTextContent : Reserved2DeTeWeContent
    {
        public string Text { get; }

        public override ReadOnlyMemory<byte> Raw => ReadOnlyMemory<byte>.Empty;

        public Reserved2DeTeWeTextContent(ReadOnlyMemory<byte> data) : base(Reserved2ContentDeTeWeType.Text, data)
        {
            Text = Encoding.UTF8.GetString(data.Span);
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write('(');
            writer.Write(Text);
            writer.Write(')');
        }
    }
}