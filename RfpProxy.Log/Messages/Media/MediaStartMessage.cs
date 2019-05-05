using System;
using System.Buffers.Binary;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages.Media
{
    public sealed class MediaStartMessage : MediaMessage
    {
        public MediaDirection Direction { get; }

        public byte Padding1 { get; }

        public uint Time { get; }

        public byte MetKeepAlive { get; }

        public ReadOnlyMemory<byte> Padding2 { get; }

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(10);

        public MediaStartMessage(ReadOnlyMemory<byte> data) : base(MsgType.MEDIA_START, data)
        {
            var span = base.Raw.Span;
            Direction = (MediaDirection)span[0];
            Padding1 = span[1];
            Time = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(2));
            MetKeepAlive = span[6];
            Padding2 = base.Raw.Slice(7, 3);
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" Direction({Direction}) Padding1({Padding1:x2}) Time({Time}) MetKeepAlive({MetKeepAlive}) Padding2({Padding2.ToHex()})");
        }
    }
}