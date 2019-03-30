using System;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages.Dnm
{
    public sealed class MacHoInProgressResPayload : DnmPayload
    {
        public byte Reserved1 { get; }

        public ReadOnlyMemory<byte> Key { get; }

        public byte Reserved2 { get; }

        public byte Id { get; }

        public override ReadOnlyMemory<byte> Raw => base.Raw.Slice(11);

        public override bool HasUnknown => true;

        public MacHoInProgressResPayload(ReadOnlyMemory<byte> data):base(data)
        {
            var span = data.Span;
            Reserved1 = span[0];
            Key = data.Slice(1, 8);
            Reserved2 = span[9];
            Id = data.Span[10];
        }

        public override void Log(TextWriter writer)
        {
            writer.Write($" Reserved1({Reserved1:x2}) Key({Key.ToHex()}) Reserved2({Reserved2:x2}) Id({Id})");
            if (!Raw.IsEmpty)
                writer.Write($" Reserved3({Raw.ToHex()})");
        }
    }
}