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

        /// <summary>
        /// md5sum of binary PARK
        /// </summary>
        public ReadOnlyMemory<byte> Md5 { get; }

        public SysLicenseTimerMessage(ReadOnlyMemory<byte> data) : base(MsgType.SYS_LICENSE_TIMER, data)
        {
            Reserved1 = Raw.Slice(0, 2);
            var grace = BinaryPrimitives.ReadUInt16BigEndian(Raw.Slice(2).Span);
            GracePeriod = TimeSpan.FromMinutes(grace);
            Md5 = Raw.Slice(4);
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Grace Period({GracePeriod}) Reserved1({HexEncoding.ByteToHex(Reserved1.Span)}) Md5({HexEncoding.ByteToHex(Md5.Span)})");
        }
    }
}