using System;
using System.IO;

namespace RfpProxyLib.AaMiDe.Dnm
{
    public abstract class LcPayload : DnmPayload
    {
        public byte B { get; }

        public string Channel { get; set; }

        public byte Length { get; }

        protected LcPayload(ReadOnlyMemory<byte> data) : base(data)
        {
            var span = data.Span;
            B = (byte) (span[0] >> 4);
            Channel = (span[0] & 0x0F) == 0 ? "Cs" : "Cf";
            if (data.Length <= 1)
            {
                return;
            }
            Length = span[1];
        }

        public override ReadOnlyMemory<byte> Raw
        {
            get
            {
                if (base.Raw.Length < 2)
                    return Array.Empty<byte>();
                return base.Raw.Slice(2);
            }
        }

        public override void Log(TextWriter writer)
        {
            writer.Write($" B({B:x1}) Channel({Channel})");
            if (Length > 0)
            {
                writer.Write($" Length({Length,3})");
            }
        }
    }
}