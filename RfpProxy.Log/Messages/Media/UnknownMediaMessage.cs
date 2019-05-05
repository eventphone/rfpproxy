using System;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages.Media
{
    public sealed class UnknownMediaMessage : MediaMessage
    {
        public override bool HasUnknown => true;

        public UnknownMediaMessage(MsgType type, ReadOnlyMemory<byte> data) : base(type, data)
        {
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Reserved({Raw.ToHex()})");
        }
    }
}