using System;
using System.Buffers.Binary;
using System.IO;
using System.Net;
using RfpProxyLib;

namespace RfpProxy.Log.Messages.Media
{
    public sealed class MediaConfMessage : MediaMessage
    {
        public byte MCEI { get; }

        public ushort PPN { get; }

        public ushort LocalPort1 { get; }

        public ushort LocalPort2 { get; }

        public IPAddress RxIpAddress { get; }

        public ushort RxPort1 { get; }

        public ushort RxPort2 { get; }

        public IPAddress TxIpAddress { get; }

        public ushort TxPort1 { get; }

        public ushort TxPort2 { get; }

        public override bool HasUnknown => true;

        protected override ReadOnlyMemory<byte> Raw => base.Raw.Slice(86);

        public ReadOnlyMemory<byte> Reserved1 { get; }

        public byte Reserved2 { get; }

        public ReadOnlyMemory<byte> Reserved3 { get; }

        public ReadOnlyMemory<byte> Reserved4 { get; }

        public MediaConfMessage(ReadOnlyMemory<byte> data):base(MsgType.MEDIA_CONF, data)
        {
            var span = base.Raw.Span;
            Reserved1 = base.Raw.Slice(0, 56);

            MCEI = span[56];
            Reserved2 = span[57];

            PPN = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(58));
            Reserved3 = base.Raw.Slice(60, 4);

            LocalPort1 = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(64));
            LocalPort2 = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(66));
            Reserved4 = base.Raw.Slice(68, 2);

            RxIpAddress = new IPAddress(span.Slice(70,4));
            RxPort1 = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(74));
            RxPort2 = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(76));
            TxIpAddress = new IPAddress(span.Slice(78,4));
            TxPort1 = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(82));
            TxPort2 = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(84));
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"Reserved1({Reserved1.ToHex()}) ");
            writer.Write($"MCEI({MCEI}) ");
            writer.Write($"Reserved2({Reserved2:x2}) ");
            writer.Write($"PPN({PPN:x4}) ");
            writer.Write($"Reserved3({Reserved3.ToHex()}) ");
            writer.Write($"Local({LocalPort1}/{LocalPort2}) ");
            writer.Write($"Reserved4({Reserved4.ToHex()}) ");
            writer.Write($"Rx({RxIpAddress}:{RxPort1}/{RxPort2}) ");
            writer.Write($"Tx({TxIpAddress}:{TxPort1}/{TxPort2}) ");
        }
    }
}