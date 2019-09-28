using System;

namespace SuperMarioBrothers
{
    public class SequenceMatch
    {
        public Memory<IndexedTone> Left { get; }
            
        public Memory<IndexedTone> Right { get; }

        public int Length => Left.Length;

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