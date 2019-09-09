using System;
using System.Buffers.Binary;
using System.IO;

namespace RfpProxyLib.AaMiDe
{
    public sealed class HeartbeatMessage : AaMiDeMessage
    {
        public TimeSpan Uptime { get; }

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(8);

        public HeartbeatMessage(ReadOnlyMemory<byte> data):base(MsgType.HEARTBEAT, data)
        {
            var span = base.Raw.Span;
            var mseconds = BinaryPrimitives.ReadUInt32LittleEndian(span);
            var nseconds = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(4));
            Uptime = TimeSpan.FromMilliseconds(mseconds).Add(TimeSpan.FromTicks(nseconds/(1000000 / TimeSpan.TicksPerMillisecond)));
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Uptime({Uptime})");
        }
    }
}