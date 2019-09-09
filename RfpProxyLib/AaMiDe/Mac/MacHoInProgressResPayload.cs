using System;
using System.IO;
using RfpProxyLib.AaMiDe.Dnm;

namespace RfpProxyLib.AaMiDe.Mac
{
    public sealed class MacHoInProgressResPayload : DnmPayload
    {
        public byte Padding1 { get; }

        public ReadOnlyMemory<byte> Key { get; }

        public byte Padding2 { get; }

        public byte Id { get; }

        public override ReadOnlyMemory<byte> Raw => base.Raw.Slice(11);

        public MacHoInProgressResPayload(ReadOnlyMemory<byte> data):base(data)
        {
            var span = data.Span;
            Padding1 = span[0];
            Key = data.Slice(1, 8);
            Padding2 = span[9];
            Id = data.Span[10];
        }

        public override void Log(TextWriter writer)
        {
            writer.Write($" Padding1({Padding1:x2}) Key({Key.ToHex()}) Padding2({Padding2:x2}) Id({Id})");
            if (!Raw.IsEmpty)
                writer.Write($" Reserved({Raw.ToHex()})");
        }
    }
}