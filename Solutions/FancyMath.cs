using System;
using System.Numerics;

namespace AdventOfCode.Solutions
{
    internal static class FancyMath
    {
        public static int Gcd(int a, int b)
        {
            int t;
            while (b != 0)
            {
                t = a;
                a = b;
                b = t % b;
            }

            return a;
        }

        public static long Gcd(long a, long b)
        {
            long t;
            while (b != 0)
            {
                t = a;
                a = b;
                b = t % b;
            }

            return a;
        }

        public static bool TryModInverse(long number, long modulo, out long result)
        {
            if (number < 1) throw new ArgumentOutOfRangeException(nameof(number));
            if (modulo < 2) throw new ArgumentOutOfRangeException(nameof(modulo));
            long n = number;
            long m = modulo, v = 0, d = 1;
            while (n > 0)
            {
                long t = m / n, x = n;
                n = m % x;
                m = x;
                x = d;
                d = checked(v - t * x); // Just in case
                v = x;
            }

            result = v % modulo;
            if (result < 0) result += modulo;
            if (MulMod(number, result, modulo) == 1) return true;
            result = default;
            return false;
        }

        public static long ModInverse(long number, long modulo)
        {
            if (!TryModInverse(number, modulo, out long result))
                throw new ArgumentException("No multiplicative inverse");
            return result;
        }

        public static long MulMod(long a, long b, long modulus)
        {
            try
            {
                return a * b % modulus;
            }
            catch (OverflowException)
            {
                return (long) (new BigInteger(a) * b % modulus);
            }
        }
    }
}