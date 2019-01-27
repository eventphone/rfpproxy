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
            writer.WriteLine();
            writer.Write($"\t{HexEncoding.ByteToHex(Raw.Span)}");
        }
    }
}