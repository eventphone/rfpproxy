using System;
using System.Buffers.Binary;
using System.IO;

namespace RfpProxy.Log.Messages
{
    public sealed class SysLicenseTimerMessage : AaMiDeMessage
    {
        public TimeSpan GracePeriod { get; }

        public SysLicenseTimerMessage(ReadOnlyMemory<byte> data) : base(MsgType.SYS_LICENSE_TIMER, data)
        {
            var grace = BinaryPrimitives.ReadUInt16BigEndian(Raw.Slice(2).Span);
            GracePeriod = TimeSpan.FromMinutes(grace);
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Grace Period: {GracePeriod}");
        }
    }
}