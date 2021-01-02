using System;
using System.Buffers;
using System.Text;

namespace RfpProxyLib
{
    public static class Extensions
    {
        public static string CString(this ReadOnlySpan<byte> data)
        {
            var eos = data.IndexOf((byte) 0);
            if (eos < 0)
                return String.Empty;
            return Encoding.UTF8.GetString(data.Slice(0, eos));
        }

        public static bool IsEmpty(this Span<byte> data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] != 0) return false;
            }
            return true;
        }

        public static bool IsEmpty(this ReadOnlySpan<byte> data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] != 0) return false;
            }
            return true;
        }

        public static ReadOnlyMemory<byte> ToMemory(this ReadOnlySequence<byte> source)
        {
            if (source.IsSingleSegment)
            {
                return source.First;
            }
            return source.ToArray();
        }
    }
}