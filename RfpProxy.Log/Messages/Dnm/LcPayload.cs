using System;

namespace RfpProxy.Log.Messages.Dnm
{
    public abstract class LcPayload : DnmPayload
    {
        public byte Reserved0 { get; }

        public byte Length { get; }

        protected LcPayload(ReadOnlyMemory<byte> data) : base(data)
        {
            var span = data.Span;
            Reserved0 = span[0];
            if (data.Length <= 1)
            {
                return;
            }
            Length = span[1];
        }

        public override ReadOnlyMemory<byte> Raw
        {
            get
            {
                if (base.Raw.Length < 2)
                    return Array.Empty<byte>();
                return base.Raw.Slice(2);
            }
        }
    }
}