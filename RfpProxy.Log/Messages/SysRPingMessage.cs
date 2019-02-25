using System;
using System.Buffers.Binary;
using System.IO;
using System.Net;
using RfpProxyLib;

namespace RfpProxy.Log.Messages
{
    public sealed class SysRPingMessage : AaMiDeMessage
    {
        public IPAddress Ip { get; }

        public TimeSpan Rtt { get; }

        public ReadOnlyMemory<byte> Reserved { get;}

        public override bool HasUnknown => true;

        public SysRPingMessage(ReadOnlyMemory<byte> data):base(MsgType.SYS_RPING, data)
        {
            var span = Raw.Span;
            Ip = new IPAddress(span.Slice(0, 4));
            Rtt = TimeSpan.FromMilliseconds(BinaryPrimitives.ReadUInt32BigEndian(span.Slice(4)));
            Reserved = Raw.Slice(8);
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"IP({Ip}) Rtt({Rtt.TotalMilliseconds}ms) Reserved({Reserved.ToHex()})");
        }
    }
}