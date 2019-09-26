using System;
using System.IO;
using RfpProxyLib.AaMiDe.Dnm;

namespace RfpProxyLib.AaMiDe.Mac
{
    public sealed class MacHoFailedIndPayload : DnmPayload
    {
        public enum HoFailedReason:byte
        {
            SetupFailed = 0x01,
        }

        public HoFailedReason Reason { get; set; }

        public override bool HasUnknown => base.HasUnknown || !Enum.IsDefined(typeof(HoFailedReason), Reason);

        public override ReadOnlyMemory<byte> Raw => base.Raw.Slice(1);

        public MacHoFailedIndPayload(ReadOnlyMemory<byte> data):base(data)
        {
            Reason = (HoFailedReason) data.Span[0];
        }

        public override void Log(TextWriter writer)
        {
            writer.Write($" Reason({Reason:G})");
            if (Raw.Length > 0)
                writer.Write($" Reserved({Raw.ToHex()})");
        }
    }
}