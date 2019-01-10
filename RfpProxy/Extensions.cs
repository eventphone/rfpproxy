using System;
using System.Buffers;

namespace RfpProxy
{
    public static class Extensions
    {
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