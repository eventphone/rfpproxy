using System;
using System.Buffers.Binary;
using System.IO;
using System.Net;

namespace RfpProxy.Log.Messages
{
    public sealed class StatisticsMediaMessage : MediaMessage
    {
        public ushort Reserved { get; }

        public TimeSpan Duration { get; }

        public uint TransmittedPackets { get; }

        public uint TransmittedBytes { get; }

        public uint ReceivedPackets { get; }

        public uint ReceivedBytes { get; }

        public uint LostPackets { get; }

        public uint MaxJitter { get; }

        public IPAddress RtpIp { get; }

        public override bool HasUnknown => true;

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(34);

        public StatisticsMediaMessage(ReadOnlyMemory<byte> data):base(MsgType.MEDIA_STATISTICS, data)
        {
            var span = base.Raw.Span;
            Reserved = BinaryPrimitives.ReadUInt16LittleEndian(span);
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
            writer.Write($"Duration({Duration}) ");
            writer.Write($"Tx({TransmittedPackets}p/{TransmittedBytes}b) ");
            writer.Write($"Rx({ReceivedPackets}p/{ReceivedBytes}b) ");
            writer.Write($"Lost({LostPackets}p) Jitter({MaxJitter}) ");
            writer.Write($"RtpIp({RtpIp}) ");
            writer.Write($"Reserved({Reserved:x4})");
        }
    }
}