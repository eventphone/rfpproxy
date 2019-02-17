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
            writer.Write($"\tReserved0(0x{Reserved0:x2})");
            if (Length > 0)
            {
                writer.Write($" Length({Length,3}) Reserved2({Raw.ToHex()})");
            }
        }
    }
}