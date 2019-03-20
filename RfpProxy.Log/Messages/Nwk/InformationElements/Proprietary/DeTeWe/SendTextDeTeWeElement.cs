using System;
using System.IO;
using System.Text;

namespace RfpProxy.Log.Messages.Nwk.InformationElements.Proprietary.DeTeWe
{
    public class SendTextDeTeWeElement : DeTeWeElement
    {
        public string Text { get; }

        public override bool HasUnknown => false;

        public override ReadOnlyMemory<byte> Raw=> ReadOnlyMemory<byte>.Empty;

        public SendTextDeTeWeElement(ReadOnlyMemory<byte> data):base(DeTeWeType.SendText, data)
        {
            Text = Encoding.UTF8.GetString(data.Span);
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"({Text})");
        }
    }
}