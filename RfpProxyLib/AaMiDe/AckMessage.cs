using System;
using System.Buffers.Binary;
using System.IO;

namespace RfpProxyLib.AaMiDe
{
    public sealed class AckMessage : AaMiDeMessage
    {
        public MsgType Message { get; }

        public ushort CallId { get; }

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(4);
        
        public AckMessage(ReadOnlyMemory<byte> data):base(MsgType.ACK, data)
        {
            var span = base.Raw.Span;
            Message = (MsgType) BinaryPrimitives.ReadUInt16BigEndian(span);
            CallId = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(2));
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Message({Message}) CallId({CallId:x4})");
        }
    }
}