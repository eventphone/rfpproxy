using System;

namespace RfpProxy
{
    public readonly struct RfpIdentifier:IEquatable<RfpIdentifier>
    {
        public static readonly int Length = 6;
        private readonly ReadOnlyMemory<byte> _identifier;

        public RfpIdentifier(ReadOnlyMemory<byte> identifier)
        {
            if (identifier.Length != 6)
                throw new ArgumentOutOfRangeException(nameof(identifier), "identifier must be 6 bytes");
            _identifier = identifier;
        }

        public bool Matches(RfpIdentifier other, ReadOnlySpan<byte> mask)
        {
            if (mask.Length != 6)
                throw new ArgumentOutOfRangeException(nameof(mask), "mask must be 6 bytes");

            for (int i = 0; i < _identifier.Length; i++)
            {
                var masked = _identifier.Span[i] & mask[i];
                if (masked != other._identifier.Span[i])
                {
                    return false;
                }
            }
            return true;
        }

        public void CopyTo(Memory<byte> target)
        {
            _identifier.CopyTo(target);
        }

        public bool Equals(RfpIdentifier other)
        {
            return _identifier.Length == other._identifier.Length &&
                   _identifier.Span.SequenceEqual(other._identifier.Span);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is RfpIdentifier other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _identifier.GetHashCode();
        }

        public static bool operator ==(RfpIdentifier left, RfpIdentifier right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RfpIdentifier left, RfpIdentifier right)
        {
            return !left.Equals(right);
        }
    }
}