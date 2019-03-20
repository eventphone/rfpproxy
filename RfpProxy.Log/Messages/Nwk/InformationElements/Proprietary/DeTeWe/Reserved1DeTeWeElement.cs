using System;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages.Nwk.InformationElements.Proprietary.DeTeWe
{
    public class Reserved1DeTeWeElement : DeTeWeElement
    {
        public string Text1 { get; }
        
        public string Text2 { get; }

        public override ReadOnlyMemory<byte> Raw { get; }

        public override bool HasUnknown => true;

        public Reserved1DeTeWeElement(ReadOnlyMemory<byte> data):base(DeTeWeType.Reserved1, data)
        {
            Text1 = data.Span.CString();
            var eos = data.Span.IndexOf((byte) 0);
            data = data.Slice(eos + 1);
            Text2 = data.Span.CString();
            eos = data.Span.IndexOf((byte) 0);
            Raw = data.Slice(eos + 1);
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"({Text1}|{Text2})");
        }
    }
}