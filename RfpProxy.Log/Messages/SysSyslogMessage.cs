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

        public ReadOnlyMemory<byte> Reserved { get; }

        public SysSyslogMessage(ReadOnlyMemory<byte> data):base(MsgType.SYS_SYSLOG, data)
        {
            var span = Raw.Span;
            Ip = new IPAddress(span.Slice(0,4));
            Port = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(4));
            Reserved = Raw.Slice(6);
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Ip({Ip}) Port({Port}) Reserved({Reserved.ToHex()})");
        }
    }
}