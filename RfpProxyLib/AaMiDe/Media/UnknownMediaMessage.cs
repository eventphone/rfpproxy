using System;
using System.IO;

namespace RfpProxyLib.AaMiDe.Media
{
    public sealed class UnknownMediaMessage : MediaMessage
    {
        public override bool HasUnknown => true;

        protected override ReadOnlyMemory<byte> Raw => ReadOnlyMemory<byte>.Empty;

        public UnknownMediaMessage(MsgType type, ReadOnlyMemory<byte> data) : base(type, data)
        {
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Reserved({base.Raw.ToHex()})");
        }
    }
}