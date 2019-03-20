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

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(20);

        public SysLicenseTimerMessage(ReadOnlyMemory<byte> data) : base(MsgType.SYS_LICENSE_TIMER, data)
        {
            Reserved = base.Raw.Slice(0, 2);
            var grace = BinaryPrimitives.ReadUInt16BigEndian(base.Raw.Slice(2).Span);
            GracePeriod = TimeSpan.FromMinutes(grace);
            Md5 = base.Raw.Slice(4,16);
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Padding({Reserved.ToHex()}) Grace Period({GracePeriod}) Md5({Md5.ToHex()})");
        }
    }
}