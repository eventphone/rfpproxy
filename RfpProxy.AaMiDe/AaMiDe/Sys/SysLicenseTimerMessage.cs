using System;
using System.Buffers.Binary;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.AaMiDe.Sys
{
    public sealed class SysLicenseTimerMessage : AaMiDeMessage
    {
        public TimeSpan GracePeriod { get; }

        /// <summary>
        /// md5sum of binary PARK
        /// </summary>
        public ReadOnlyMemory<byte> Md5 { get; }

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(20);

        public override ushort Length => (ushort) (base.Length + 20);

        public SysLicenseTimerMessage(ReadOnlyMemory<byte> data) : base(MsgType.SYS_LICENSE_TIMER, data)
        {
            var grace = BinaryPrimitives.ReadUInt32BigEndian(base.Raw.Span);
            GracePeriod = TimeSpan.FromMinutes(grace);
            Md5 = base.Raw.Slice(4,16);
        }

        public SysLicenseTimerMessage(TimeSpan gracePeriod, ReadOnlyMemory<byte> md5):base(MsgType.SYS_LICENSE_TIMER)
        {
            GracePeriod = gracePeriod;
            Md5 = md5;
        }

        public override Span<byte> Serialize(Span<byte> data)
        {
            data = base.Serialize(data);
            BinaryPrimitives.WriteUInt32BigEndian(data, (uint)GracePeriod.TotalMinutes);
            Md5.Span.CopyTo(data.Slice(4));
            return data.Slice(20);
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            if (GracePeriod.TotalMinutes > Int32.MaxValue)
                writer.Write($"Query ");
            else
                writer.Write($"Grace Period({GracePeriod}) ");
            writer.Write($"Md5({Md5.ToHex()})");
        }
    }
}