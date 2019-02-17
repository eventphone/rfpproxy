using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace RfpProxyLib
{
    public static class HexEncoding
    {
        public static string ToHex(this ReadOnlyMemory<byte> bytes)
        {
            return ByteToHex(bytes.Span);
        }

        public static string ToHex(this Memory<byte> bytes)
        {
            return ByteToHex(bytes.Span);
        }

        public static string ToHex(this ReadOnlySpan<byte> bytes)
        {
            return ByteToHex(bytes);
        }

        public static string ToHex(this Span<byte> bytes)
        {
            return ByteToHex(bytes);
        }

        //converts a byte array to a hex string
        public static string ByteToHex(ReadOnlySpan<byte> bytes)
        {
            StringBuilder s = new StringBuilder(bytes.Length*2);
            foreach (byte b in bytes)
                s.Append(b.ToString("x2"));
            return s.ToString();
        }

        //converts a hex string to a byte array
        public static byte[] HexToByte(string hex)
        {
            if ((hex.Length & 1) != 0)
                throw new ArgumentException("uneven hex length");
            var r = new byte[hex.Length / 2];
            for (var i = 0; i < hex.Length - 1; i += 2)
            {
                var a = GetHex(hex[i]);
                var b = GetHex(hex[i + 1]);
                r[i / 2] = (byte)(a * 16 + b);
            }
            return r;
        }

        //converts a single hex character to it's decimal value
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte GetHex(char x)
        {
            if (x <= '9' && x >= '0')
            {
                return (byte)(x - '0');
            }
            else if (x <= 'z' && x >= 'a')
            {
                return (byte)(x - 'a' + 10);
            }
            else if (x <= 'Z' && x >= 'A')
            {
                return (byte)(x - 'A' + 10);
            }
            return 0;
        }
    }
}