using System;
using System.IO;

namespace RfpProxy.Log.Messages
{
    public sealed class SysHeartbeatIntervalMessage : AaMiDeMessage
    {
        public TimeSpan Interval { get; }

        /// <summary>
        /// Padding
        /// </summary>
        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(1);

        public override bool HasUnknown => false;

        public SysHeartbeatIntervalMessage(ReadOnlyMemory<byte> data):base(MsgType.SYS_HEARTBEAT_INTERVAL, data)
        {
            Interval = TimeSpan.FromSeconds(base.Raw.Span[0]);
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Interval({Interval})");
        }
    }
}