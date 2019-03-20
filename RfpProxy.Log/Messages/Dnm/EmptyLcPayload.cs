using System;

namespace RfpProxy.Log.Messages.Dnm
{
    public sealed class EmptyLcPayload : LcPayload
    {
        public EmptyLcPayload(ReadOnlyMemory<byte> data) : base(data)
        {
        }
    }
}