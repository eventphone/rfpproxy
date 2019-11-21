using System;
using System.IO;

namespace RfpProxy.AaMiDe.Sys
{
    public sealed class SysResetMessage : AaMiDeMessage
    {
        public ResetType Reset { get; }

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(1);

        public SysResetMessage(ReadOnlyMemory<byte> data):base(MsgType.SYS_RESET, data)
        {
            Reset = (ResetType) base.Raw.Span[0];
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            var value = Reset.ToString("G");
            if (!Enum.IsDefined(typeof(ResetType), Reset))
                value = Reset.ToString("X");
            writer.Write($"Type({value})");
        }

        public enum ResetType : byte
        {
            Reset = 0x02,
            CheckImageWithReboot = 0x05,
            CheckImageWithoutReboot = 0x06,
            EnableImageAutoCheck = 0x07,
            DisableImageAutoCheck = 0x08
        }
    }
}