using System;
using System.Buffers.Binary;
using System.IO;

namespace RfpProxy.AaMiDe
{
    public sealed class NackMessage : AaMiDeMessage
    {
        public MsgType Message { get; }

        public ushort CallId { get; }

        public NackReason Reason { get; }

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(8);

        public NackMessage(ReadOnlyMemory<byte> data):base(MsgType.NACK, data)
        {
            var span = base.Raw.Span;
            Message = (MsgType) BinaryPrimitives.ReadUInt16BigEndian(span);
            CallId = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(2));
            Reason = (NackReason) BinaryPrimitives.ReadUInt32BigEndian(span.Slice(4));
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Message({Message}) CallId({CallId:x4}) Reason({Reason:G})");
        }
    }

    public enum NackReason : uint
    {
        Ok = 0x4000000,
        InvalidElement = 0x4000001,
        NoResource = 0x4000002,
        WrongState = 0x4000003,
        InvalidParameters = 0x4000004,
        PortInUse = 0x4000005,
        CodecNotSupported = 0x4000006,
        VideoNotSupported = 0x4000007,
    }
}