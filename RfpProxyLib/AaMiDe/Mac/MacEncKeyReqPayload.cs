using System;
using System.IO;
using RfpProxyLib.AaMiDe.Dnm;

namespace RfpProxyLib.AaMiDe.Mac
{
    public sealed class MacEncKeyReqPayload : DnmPayload
    {
        public ReadOnlyMemory<byte> Key { get; }

        public byte Id { get; }

        public override ReadOnlyMemory<byte> Raw => base.Raw.Slice(9);

        public MacEncKeyReqPayload(ReadOnlyMemory<byte> data) : base(data)
        {
            if (data.Length != 9)
                throw new ArgumentException("invalid length");
            Key = data.Slice(0, 8);
            Id = data.Span[8];
        }

        public override void Log(TextWriter writer)
        {
            writer.Write($" Key({Key.ToHex()}) Id({Id,3})");
        }
    }
}