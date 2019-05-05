using System;
using System.IO;

namespace RfpProxy.Log.Messages.Media
{
    public sealed class MediaStopMessage : MediaMessage
    {
        public enum Direction : byte
        {
            None=0,
            Rx = 1,
            Tx=2,
            TxRx = 3
        }

        public Direction Dir { get; }

        public byte Padding { get; }

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(2);

        public MediaStopMessage(ReadOnlyMemory<byte> data):base(MsgType.MEDIA_STOP, data)
        {
            var span = base.Raw.Span;
            Dir = (Direction) span[0];
            Padding = span[1];
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" Direction({Dir}) Padding({Padding:x2})");
        }
    }
}