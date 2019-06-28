using System;
using System.Collections.Generic;
using System.IO;
using RfpProxy.Log.Messages.Nwk.InformationElements.Proprietary.DeTeWe.Reserved2;

namespace RfpProxy.Log.Messages.Nwk.InformationElements.Proprietary.DeTeWe
{
    public class Reserved2DeTeWeElement : DeTeWeElement
    {
        public byte Reserved2Type { get; }

        public List<Reserved2DeTeWeContent> Elements { get; } = new List<Reserved2DeTeWeContent>();

        public override ReadOnlyMemory<byte> Raw =>ReadOnlyMemory<byte>.Empty;

        public override bool HasUnknown { get; }

        public Reserved2DeTeWeElement(ReadOnlyMemory<byte> data):base(DeTeWeType.Reserved2, data)
        {
            Reserved2Type = data.Span[0];
            data = data.Slice(1);
            while (data.Length > 0)
            {
                var type = (Reserved2ContentDeTeWeType)data.Span[0];
                var length = data.Span[1];
                data = data.Slice(2);
                var content = Reserved2DeTeWeContent.Create(type, data.Slice(0,length));
                Elements.Add(content);
                data = data.Slice(length);
                HasUnknown |= content.HasUnknown;
            }
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.WriteLine($"({Reserved2Type:x2})");
            foreach (var element in Elements)
            {
                element.Log(writer);
            }
        }
    }
}