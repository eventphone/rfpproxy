using System;

namespace RfpProxy.AaMiDe.Rfpc
{
    public sealed class UnknownDnmRfpcValue : DnmRfpcValue
    {
        public override bool HasUnknown => Type != (RfpcKey) 22;

        public UnknownDnmRfpcValue(RfpcKey type, ReadOnlyMemory<byte> data):base(type, data)
        {
        }
    }
}