using System;
using System.IO;

namespace RfpProxy.Log.Messages
{
    public sealed class SysMaxChannelsMessage : AaMiDeMessage
    {
        public byte Dsp { get; }

        public byte Sessions { get; }

        public override bool HasUnknown => false;

        public SysMaxChannelsMessage(ReadOnlyMemory<byte> data):base(MsgType.SYS_MAX_CHANNELS, data)
        {
            Dsp = Raw.Span[0];
            Sessions = Raw.Span[1];
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"DSP({Dsp}) Sessions({Sessions})");
        }
    }
}