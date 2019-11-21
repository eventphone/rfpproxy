using System;
using System.IO;

namespace RfpProxy.AaMiDe.Media
{
    public enum MediaDirection : byte
    {
        None = 0,
        Rx = 1,
        Tx = 2,
        TxRx = 3
    }

    public sealed class MediaStopMessage : MediaMessage
    {
        public MediaDirection Direction { get; }

        public byte Padding { get; }

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(2);

        public MediaStopMessage(ReadOnlyMemory<byte> data):base(MsgType.MEDIA_STOP, data)
        {
            var span = base.Raw.Span;
            Direction = (MediaDirection) span[0];
            Padding = span[1];
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Direction({Direction}) Padding({Padding:x2})");
        }
    }
}