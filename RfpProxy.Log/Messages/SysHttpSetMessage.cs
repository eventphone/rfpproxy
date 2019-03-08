using System;
using System.Buffers.Binary;
using System.IO;
using System.Net;
using RfpProxyLib;

namespace RfpProxy.Log.Messages
{
    public sealed class SysHttpSetMessage : AaMiDeMessage
    {
        public IPAddress Ip { get; }

        public ushort Port { get; }

        public ReadOnlyMemory<byte> Padding { get; }

        public override bool HasUnknown => false;

        public SysHttpSetMessage(ReadOnlyMemory<byte> data):base(MsgType.SYS_HTTP_SET, data)
        {
            var span = Raw.Span;
            Ip = new IPAddress(span.Slice(0,4));
            Port = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(4));
            Padding = Raw.Slice(6);
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Ip({Ip}) Port({Port})");
        }
    }
}