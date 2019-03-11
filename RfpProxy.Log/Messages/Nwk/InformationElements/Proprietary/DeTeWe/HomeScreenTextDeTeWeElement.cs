using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RfpProxy.Log.Messages.Nwk.InformationElements.Proprietary.DeTeWe
{
    public class HomeScreenTextDeTeWeElement : DeTeWeElement
    {
        public byte Reserved { get; }
        
        public List<string> Values { get; }

        public override bool HasUnknown => true;

        public HomeScreenTextDeTeWeElement(ReadOnlyMemory<byte> data) : base(DeTeWeType.HomeScreenText)
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
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"({String.Join('|', Values)}) Reserved({Reserved:x2})");
        }
    }
}