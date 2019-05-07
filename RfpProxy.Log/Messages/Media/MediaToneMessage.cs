using System;
using System.Buffers.Binary;
using System.IO;

namespace RfpProxy.Log.Messages.Media
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

            public ushort Next { get; }

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
                writer.Write($"Tone1({Frequency1,5}Hz/{CB1/10d}dB) ");
                writer.Write($"Tone2({Frequency2,5}Hz/{CB2 / 10d}dB) ");
                writer.Write($"Tone3({Frequency3,5}Hz/{CB3 / 10d}dB) ");
                writer.Write($"Tone4({Frequency4,5}Hz/{CB4 / 10d}dB) ");
                writer.Write($"Duration({Duration}) Cycle({CycleCount} * to {CycleTo}) Next({Next})");
            }
        }

        public MediaDirection Direction { get; }

        public byte Count { get; }

        public uint Offset { get; }

        public Tone[] Tones { get; }

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(6).Slice(Count*24);

        public MediaToneMessage(ReadOnlyMemory<byte> data):base(MsgType.MEDIA_TONE2, data)
        {
            var span = base.Raw.Span;
            Direction = (MediaDirection)span[0];
            Count = span[1];
            Offset = BinaryPrimitives.ReadUInt32BigEndian(span.Slice(2));
            span = span.Slice(6);
            Tones = new Tone[Count];
            for (int i = 0; i < Count; i++)
            {
                Tones[i] = new Tone(span.Slice(0, 24));
                span = span.Slice(24);
            }
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" Direction({Direction})");
            writer.Write(Count == 0 ? " Off" : " On");
            writer.Write($" Offset({Offset})");
            for (var i = 0; i < Tones.Length; i++)
            {
                var tone = Tones[i];
                writer.WriteLine();
                writer.Write($"\t[{i}] ");
                tone.Log(writer);
            }
        }
    }
}