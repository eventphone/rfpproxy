using System;
using System.Buffers.Binary;
using System.IO;

namespace RfpProxy.AaMiDe
{
    public sealed class HeartbeatMessage : AaMiDeMessage
    {
        private const long NanosecondsPerTick = (1000000 / TimeSpan.TicksPerMillisecond);

        public TimeSpan Uptime { get; }

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(8);

        public HeartbeatMessage(ReadOnlyMemory<byte> data):base(MsgType.HEARTBEAT, data)
        {
            var span = base.Raw.Span;
            var mseconds = BinaryPrimitives.ReadUInt32LittleEndian(span);
            var nseconds = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(4));
            Uptime = TimeSpan.FromMilliseconds(mseconds).Add(TimeSpan.FromTicks(nseconds/NanosecondsPerTick));
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Uptime({Uptime})");
        }
    }
}