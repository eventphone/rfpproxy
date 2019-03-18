using System;
using System.Buffers.Binary;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages
{
    public sealed class AckMessage : AaMiDeMessage
    {
        public MsgType Message { get; }

        public ushort CallId { get; }

        public ReadOnlyMemory<byte> Reserved { get; }

        public override bool HasUnknown => true;

        public AckMessage(ReadOnlyMemory<byte> data):base(MsgType.ACK, data)
        {
            Message = (MsgType) BinaryPrimitives.ReadUInt16BigEndian(Raw.Span);
            CallId = BinaryPrimitives.ReadUInt16LittleEndian(Raw.Span.Slice(2));
            Reserved = Raw.Slice(4);
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Message({Message}) CallId({CallId:x4}) Reserved({Reserved.ToHex()})");
        }
    }
}