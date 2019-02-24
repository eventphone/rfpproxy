using System;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages.Dnm
{
    public sealed class UnknowLcPayload : LcPayload
    {
        public UnknowLcPayload(ReadOnlyMemory<byte> data) : base(data)
        {
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            if (Length > 0)
            {
                writer.Write($" Reserved2({Raw.ToHex()})");
            }
        }
    }
}