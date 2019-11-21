using System;
using System.Buffers.Binary;
using System.IO;
using System.Net;

namespace RfpProxy.AaMiDe.Media
{
    public sealed class MediaStatisticsMessage : MediaMessage
    {
        public ushort Padding { get; }

        public TimeSpan Duration { get; }

        public uint TransmittedPackets { get; }

        public uint TransmittedBytes { get; }

        public uint ReceivedPackets { get; }

        public uint ReceivedBytes { get; }

        public uint LostPackets { get; }

        public uint MaxJitter { get; }

        public IPAddress RtpIp { get; }

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(34);

        public MediaStatisticsMessage(ReadOnlyMemory<byte> data):base(MsgType.MEDIA_STATISTICS, data)
        {
            var span = base.Raw.Span;
            Padding = BinaryPrimitives.ReadUInt16LittleEndian(span);
            Duration = TimeSpan.FromSeconds(BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(2)));
            TransmittedPackets = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(6));
            TransmittedBytes = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(10));
            ReceivedPackets = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(14));
            ReceivedBytes = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(18));
            LostPackets = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(22));
            MaxJitter = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(26));
            RtpIp = new IPAddress(span.Slice(30, 4));
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Padding({Padding:x4}) ");
            writer.Write($"Duration({Duration}) ");
            writer.Write($"Tx({TransmittedPackets}p/{TransmittedBytes}b) ");
            writer.Write($"Rx({ReceivedPackets}p/{ReceivedBytes}b) ");
            writer.Write($"Lost({LostPackets}p) Jitter({(MaxJitter/1000d):F3}ms) ");
            writer.Write($"RtpIp({RtpIp}) ");
        }
    }
}