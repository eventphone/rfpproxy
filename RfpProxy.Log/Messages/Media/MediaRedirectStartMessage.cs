using System;
using System.Buffers.Binary;
using System.IO;
using System.Net;

namespace RfpProxy.Log.Messages.Media
{
    public sealed class MediaRedirectStartMessage : MediaMessage
    {
        public ushort Padding { get; }

        public ushort LocalPort1 { get; }

        public ushort LocalPort2 { get; }

        public IPAddress RemoteIpAddress { get; }

        public ushort RemotePort1 { get; }

        public ushort RemotePort2 { get; }

        public uint Time { get; }

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(18);

        public MediaRedirectStartMessage(ReadOnlyMemory<byte> data):base(MsgType.MEDIA_REDIRECT_START, data)
        {
            var span = base.Raw.Span;
            Padding = BinaryPrimitives.ReadUInt16BigEndian(span);
            LocalPort1 = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(2));
            LocalPort2 = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(4));
            RemoteIpAddress = new IPAddress(span.Slice(6, 4));
            RemotePort1 = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(10));
            RemotePort2 = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(12));
            Time = BinaryPrimitives.ReadUInt32LittleEndian(span.Slice(14));
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($" Local({LocalPort1}/{LocalPort2}) Remote({RemoteIpAddress}:{RemotePort1}/{RemotePort2}) Time({Time})");
        }
    }
}