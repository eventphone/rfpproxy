using System;
using System.Buffers.Binary;
using System.IO;

namespace RfpProxy.Log.Messages
{
    public sealed class SysRoundtripDelayMessage : AaMiDeMessage
    {
        private static readonly DateTimeOffset Epoch = new DateTimeOffset(1900, 01, 01, 0, 0, 0, TimeSpan.Zero);

        public DateTimeOffset Time1 { get; }

        public DateTimeOffset Time2 { get; }

        public SysRoundtripDelayMessage(ReadOnlyMemory<byte> data):base(MsgType.SYS_ROUNDTRIP_DELAY, data)
        {
            var span = Raw.Span;
            
            var seconds = BinaryPrimitives.ReadUInt32BigEndian(span);
            var nseconds = BinaryPrimitives.ReadUInt32BigEndian(span.Slice(4));
            Time1 = Epoch.AddSeconds(seconds).AddTicks(nseconds /(1000000 / TimeSpan.TicksPerMillisecond));

            seconds = BinaryPrimitives.ReadUInt32BigEndian(span.Slice(8));
            nseconds = BinaryPrimitives.ReadUInt32BigEndian(span.Slice(12));
            Time2 = Epoch.AddSeconds(seconds).AddTicks(nseconds /(1000000 / TimeSpan.TicksPerMillisecond));
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Time1({Time1}) Time2({Time2}) Delta: {Time2-Time1}");
        }
    }
}