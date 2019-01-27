using System;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages
{
    public sealed class UnknownMediaMessage : MediaMessage
    {
        public UnknownMediaMessage(MsgType type, ReadOnlyMemory<byte> data) : base(type, data)
        {
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write(HexEncoding.ByteToHex(Raw.Span));
        }
    }
}