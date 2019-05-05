using System;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages
{
    public sealed class MediaDtmfMessage : MediaMessage
    {
        public ReadOnlyMemory<byte> Reserved1 { get; }

        public char Key { get; }

        public ReadOnlyMemory<byte> Reserved2 { get; }

        public override bool HasUnknown => true;

        public MediaDtmfMessage(ReadOnlyMemory<byte> data) : base(MsgType.MEDIA_DTMF, data)
        {
            Reserved1 = Raw.Slice(0, 2);
            var key = Raw.Span[2];
            Key = (char) key;
            Reserved2 = Raw.Slice(3);
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Reserved1({Reserved1.ToHex()}) Key({Key}) Reserved2({Reserved2.ToHex()})");
        }
    }
}