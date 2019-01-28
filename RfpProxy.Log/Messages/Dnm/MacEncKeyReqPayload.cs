using System;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages.Dnm
{
    public sealed class MacEncKeyReqPayload : DnmPayload
    {
        public ReadOnlyMemory<byte> Key { get; }

        public byte Id { get; }

        public MacEncKeyReqPayload(ReadOnlyMemory<byte> data) : base(data)
        {
            if (data.Length != 9)
                throw new ArgumentException("invalid length");
            Key = data.Slice(0, 8);
            Id = data.Span[8];
        }

        public override void Log(TextWriter writer)
        {
            writer.Write($"\tKey({HexEncoding.ByteToHex(Key.Span)}) Id({Id,3})");
        }
    }
}