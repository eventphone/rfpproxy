using System;
using System.Buffers.Binary;
using System.IO;

namespace RfpProxy.Log.Messages
{
    public sealed class HeartbeatMessage : AaMiDeMessage
    {
        public TimeSpan Uptime { get; }

        public override bool HasUnknown => false;

        public HeartbeatMessage(ReadOnlyMemory<byte> data):base(MsgType.HEARTBEAT, data)
        {
            var span = Raw.Span;
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