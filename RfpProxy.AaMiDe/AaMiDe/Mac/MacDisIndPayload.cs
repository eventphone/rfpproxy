using RfpProxy.AaMiDe.Dnm;
using System;
using System.IO;

namespace RfpProxy.AaMiDe.Mac
{
    public sealed class MacDisIndPayload : DnmPayload
    {
        public enum MacDisIndReason : byte
        {
            Unspecified = 0,
            Normal = 1,
            Abnormal = 2
        }

        public MacDisIndReason Reason { get; }

        public override ReadOnlyMemory<byte> Raw => base.Raw.IsEmpty ? base.Raw : base.Raw.Slice(1);

        public MacDisIndPayload(ReadOnlyMemory<byte> data) : base(data)
        {
            if (data.Length > 1)
                throw new ArgumentException("only one byte allowed");
            if (data.Length == 1)
                Reason = (MacDisIndReason) data.Span[0];
        }

        public override void Log(TextWriter writer)
        {
            writer.Write($" Reason({Reason,-11:G})");
        }
    }
}