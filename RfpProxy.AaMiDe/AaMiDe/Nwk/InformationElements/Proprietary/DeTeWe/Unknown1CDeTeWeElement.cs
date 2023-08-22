using System;
using System.IO;
using System.Text;

namespace RfpProxy.AaMiDe.Nwk.InformationElements.Proprietary.DeTeWe
{
    public class Unknown1CDeTeWeElement : DeTeWeElement
    {
        public string Content { get; set; }

        public override bool HasUnknown => false;

        public override ReadOnlyMemory<byte> Raw => Memory<byte>.Empty;

        public Unknown1CDeTeWeElement(ReadOnlyMemory<byte> data) : base(DeTeWeType.Unknown1C, data)
        {
            Content = Encoding.UTF8.GetString(data.Span);
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"({Content})");
        }
    }
}