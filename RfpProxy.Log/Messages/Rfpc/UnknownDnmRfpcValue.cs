using System;

namespace RfpProxy.Log.Messages.Rfpc
{
    public sealed class UnknownDnmRfpcValue : DnmRfpcValue
    {
        public override bool HasUnknown => true;

        public UnknownDnmRfpcValue(RfpcKey type, ReadOnlyMemory<byte> data):base(type, data)
        {
        }
    }
}