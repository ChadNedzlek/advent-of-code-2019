using System;

namespace AdventOfCode
{
    public struct IntSegment2 : IEquatable<IntSegment2>
    {
        public IntSegment2(IntCo2 a, IntCo2 b)
        {
            A = a;
            B = b;
        }

        public IntCo2 A { get; }
        public IntCo2 B { get; }

        public bool Equals(IntSegment2 other)
        {
            return A.Equals(other.A) && B.Equals(other.B) || A.Equals(other.B) && B.Equals(other.A);
        }

        public override bool Equals(object obj)
        {
            return obj is IntSegment2 other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // Intentionally symmetric
                return A.GetHashCode() ^ B.GetHashCode();
            }
        }

        public static bool operator ==(IntSegment2 left, IntSegment2 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(IntSegment2 left, IntSegment2 right)
        {
            return !left.Equals(right);
        }

        public override string ToString() => $"{A} <-> {B}";
    }
}