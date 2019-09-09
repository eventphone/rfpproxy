using System;
using System.Buffers.Binary;
using System.IO;

namespace RfpProxyLib.AaMiDe.Media
{
    public sealed class MediaDspCloseMessage : MediaMessage
    {
        public ushort Padding { get; }

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(2);

        public MediaDspCloseMessage(ReadOnlyMemory<byte> data):base(MsgType.MEDIA_DSP_CLOSE, data)
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