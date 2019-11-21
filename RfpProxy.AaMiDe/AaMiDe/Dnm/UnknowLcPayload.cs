using System;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.AaMiDe.Dnm
{
    public sealed class UnknowLcPayload : LcPayload
    {
        public override bool HasUnknown => true;

        public UnknowLcPayload(ReadOnlyMemory<byte> data) : base(data)
        {
            DataLength = Raw.Span[0];
        }

        public override byte DataLength { get; }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            if (Raw.Length > 0)
            {
                writer.Write($" Reserved2({Raw.ToHex()})");
            }
        }
    }
}