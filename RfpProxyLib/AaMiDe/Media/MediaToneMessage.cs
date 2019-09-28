using System;
using System.Buffers.Binary;
using System.IO;

namespace RfpProxyLib.AaMiDe.Media
{
    public sealed class MediaToneMessage : MediaMessage
    {
        public class Tone
        {
            //tenth dB = cB
            public ushort CB1 { get; }
            //tenth dB = cB
            public ushort CB2 { get; }
            //tenth dB = cB
            public ushort CB3 { get; }
            //tenth dB = cB
            public ushort CB4 { get; }

            public ushort Frequency1 { get; }

            public ushort Frequency2 { get; }

            public ushort Frequency3 { get; }

            public ushort Frequency4 { get; }

            public ushort Duration { get; }

            public ushort CycleCount { get; }

            public ushort CycleTo { get; }

            public ushort Next { get; set; }

            public Tone(ushort cb1, ushort cb2, ushort cb3, ushort cb4, ushort freq1, ushort freq2, ushort freq3, ushort freq4, ushort duration, ushort cyclecount, ushort cycleTo, ushort next)
            {
                CB1 = cb1;
                CB2 = cb2;
                CB3 = cb3;
                CB4 = cb4;
                Frequency1 = freq1;
                Frequency2 = freq2;
                Frequency3 = freq3;
                Frequency4 = freq4;
                Duration = duration;
                CycleCount = cyclecount;
                CycleTo = cycleTo;
                Next = next;
            }

            public Tone(ReadOnlySpan<byte> data)
            {
                if (data.Length != 24)
                    throw new ArgumentOutOfRangeException(nameof(data));
                Frequency1 = BinaryPrimitives.ReadUInt16LittleEndian(data);
                Frequency2 = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(2));
                Frequency3 = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(4));
                Frequency4 = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(6));
                CB1 = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(8));
                CB2 = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(10));
                CB3 = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(12));
                CB4 = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(14));
                Duration = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(16));
                CycleCount = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(18));
                CycleTo = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(20));
                Next = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(22));
            }

            public void Log(TextWriter writer)
            {
                writer.Write($"Tone1({Frequency1,5}Hz/{CB1 / 10d,3}dB) ");
                writer.Write($"Tone2({Frequency2,5}Hz/{CB2 / 10d,3}dB) ");
                writer.Write($"Tone3({Frequency3,5}Hz/{CB3 / 10d,3}dB) ");
                writer.Write($"Tone4({Frequency4,5}Hz/{CB4 / 10d,3}dB) ");
                writer.Write($"Duration({Duration,3}) Cycle({CycleCount} * to {CycleTo}) Next({Next})");
            }

            public Span<byte> Serialize(Span<byte> data)
            {
                 BinaryPrimitives.WriteUInt16LittleEndian(data, Frequency1);
                 BinaryPrimitives.WriteUInt16LittleEndian(data.Slice(2), Frequency2);
                 BinaryPrimitives.WriteUInt16LittleEndian(data.Slice(4), Frequency3);
                 BinaryPrimitives.WriteUInt16LittleEndian(data.Slice(6), Frequency4);
                 BinaryPrimitives.WriteUInt16LittleEndian(data.Slice(8), CB1);
                 BinaryPrimitives.WriteUInt16LittleEndian(data.Slice(10), CB2);
                 BinaryPrimitives.WriteUInt16LittleEndian(data.Slice(12), CB3);
                 BinaryPrimitives.WriteUInt16LittleEndian(data.Slice(14), CB4);
                 BinaryPrimitives.WriteUInt16LittleEndian(data.Slice(16), Duration);
                 BinaryPrimitives.WriteUInt16LittleEndian(data.Slice(18), CycleCount);
                 BinaryPrimitives.WriteUInt16LittleEndian(data.Slice(20), CycleTo);
                 BinaryPrimitives.WriteUInt16LittleEndian(data.Slice(22), Next);
                 return data.Slice(24);
            }
        }

        public MediaDirection Direction { get; }

        public uint Offset { get; }

        public Tone[] Tones { get; }

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(6).Slice(Tones.Length*24);

        public override ushort Length => (ushort) (base.Length + 6 + Tones.Length * 24);

        public MediaToneMessage(ushort handle, MediaDirection direction, ushort offset, Tone[] tones):base(handle, MsgType.MEDIA_TONE2)
        {
            if (tones.Length > 255)
                throw new ArgumentOutOfRangeException(nameof(tones), "max 255 tones allowed");
            Direction = direction;
            Offset = offset;
            Tones = tones;
        }

        public MediaToneMessage(ReadOnlyMemory<byte> data):base(MsgType.MEDIA_TONE2, data)
        {
            var span = base.Raw.Span;
            Direction = (MediaDirection)span[0];
            var count = span[1];
            Offset = BinaryPrimitives.ReadUInt32BigEndian(span.Slice(2));
            span = span.Slice(6);
            Tones = new Tone[count];
            for (int i = 0; i < Tones.Length; i++)
            {
                Tones[i] = new Tone(span.Slice(0, 24));
                span = span.Slice(24);
            }
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" Direction({Direction})");
            writer.Write(Tones.Length == 0 ? " Off" : " On");
            writer.Write($" Offset({Offset})");
            for (var i = 0; i < Tones.Length; i++)
            {
                var tone = Tones[i];
                writer.WriteLine();
                writer.Write($"\t[{i}] ");
                tone.Log(writer);
            }
        }

        public override Span<byte> Serialize(Span<byte> data)
        {
            data = base.Serialize(data);
            data[0] = (byte) Direction;
            data[1] = (byte) Tones.Length;
            BinaryPrimitives.WriteUInt32BigEndian(data.Slice(2), Offset);
            data = data.Slice(6);
            foreach (var tone in Tones)
            {
                data = tone.Serialize(data);
            }
            return data;
        }
    }
}