using System;
using System.Buffers.Binary;
using System.IO;
using RfpProxyLib;

namespace RfpProxy.Log.Messages.Media
{
    public sealed class MediaToneMessage : MediaMessage
    {
        public MediaDirection Direction { get; }

        public byte Count { get; }

        public ushort Padding { get; }

        public ReadOnlyMemory<byte>[] Tones { get; }

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(4).Slice(Count*24);

        public override bool HasUnknown => true;

        public MediaToneMessage(ReadOnlyMemory<byte> data):base(MsgType.MEDIA_TONE2, data)
        {
            var span = base.Raw.Span;
            Direction = (MediaDirection)span[0];
            Count = span[1];
            Padding = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(2));
            var raw = base.Raw.Slice(4);
            Tones = new ReadOnlyMemory<byte>[Count];
            for (int i = 0; i < Count; i++)
            {
                Tones[i] = raw.Slice(0, 24);
                raw = raw.Slice(24);
            }
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" Direction({Direction})");
            writer.Write(Count == 0 ? " Off" : " On");
            writer.Write($" Padding({Padding:x4})");
            foreach (var tone in Tones)
            {
                writer.WriteLine();
                writer.Write($"\t Tone({tone.ToHex()})");
            }
        }
    }
}