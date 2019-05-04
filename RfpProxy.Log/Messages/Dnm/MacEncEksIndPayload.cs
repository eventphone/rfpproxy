using System;
using System.Buffers.Binary;
using System.IO;

namespace RfpProxy.Log.Messages.Dnm
{
    public sealed class MacEncEksIndPayload : DnmPayload
    {
        public enum MacEncEksIndFlag : byte
        {
            Encrypted = 1,
            EncrytpedWithId = 2,
        }

        public MacEncEksIndFlag Flag { get; }

        public byte Id { get; }

        public ushort Ppn { get; }

        public override ReadOnlyMemory<byte> Raw { get; }

        public MacEncEksIndPayload(ReadOnlyMemory<byte> data):base(data)
        {
            Flag = (MacEncEksIndFlag) data.Span[0];
            Raw = base.Raw.Slice(1);
            if (Flag == MacEncEksIndFlag.EncrytpedWithId)
            {
                Id = Raw.Span[0];
                Ppn = BinaryPrimitives.ReadUInt16BigEndian(Raw.Span.Slice(1));
                Raw = Raw.Slice(3);
            }
        }

        public override void Log(TextWriter writer)
        {
            writer.Write($" Flag({Flag:G})");
            if (Flag == MacEncEksIndFlag.EncrytpedWithId)
                writer.Write($" Id({Id}) Ppn({Ppn})");
        }
    }
}