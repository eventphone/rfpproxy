using System;
using System.Buffers.Binary;
using System.IO;

namespace RfpProxy.AaMiDe.Sys
{
    public sealed class SysRandomValueMessage : AaMiDeMessage
    {
        public uint Random { get; }

        public override bool HasUnknown => false;

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(4);

        public SysRandomValueMessage(ReadOnlyMemory<byte> data) : base(MsgType.SYS_RANDOM_VALUE, data)
        {
            Random = BinaryPrimitives.ReadUInt32BigEndian(base.Raw.Span);
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Random({Random:X})");
        }
    }
}