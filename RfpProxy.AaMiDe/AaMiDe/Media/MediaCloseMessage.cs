using System;
using System.Buffers.Binary;
using System.IO;

namespace RfpProxy.AaMiDe.Media
{
    public sealed class MediaCloseMessage : MediaMessage
    {
        public ushort Padding { get; }

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(2);

        public MediaCloseMessage(ReadOnlyMemory<byte> data):base(MsgType.MEDIA_CLOSE, data)
        {
            Padding = BinaryPrimitives.ReadUInt16BigEndian(base.Raw.Span);
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Padding({Padding:x4})");
        }
    }
}