﻿using System;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages.Media
{
    public sealed class MediaOpenMessage : MediaMessage
    {
        public byte Codec { get; }

        public byte SlotCount { get; }

        public byte Flags { get; }

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(3);

        public MediaOpenMessage(ReadOnlyMemory<byte> data) : base(MsgType.MEDIA_OPEN, data)
        {
            var span = base.Raw.Span;
            Codec = span[0];
            SlotCount = span[1];
            Flags = span[2];
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"codec({Codec}) slots({SlotCount}) flags({Flags})");
        }
    }
}