using System;
using System.Buffers.Binary;
using System.IO;

namespace RfpProxy.Log.Messages.Media
{
    public abstract class MediaMessage : AaMiDeMessage
    {
        public ushort Handle { get; }

        protected MediaMessage(MsgType type, ReadOnlyMemory<byte> data) : base(type, data)
        {
            Handle = BinaryPrimitives.ReadUInt16LittleEndian(base.Raw.Span);
        }

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(2);

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"HDL({Handle:X}) ");
        }
    }
}