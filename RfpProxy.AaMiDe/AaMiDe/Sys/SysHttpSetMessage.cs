﻿using System;
using System.Buffers.Binary;
using System.IO;
using System.Net;

namespace RfpProxy.AaMiDe.Sys
{
    public sealed class SysHttpSetMessage : AaMiDeMessage
    {
        public IPAddress Ip { get; }

        public ushort Port { get; }

        /// <summary>
        /// padding
        /// </summary>
        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(18);

        public override bool HasUnknown => false;

        public SysHttpSetMessage(ReadOnlyMemory<byte> data):base(MsgType.SYS_HTTP_SET, data)
        {
            var span = base.Raw.Span;
            Ip = new IPAddress(span.Slice(0,16));
            Port = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(16));
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Ip({Ip}) Port({Port})");
        }
    }
}