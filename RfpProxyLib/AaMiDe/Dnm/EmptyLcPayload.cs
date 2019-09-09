using System;

namespace RfpProxyLib.AaMiDe.Dnm
{
    public sealed class EmptyLcPayload : LcPayload
    {
        public EmptyLcPayload(ReadOnlyMemory<byte> data) : base(data)
        {
        }
    }
}