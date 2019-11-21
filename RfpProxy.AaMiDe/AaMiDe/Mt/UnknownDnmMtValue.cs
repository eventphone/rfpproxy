using System;

namespace RfpProxy.AaMiDe.Mt
{
    public sealed class UnknownDnmMtValue : DnmMtValue
    {
        public override bool HasUnknown => true;

        public UnknownDnmMtValue(MtKey type, ReadOnlyMemory<byte> data):base(type, data)
        {
        }
    }
}