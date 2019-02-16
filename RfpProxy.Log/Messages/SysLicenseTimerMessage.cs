using System;
using System.Buffers.Binary;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages
{
    public sealed class SysLicenseTimerMessage : AaMiDeMessage
    {
        public ReadOnlyMemory<byte> Reserved1 { get; }

        public TimeSpan GracePeriod { get; }

        public ReadOnlyMemory<byte> Reserved2 { get; }

        public SysLicenseTimerMessage(ReadOnlyMemory<byte> data) : base(MsgType.SYS_LICENSE_TIMER, data)
        {
            Reserved1 = Raw.Slice(0, 2);
            var grace = BinaryPrimitives.ReadUInt16BigEndian(Raw.Slice(2).Span);
            GracePeriod = TimeSpan.FromMinutes(grace);
            Reserved2 = Raw.Slice(4);
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Grace Period({GracePeriod}) Reserved1({HexEncoding.ByteToHex(Reserved1.Span)}) Reserved2({HexEncoding.ByteToHex(Reserved2.Span)})");
        }
    }
}