using System;

namespace AdventOfCode
{
    public readonly struct IntCo2 : IEquatable<IntCo2>
    {
        public int X { get; }
        public int Y { get; }

        public IntCo2(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(IntCo2 other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is IntCo2 other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public static bool operator ==(IntCo2 left, IntCo2 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(IntCo2 left, IntCo2 right)
        {
            return !left.Equals(right);
        }

        public static implicit operator IntCo2((int x, int y) tuple)
        {
            return new IntCo2(tuple.x, tuple.y);
        }

        public int OrthogonalDistance(IntCo2 other)
        {
            return Math.Abs(other.X - X) + Math.Abs(other.Y - Y);
        }

        public override string ToString() => $"({X},{Y})";
    }
}