using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RfpProxyLib.AaMiDe.Nwk.InformationElements.Proprietary.DeTeWe
{
    public class HomeScreenTextDeTeWeElement : DeTeWeElement
    {
        public byte Reserved { get; }
        
        public List<string> Values { get; }

        public override bool HasUnknown => Reserved != 0x90;

        public override ReadOnlyMemory<byte> Raw { get; }

        public HomeScreenTextDeTeWeElement(ReadOnlyMemory<byte> data) : base(DeTeWeType.HomeScreenText, data)
        {
            Reserved = data.Span[0];
            data = data.Slice(1);
            Values = new List<string>();
            while (data.Length > 0)
            {
                var length = data.Span[0];
                var value = data.Slice(1, length);
                Values.Add(Encoding.UTF8.GetString(value.Span));
                data = data.Slice(1).Slice(length);
            }
            Raw = data;
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"({String.Join('|', Values)})");
            if (HasUnknown)
                writer.Write($" Reserved({Reserved:x2})");
        }
    }
}