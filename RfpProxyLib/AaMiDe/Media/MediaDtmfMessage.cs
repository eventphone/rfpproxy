using System;
using System.Buffers.Binary;
using System.IO;

namespace RfpProxyLib.AaMiDe.Media
{
    public sealed class MediaDtmfMessage : MediaMessage
    {
        public ushort Duration { get; }

        public char Key { get; }

        public MediaDirection Direction { get; }

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(4);

        public MediaDtmfMessage(ReadOnlyMemory<byte> data) : base(MsgType.MEDIA_DTMF, data)
        {
            var span = base.Raw.Span;
            Duration = BinaryPrimitives.ReadUInt16LittleEndian(span);
            Key = (char) span[2];
            Direction = (MediaDirection) span[3];
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Duration({Duration}ms) Key({Key}) Direction({Direction})");
        }
    }
}