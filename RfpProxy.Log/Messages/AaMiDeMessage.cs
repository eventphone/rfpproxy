using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace RfpProxy.Log.Messages
{
    public abstract class AaMiDeMessage
    {
        public MsgType Type { get; }

        public ushort Length => BinaryPrimitives.ReadUInt16BigEndian(_data.Slice(2).Span);

        private readonly ReadOnlyMemory<byte> _data;

        protected virtual ReadOnlyMemory<byte> Raw => _data.Slice(4);

        protected AaMiDeMessage(MsgType type, ReadOnlyMemory<byte> data)
        {
            Type = type;
            _data = data;
        }

        public static AaMiDeMessage Create(ReadOnlyMemory<byte> data)
        {
            var type = (MsgType)BinaryPrimitives.ReadUInt16BigEndian(data.Slice(0, 2).Span);
            switch (type)
            {
                case MsgType.SYS_LED:
                    return new SysLedMessage(data);
                case MsgType.SYS_LICENSE_TIMER:
                    return new SysLicenseTimerMessage(data);
                default:
                    return new UnknownAaMiDeMessage(type, data);
            }
        }

        public virtual void Log(TextWriter writer)
        {
            writer.Write($"{Type,-22}({Length,4}) ");
        }

        public static string ByteToHex(ReadOnlySpan<byte> bytes)
        {
            StringBuilder s = new StringBuilder(bytes.Length*2);
            foreach (byte b in bytes)
                s.Append(b.ToString("x2"));
            return s.ToString();
        }
    }
}