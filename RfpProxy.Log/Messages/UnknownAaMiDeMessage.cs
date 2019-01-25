using System;
using System.IO;

namespace RfpProxy.Log.Messages
{
    public sealed class UnknownAaMiDeMessage:AaMiDeMessage
    {
        public UnknownAaMiDeMessage(MsgType type, ReadOnlyMemory<byte> data) : base(type, data)
        {
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write(ByteToHex(Raw.Span));
        }
    }
}