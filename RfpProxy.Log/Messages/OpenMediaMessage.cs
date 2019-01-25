using System;
using System.IO;

namespace RfpProxy.Log.Messages
{
    public sealed class OpenMediaMessage : MediaMessage
    {
        public byte Codec { get; }

        public byte SlotCount { get; }

        public byte Flags { get; }

        public OpenMediaMessage(ReadOnlyMemory<byte> data) : base(MsgType.MEDIA_OPEN, data)
        {
            var span = Raw.Span;
            Codec = span[0];
            SlotCount = span[1];
            Flags = span[2];
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"codec:{Codec} slots:{SlotCount} flags:{Flags}");
            if (Raw.Length > 3)
            {
                PrintIfNotZero(writer, " extra:", Raw.Slice(3).Span);
            }
        }
    }
}