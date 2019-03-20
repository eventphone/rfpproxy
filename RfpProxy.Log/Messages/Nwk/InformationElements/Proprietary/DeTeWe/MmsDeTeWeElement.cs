using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;
using RfpProxyLib;

namespace RfpProxy.Log.Messages.Nwk.InformationElements.Proprietary.DeTeWe
{
    public class MmsDeTeWeElement : DeTeWeElement
    {
        public ushort SubType { get; }

        public ReadOnlyMemory<byte> Reserved1 { get; }

        public ReadOnlyMemory<byte> Reserved2 { get; }

        public string SenderUri { get; }

        public string SenderName { get; }

        public ReadOnlyMemory<byte> Reserved3 { get; }

        public string Message { get; }

        public ReadOnlyMemory<byte> Reserved4 { get; }

        public override bool HasUnknown => true;

        public MmsDeTeWeElement(ReadOnlyMemory<byte> data):base(DeTeWeType.Mms, data)
        {
            SubType = BinaryPrimitives.ReadUInt16BigEndian(data.Span);
            data = data.Slice(2);

            var length = data.Span[0];
            if (length == 0) return;

            Reserved1 = data.Slice(1, length);
            data = data.Slice(1).Slice(length);

            length = data.Span[0];
            Reserved2 = data.Slice(1, length);
            data = data.Slice(1).Slice(length);

            length = data.Span[0];
            SenderUri = Encoding.UTF8.GetString(data.Span.Slice(1, length));
            data = data.Slice(1).Slice(length);

            length = data.Span[0];
            SenderName = Encoding.UTF8.GetString(data.Span.Slice(1, length));
            data = data.Slice(1).Slice(length);

            Reserved3 = data.Slice(0, 4);
            data = data.Slice(4);

            length = data.Span[0];
            Message = Encoding.UTF8.GetString(data.Span.Slice(1, length));
            data = data.Slice(1).Slice(length);

            Reserved4 = data;
        }

        public override void Log(TextWriter writer)
        {
            base.Log(writer);
            writer.Write($"({SubType:x4})");
            if (!Reserved1.IsEmpty)
                writer.Write($" Reserved1({Reserved1.ToHex()})");
            if (SubType != 0x8030)
                return;
            
            if (!Reserved2.IsEmpty)
                writer.Write($" Reserved2({Reserved2.ToHex()})");

            writer.Write($" Reserved3({Reserved3.ToHex()})");

            writer.Write($" SenderUri({SenderUri}) SenderName({SenderName}) Message({Message})");

            if (!Reserved4.IsEmpty)
                writer.Write($" Reserved4({Reserved4.ToHex()})");
        }
    }
}