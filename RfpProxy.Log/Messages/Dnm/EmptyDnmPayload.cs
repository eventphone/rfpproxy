using System;
using System.IO;

namespace RfpProxy.Log.Messages.Dnm
{
    public sealed class EmptyDnmPayload : DnmPayload
    {
        public EmptyDnmPayload(ReadOnlyMemory<byte> data) : base(data)
        {
            if (Raw.Length > 0)
                throw new ArgumentException("empty payload expected");
        }

        public override void Log(TextWriter writer)
        {
        }
    }
}