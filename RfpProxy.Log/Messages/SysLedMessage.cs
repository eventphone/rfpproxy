using System;
using System.IO;

namespace RfpProxy.Log.Messages
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

        public bool Led1 { get; }

        public bool Led2 { get; }

        public bool Led3 { get; }

        public bool Led4 { get; }

        public ColorScheme Color { get; }
        
        public SysLedMessage(ReadOnlyMemory<byte> data) : base(MsgType.SYS_LED, data)
        {
            var led = Raw.Span[0];
            var color = Raw.Span[1];
            Led1 = (led & 0x1) != 0;
            Led2 = (led & 0x2) != 0;
            Led3 = (led & 0x4) != 0;
            Led4 = (led & 0x8) != 0;
            Color = (ColorScheme) color;
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"LEDs: {(Led1?'1':'0')}{(Led2?'1':'0')}{(Led3?'1':'0')}{(Led4?'1':'0')} Color: {Color}");
        }
    }
}