using System;
using System.IO.MemoryMappedFiles;

namespace SuperMarioBrothers
{
    public class SequenceMatch
    {
        public bool IsGapless => Right.Span[0].Index == Left.Span[Length - 1].Index + 1;

        public Memory<IndexedTone> Left { get; }
            
        public Memory<IndexedTone> Right { get; }

        public int Length => Left.Length;

        public int Start => Left.Span[0].Index;

        public int End => Right.Span[Length - 1].Index;

        public int Between => Right.Span[0].Index - Left.Span[Length - 1].Index - 1;

        public SequenceMatch(Memory<IndexedTone> left, Memory<IndexedTone> right)
        {
            if (left.Length != right.Length)
                throw new ArgumentException(nameof(right));

            if (left.Span[0].Index < right.Span[0].Index)
            {
                Left = left;
                Right = right;
            }
            else
            {
                Left = right;
                Right = left;
            }
        }
    }
}