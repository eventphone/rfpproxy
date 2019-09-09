using System;
using System.IO;

namespace RfpProxyLib.AaMiDe.Nwk.InformationElements
{
    public sealed class NwkIeDuration : NwkVariableLengthInformationElement
    {
        //ETSI EN 300 175-2 Annex B
        public static readonly TimeSpan DectMultiframeDuration = TimeSpan.FromMilliseconds(160);

        public enum LockLimitsType : byte
        {
            TemporaryUserLimit = 0b110,
            NoLimits = 0b111,
            TemporaryUserLimit2 = 0b101,
        }

        public enum TimeLimitsType : byte
        {
            Erase = 0b0000,
            Define1 = 0b001,
            Define2 = 0b0010,
            Standard = 0b0100,
            Infinite = 0b1111,
        }

        public LockLimitsType LockLimits { get; }

        public TimeLimitsType TimeLimits { get; }

        public TimeSpan Duration { get; }

        public override ReadOnlyMemory<byte> Raw => base.Raw.Slice(2);

        public NwkIeDuration(ReadOnlyMemory<byte> data):base(NwkVariableLengthElementType.Duration, data)
        {
            var span = data.Span;
            LockLimits = (LockLimitsType) ((span[0] >> 4) & 0x7);
            TimeLimits = (TimeLimitsType) (span[0] & 0xf);
            if (TimeLimits == TimeLimitsType.Define1)
            {
                Duration = 256 * span[1] * DectMultiframeDuration;
            }
            else if (TimeLimits == TimeLimitsType.Define2)
            {
                Duration = 65536 * span[1] * DectMultiframeDuration;
            }
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" LockLimits({LockLimits:G}) TimeLimits({TimeLimits:G})");
            if (TimeLimits == TimeLimitsType.Define1 | TimeLimits == TimeLimitsType.Define2)
            {
                writer.Write($" Duration({Duration})");
            }
        }
    }
}