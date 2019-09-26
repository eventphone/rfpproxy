using System;

namespace RfpProxyLib.AaMiDe.Dnm
{
    public sealed class EmptyLcPayload : LcPayload
    {
        public override byte DataLength => 0;

        public EmptyLcPayload(ReadOnlyMemory<byte> data) : base(data)
        {
        }
    }
}