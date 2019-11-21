using System;
using System.Buffers.Binary;
using System.IO;
using System.Net;

namespace RfpProxy.AaMiDe.Sys
{
    public sealed class SysRPingMessage : AaMiDeMessage
    {
        public IPAddress Ip { get; }

        public TimeSpan Rtt { get; }

        public override bool HasUnknown => false;

        /// <summary>
        /// padding
        /// </summary>
        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(8);

        public SysRPingMessage(ReadOnlyMemory<byte> data):base(MsgType.SYS_RPING, data)
        {
            var span = base.Raw.Span;
            Ip = new IPAddress(span.Slice(0, 4));
            Rtt = TimeSpan.FromMilliseconds(BinaryPrimitives.ReadUInt32BigEndian(span.Slice(4)));
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"IP({Ip}) Rtt({Rtt.TotalMilliseconds}ms)");
        }
    }
}