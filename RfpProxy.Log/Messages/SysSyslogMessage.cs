using System;
using System.Buffers.Binary;
using System.IO;
using System.Net;
using RfpProxyLib;

namespace RfpProxy.Log.Messages
{
    public sealed class SysSyslogMessage : AaMiDeMessage
    {
        public IPAddress Ip { get; }

        public ushort Port { get; }

        /// <summary>
        /// padding
        /// </summary>
        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(6);

        public override bool HasUnknown => false;

        public SysSyslogMessage(ReadOnlyMemory<byte> data):base(MsgType.SYS_SYSLOG, data)
        {
            var span = base.Raw.Span;
            Ip = new IPAddress(span.Slice(0,4));
            Port = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(4));
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Ip({Ip}) Port({Port})");
        }
    }
}