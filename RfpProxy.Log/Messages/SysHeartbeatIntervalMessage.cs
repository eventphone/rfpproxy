using System;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages
{
    public sealed class SysHeartbeatIntervalMessage : AaMiDeMessage
    {
        public TimeSpan Interval { get; }

        /// <summary>
        /// Padding
        /// </summary>
        public ReadOnlyMemory<byte> Reserved { get; }

        public override bool HasUnknown => false;

        public SysHeartbeatIntervalMessage(ReadOnlyMemory<byte> data):base(MsgType.SYS_HEARTBEAT_INTERVAL, data)
        {
            Interval = TimeSpan.FromSeconds(Raw.Span[0]);
            Reserved = Raw.Slice(1);
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Interval({Interval}) Reserved({Reserved.ToHex()})");
        }
    }
}