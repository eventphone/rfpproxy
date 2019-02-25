using System;
using System.Buffers.Binary;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages
{
    public sealed class AckMessage : AaMiDeMessage
    {
        public MsgType Message { get; }

        public ReadOnlyMemory<byte> Reserved { get; }

        public override bool HasUnknown => true;

        public AckMessage(ReadOnlyMemory<byte> data):base(MsgType.ACK, data)
        {
            Message = (MsgType) BinaryPrimitives.ReadUInt16BigEndian(Raw.Slice(0, 2).Span);
            Reserved = Raw.Slice(2);
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Message({Message}) Reserved({Reserved.ToHex()})");
        }
    }
}