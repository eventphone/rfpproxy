using System;
using System.Buffers.Binary;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages
{
    public sealed class SysLicenseTimerMessage : AaMiDeMessage
    {
        /// <summary>
        /// Padding
        /// </summary>
        public ReadOnlyMemory<byte> Reserved { get; }

        public TimeSpan GracePeriod { get; }

        /// <summary>
        /// md5sum of binary PARK
        /// </summary>
        public ReadOnlyMemory<byte> Md5 { get; }

        public override bool HasUnknown => false;

        public SysLicenseTimerMessage(ReadOnlyMemory<byte> data) : base(MsgType.SYS_LICENSE_TIMER, data)
        {
            Reserved = Raw.Slice(0, 2);
            var grace = BinaryPrimitives.ReadUInt16BigEndian(Raw.Slice(2).Span);
            GracePeriod = TimeSpan.FromMinutes(grace);
            Md5 = Raw.Slice(4);
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Grace Period({GracePeriod}) Reserved({Reserved.ToHex()}) Md5({Md5.ToHex()})");
        }
    }
}