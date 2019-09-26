using System;
using System.Buffers.Binary;
using System.IO;
using RfpProxyLib.AaMiDe.Dnm;

namespace RfpProxyLib.AaMiDe.Mac
{
    public sealed class MacEncKeyReqPayload : DnmPayload
    {
        public ulong Key { get; }

        public byte Id { get; }

        public override ReadOnlyMemory<byte> Raw => base.Raw.Slice(9);

        public MacEncKeyReqPayload(ReadOnlyMemory<byte> data) : base(data)
        {
            if (data.Length != 9)
                throw new ArgumentException("invalid length");
            Key = BinaryPrimitives.ReadUInt64BigEndian(data.Span);
            Id = data.Span[8];
        }

        public override void Log(TextWriter writer)
        {
            writer.Write($" Key({Key:x16}) Id({Id,3})");
        }
    }
}