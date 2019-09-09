using System;
using System.IO;

namespace RfpProxyLib.AaMiDe.Media
{
    public sealed class MediaRedirectStopMessage : MediaMessage
    {
        public byte Fallback { get; }

        public byte Padding { get; }

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(2);

        public MediaRedirectStopMessage(ReadOnlyMemory<byte> data):base(MsgType.MEDIA_REDIRECT_STOP, data)
        {
            Fallback = base.Raw.Span[0];
            Padding = base.Raw.Span[1];
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            if (Fallback != 0)
            {
                writer.Write("fallback ");
            }
            writer.Write($"Padding({Padding})");
        }
    }
}