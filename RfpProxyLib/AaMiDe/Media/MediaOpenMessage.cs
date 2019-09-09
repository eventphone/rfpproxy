using System;
using System.Buffers.Binary;
using System.IO;

namespace RfpProxyLib.AaMiDe.Media
{
    public sealed class MediaOpenMessage : MediaMessage
    {
        public byte Codec { get; }

        public byte SlotCount { get; }

        public uint Flags { get; }

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(6);

        public MediaOpenMessage(ReadOnlyMemory<byte> data) : base(MsgType.MEDIA_OPEN, data)
        {
            var span = base.Raw.Span;
            Codec = span[0];
            SlotCount = span[1];
            Flags = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(2));
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"codec({Codec}) slots({SlotCount}) flags({Flags})");
        }
    }
}