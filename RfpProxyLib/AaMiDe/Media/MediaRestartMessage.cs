using System;
using System.IO;

namespace RfpProxyLib.AaMiDe.Media
{
    public sealed class MediaRestartMessage : MediaMessage
    {
        public byte MCEI { get; }

        public byte Padding { get; }

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(2);

        public MediaRestartMessage(ReadOnlyMemory<byte> data):base(MsgType.MEDIA_RESTART, data)
        {
            MCEI = base.Raw.Span[0];
            Padding = base.Raw.Span[1];
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" MCEI({MCEI}) Padding({Padding:x2})");
        }
    }
}