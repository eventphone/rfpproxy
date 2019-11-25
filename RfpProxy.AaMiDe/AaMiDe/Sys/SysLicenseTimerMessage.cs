﻿using System;
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

        public SysLicenseTimerMessage(ReadOnlyMemory<byte> data) : base(MsgType.SYS_LICENSE_TIMER, data)
        {
            var grace = BinaryPrimitives.ReadUInt32BigEndian(base.Raw.Span);
            GracePeriod = TimeSpan.FromMinutes(grace);
            Md5 = base.Raw.Slice(4,16);
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