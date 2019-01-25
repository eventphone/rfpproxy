using System;
using System.IO;

namespace RfpProxy.Log.Messages
{
    public sealed class DtmfMediaMessage : MediaMessage
    {
        public char Key { get; }

        public DtmfMediaMessage(ReadOnlyMemory<byte> data) : base(MsgType.MEDIA_DTMF, data)
        {
            var key = Raw.Span[2];
            Key = (char) key;
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Key: {Key}");
            PrintIfNotZero(writer, " extra1:", Raw.Slice(0,2).Span);
            PrintIfNotZero(writer, " extra2:", Raw.Slice(3).Span);
        }
    }
}