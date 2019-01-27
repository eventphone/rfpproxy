using System;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages
{
    public sealed class UnknownDnmPayload : DnmPayload
    {

        public UnknownDnmPayload(ReadOnlyMemory<byte> data) : base(data)
        {
        }


        public override void Log(TextWriter writer)
        {
            writer.WriteLine();
            writer.Write($"\t{HexEncoding.ByteToHex(Raw.Span)}");
        }
    }

    public sealed class EmptyDnmPayload : DnmPayload
    {
        public EmptyDnmPayload(ReadOnlyMemory<byte> data) : base(data)
        {
            if (Raw.Length > 0)
                throw new ArgumentException("empty payload expected");
        }

        public override void Log(TextWriter writer)
        {
        }
    }
}