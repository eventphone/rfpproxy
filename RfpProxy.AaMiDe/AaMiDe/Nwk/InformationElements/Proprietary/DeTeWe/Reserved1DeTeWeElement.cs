using System;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.AaMiDe.Nwk.InformationElements.Proprietary.DeTeWe
{
    public class Reserved1DeTeWeElement : DeTeWeElement
    {
        public string Text1 { get; }
        
        public string Text2 { get; }

        public override ReadOnlyMemory<byte> Raw { get; }

        public override bool HasUnknown => !Raw.IsEmpty;

        public Reserved1DeTeWeElement(ReadOnlyMemory<byte> data):base(DeTeWeType.Reserved1, data)
        {
            if (data.Span[0] != 6)
            {
                Raw = data;
                return;
            }
            data = data.Slice(1);
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
            if (HasUnknown)
                writer.WriteLine($" Reserved({Raw.ToHex()})");
        }
    }
}