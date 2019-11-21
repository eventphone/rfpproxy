using System;
using System.Buffers.Binary;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.AaMiDe.Rfpc
{
    public sealed class RfpiRfpcValue : DnmRfpcValue
    {
        public enum AriClass : byte
        {
            A = 0b0000,
            B = 0b0001,
            C = 0b0010,
            D = 0b0011,
            E = 0b0100,
        }

        public abstract class Ari
        {
            public AriClass Class { get; }

            protected Ari(AriClass ariClass)
            {
                Class = ariClass;
            }

            public virtual void Log(TextWriter writer)
            {
                writer.Write($" Class({Class:G})");
            }

            public static Ari Create(ReadOnlyMemory<byte> data)
            {
                var ariClass = (AriClass) ((data.Span[0] & 0x70) >> 4);
                switch (ariClass)
                {
                    case AriClass.B:
                        return new AriB(data.Slice(0,4));
                    case AriClass.A:
                    case AriClass.C:
                    case AriClass.D:
                    case AriClass.E:
                        throw new NotImplementedException();
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public sealed class AriB:Ari
        {
            /// <summary>
            /// Equipment Installer's Code
            /// </summary>
            public ushort Eic { get; }

            /// <summary>
            /// Fixed Part Number
            /// </summary>
            public byte Fpn { get; }

            /// <summary>
            /// Fixed Part Sub-number
            /// </summary>
            public byte Fps { get; }

            public AriB(ReadOnlyMemory<byte> data):base(AriClass.B)
            {
                var span = data.Span;
                Eic = (ushort) ((BinaryPrimitives.ReadUInt16BigEndian(span.Slice(1)) >> 4) | ((span[0] & 0x0f) << 12));
                Fpn = (byte) (((span[2] & 0x0f) << 4) | ((span[3] & 0xf0) >> 4));
                Fps = (byte) (span[3] & 0x0f);
            }

            public override void Log(TextWriter writer)
            {
                base.Log(writer);
                writer.Write($" EIC({Eic,5}) FPN({Fpn,3}) FPS({Fps,2})");
            }
        }

        public bool HasSARIs { get; }

        public Ari Ard { get; }

        public byte Rpn { get; }

        public ReadOnlyMemory<byte> Pari { get; }

        public override ReadOnlyMemory<byte> Raw => base.Raw.Slice(5);

        public RfpiRfpcValue(ReadOnlyMemory<byte> data):base(RfpcKey.RFPI, data)
        {
            var span = data.Span;
            HasSARIs = span[0] >= 128;
            Ard = Ari.Create(data);
            if (Ard.Class == AriClass.A)
            {
                Rpn = (byte) (span[4] & 0x03);
                Pari = data;
            }
            else
            {
                Rpn = span[4];
                Pari = data.Slice(0, 4);
            }
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" HasSARI({HasSARIs})");
            Ard.Log(writer);
            writer.Write($" RPN({Rpn}) PARI({Pari.ToHex()})");
        }
    }
}