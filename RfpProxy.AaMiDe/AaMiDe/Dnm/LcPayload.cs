using System;
using System.IO;

namespace RfpProxy.AaMiDe.Dnm
{
    public abstract class LcPayload : DnmPayload
    {
        public enum LogicalChannel:byte
        {
            Cs = 0,
            Cf = 0xf
        }

        public byte B { get; }//todo check with rfpproxy.inject

        public LogicalChannel Channel { get; set; }//todo check with rfpproxy.inject

        public abstract byte DataLength { get; }

        protected LcPayload(ReadOnlyMemory<byte> data) : base(data)
        {
            var span = data.Span;
            B = (byte) (span[0] >> 4);
            var channel = (span[0] & 0x0F);
            if (channel != 0 && channel != 0xf)
                throw new ArgumentOutOfRangeException("channel");
            Channel = (LogicalChannel) channel;
        }

        public override ReadOnlyMemory<byte> Raw
        {
            get
            {
                if (base.Raw.Length < 2)
                    return Array.Empty<byte>();
                return base.Raw.Slice(1);
            }
        }

        public override void Log(TextWriter writer)
        {
            writer.Write($" B({B:x1}) Channel({Channel}) Length({DataLength,3})");
        }
    }
}