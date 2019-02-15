using System;
using System.Buffers.Binary;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages
{
    public sealed class SysLicenseTimerMessage : AaMiDeMessage
    {
        public TimeSpan GracePeriod { get; }

        public SysLicenseTimerMessage(ReadOnlyMemory<byte> data) : base(MsgType.SYS_LICENSE_TIMER, data)
        {
            var grace = BinaryPrimitives.ReadUInt16BigEndian(base.Raw.Slice(2).Span);
            GracePeriod = TimeSpan.FromMinutes(grace);
        }

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(2);

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Grace Period: {GracePeriod} Reserved: {HexEncoding.ByteToHex(Raw.Span)}");
        }
    }
}