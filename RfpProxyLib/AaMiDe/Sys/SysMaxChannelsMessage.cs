using System;
using System.IO;

namespace RfpProxyLib.AaMiDe.Sys
{
    public sealed class SysMaxChannelsMessage : AaMiDeMessage
    {
        public byte Dsp { get; }

        public byte Sessions { get; }

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(2);

        public SysMaxChannelsMessage(ReadOnlyMemory<byte> data):base(MsgType.SYS_MAX_CHANNELS, data)
        {
            var span = base.Raw.Span;
            Dsp = span[0];
            Sessions = span[1];
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"DSP({Dsp}) Sessions({Sessions})");
        }
    }
}