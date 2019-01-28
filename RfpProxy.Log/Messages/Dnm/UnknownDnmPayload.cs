using System;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages.Dnm
{
    public sealed class UnknownDnmPayload : DnmPayload
    {

        public UnknownDnmPayload(ReadOnlyMemory<byte> data) : base(data)
        {
        }


        public override void Log(TextWriter writer)
        {
            writer.Write($"\tUNK: {HexEncoding.ByteToHex(Raw.Span)}");
        }
    }
}