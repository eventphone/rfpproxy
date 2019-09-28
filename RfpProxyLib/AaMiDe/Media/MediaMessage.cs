using System;
using System.Buffers.Binary;
using System.IO;

namespace RfpProxyLib.AaMiDe.Media
{
    public abstract class MediaMessage : AaMiDeMessage
    {
        public ushort Handle { get; }

        public override ushort Length => (ushort) (base.Length + 2);

        protected MediaMessage(ushort handle, MsgType type):base(type)
        {
            Handle = handle;
        }

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

        public override Span<byte> Serialize(Span<byte> data)
        {
            data = base.Serialize(data);
            BinaryPrimitives.WriteUInt16LittleEndian(data, Handle);
            return data.Slice(2);
        }
    }
}