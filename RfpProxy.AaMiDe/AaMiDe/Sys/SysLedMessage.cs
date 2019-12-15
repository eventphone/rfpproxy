using System;
using System.IO;

namespace RfpProxy.AaMiDe.Sys
{
    public sealed class SysLedMessage : AaMiDeMessage
    {
        public enum ColorScheme:byte
        {
            Off = 0x00,
            Green = 0x01,
            GreenFlash = 0x02,
            GreenOrangeFlash = 0x03,
            GreenRedFlash = 0x04,
            Red = 0x05,
            Orange = 0x06,
            GreenRed = 0x07
        }

        public byte Led { get; }

        public ColorScheme Color { get; }

        /// <summary>
        /// padding
        /// </summary>
        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(2);

        public override bool HasUnknown => false;

        public SysLedMessage(ReadOnlyMemory<byte> data) : base(MsgType.SYS_LED, data)
        {
            Led = base.Raw.Span[0];
            Color = (ColorScheme) base.Raw.Span[1];
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"LED({Led}) Color({Color})");
        }
    }
}