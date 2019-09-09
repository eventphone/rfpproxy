using System;
using System.IO;

namespace RfpProxyLib.AaMiDe.Dnm
{
    public sealed class UnknownDnmPayload : DnmPayload
    {
        public override bool HasUnknown => true;

        public UnknownDnmPayload(ReadOnlyMemory<byte> data) : base(data)
        {
        }


        public override void Log(TextWriter writer)
        {
            writer.Write($" Reserved({Raw.ToHex()})");
        }
    }
}