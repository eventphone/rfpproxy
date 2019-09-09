using System;
using System.Globalization;
using System.IO;
using RfpProxyLib;

namespace RfpProxyLib.AaMiDe.Nwk.InformationElements.Proprietary.DeTeWe
{
    public class DateTimeDeTeWeElement : DeTeWeElement
    {
        public byte Reserved1 { get; }

        public DateTime DateTime { get; }

        public byte Reserved2 { get; }

        public override bool HasUnknown => Reserved1 != 0xc0 || Reserved2 != 0x00;

        public override ReadOnlyMemory<byte> Raw => base.Raw.Slice(8);

        public DateTimeDeTeWeElement(ReadOnlyMemory<byte> data):base(DeTeWeType.DateTime, data)
        {
            var span = data.Span;
            Reserved1 = data.Span[0];
            string date = span.Slice(1, 6).ToHex();
            DateTime = DateTime.ParseExact(date, "yyMMddHHmmss", CultureInfo.InvariantCulture);
            Reserved2 = span[7];
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"({DateTime})");
            if (HasUnknown)
                writer.Write($" Reserved1({Reserved1:x2}) Reserved2({Reserved2:x2})");
        }
    }
}