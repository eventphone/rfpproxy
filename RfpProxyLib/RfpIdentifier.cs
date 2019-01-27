using System;
using System.Buffers.Binary;

namespace RfpProxyLib
{
    public readonly struct RfpIdentifier:IEquatable<RfpIdentifier>
    {
        public static readonly int Length = 6;
        private readonly ReadOnlyMemory<byte> _identifier;

        public RfpIdentifier(ReadOnlyMemory<byte> identifier)
        {
            if (identifier.Length != Length)
                throw new ArgumentOutOfRangeException(nameof(identifier), $"identifier must be {Length} bytes");
            _identifier = identifier;
        }

        public bool Matches(RfpIdentifier other, ReadOnlySpan<byte> mask)
        {
            if (mask.Length != Length)
                throw new ArgumentOutOfRangeException(nameof(mask), $"mask must be {Length} bytes");

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

        public void CopyTo(Span<byte> target)
        {
            _identifier.Span.CopyTo(target);
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
            var first = BinaryPrimitives.ReadInt32BigEndian(_identifier.Span);
            var second = BinaryPrimitives.ReadInt32BigEndian(_identifier.Span.Slice(2));
            return first ^ second;
        }

        public static bool operator ==(RfpIdentifier left, RfpIdentifier right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RfpIdentifier left, RfpIdentifier right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return HexEncoding.ByteToHex(_identifier.Span);
        }
    }
}