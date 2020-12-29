using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace AdventOfCode.Solutions
{
    public class Day12
    {
        public static async Task Problem1()
        {
            var data = await Data.GetDataLines();
            List<Vec> positions = new List<Vec>();
            List<Vec> velocities = new List<Vec>();
            foreach (string line in data)
            {
                Rx.M(line, @"<x=(-?\d+), y=(-?\d+), z=(-?\d+)>", out int x, out int y, out int z);
                positions.Add(new Vec(x, y, z));
                velocities.Add(new Vec(0, 0, 0));
            }

            for (int i = 0; i < 3120; i++)
            {
                for (int index = 0; index < positions.Count; index++)
                {
                    var a = positions[index];
                    foreach (var b in positions)
                    {
                        velocities[index] += a.GravityToward(b);
                    }
                }

                for (int index = 0; index < positions.Count; index++)
                {
                    positions[index] += velocities[index];
                }
            }

            long energy = 0;
            for (int i = 0; i < positions.Count; i++)
            {
                Vec position = positions[i];
                Vec velocity = velocities[i];
                int potential = Math.Abs(position.X) + Math.Abs(position.Y) + Math.Abs(position.Z);
                int kinetic = Math.Abs(velocity.X) + Math.Abs(velocity.Y) + Math.Abs(velocity.Z);
                energy += potential * kinetic;
            }

            Console.WriteLine($"Total energy is {energy}");
        }

        public static async Task Problem2()
        {
            var data = await Data.GetDataLines();
            List<Vec> positions = new List<Vec>();
            List<Vec> velocities = new List<Vec>();
            Vec cycle = new Vec();
            foreach (string line in data)
            {
                Rx.M(line, @"<x=(-?\d+), y=(-?\d+), z=(-?\d+)>", out int x, out int y, out int z);
                positions.Add(new Vec(x, y, z));
                velocities.Add(new Vec(0, 0, 0));
            }

            List<Vec> initial = positions.ToList();

            for (int i = 0; cycle.X == 0 || cycle.Y == 0 || cycle.Z == 0; i++)
            {
                for (int index = 0; index < positions.Count; index++)
                {
                    var a = positions[index];
                    foreach (var b in positions)
                    {
                        velocities[index] += a.GravityToward(b);
                    }
                }

                for (int index = 0; index < positions.Count; index++)
                {
                    Vec v = velocities[index];
                    positions[index] += v;
                }

                if (cycle.X == 0 &&
                    velocities.All(v => v.X == 0) &&
                    positions.Zip(initial).All(z => z.First.X == z.Second.X))
                    cycle = new Vec(i + 1, cycle.Y, cycle.Z);

                if (cycle.Y == 0 &&
                    velocities.All(v => v.Y == 0) &&
                    positions.Zip(initial).All(z => z.First.Y == z.Second.Y))
                    cycle = new Vec(cycle.X, i + 1, cycle.Z);

                if (cycle.Z == 0 &&
                    velocities.All(v => v.Z == 0) &&
                    positions.Zip(initial).All(z => z.First.Z == z.Second.Z))
                    cycle = new Vec(cycle.X, cycle.Y, i + 1);
            }

            Console.WriteLine($"X loop: {FS(cycle.X)} = {cycle.X}");
            Console.WriteLine($"Y loop: {FS(cycle.Y)} = {cycle.Y}");
            Console.WriteLine($"Z loop: {FS(cycle.Z)} = {cycle.Z}");

            Console.WriteLine("");

            Console.WriteLine($"Smallest cycle is {GCM(cycle.X, cycle.Y, cycle.Z)}");
        }

        public static string FS(long num) => FS(PrimeFactor(num));

        public static string FS(Dictionary<int, int> factors)
        {
            return string.Join(" x ", factors.OrderBy(p => p.Key).Select(p => $"{p.Key} ^ {p.Value}"));
        }

        public static long GCM(params long[] nums)
        {
            Dictionary<int, int> factors = new Dictionary<int, int>();
            foreach (var num in nums)
            {
                var f = PrimeFactor(num);
                foreach (var (k, v) in f)
                {
                    factors[k] = Math.Max(v, factors.GetValueOrDefault(k));
                }
            }

            long m = 1;
            foreach (var (k, v) in factors)
            {
                for (int i = 0; i < v; i++)
                {
                    m *= k;
                }
            }

            return m;
        }

        public static Dictionary<int, int> PrimeFactor(long x)
        {
            Dictionary<int, int> f = new Dictionary<int, int>();
            while (x != 1)
            {
                for (int i = 2;; i++)
                {
                    if (x % i == 0)
                    {
                        f[i] = f.GetValueOrDefault(i) + 1;
                        x = x / i;
                        break;
                    }
                }
            }

            return f;
        }

        public struct Vec
            : IEquatable<Vec>
        {
            public int X { get; }
            public int Y { get; }
            public int Z { get; }

            public Vec(int x, int y, int z)
            {
                X = x;
                Y = y;
                Z = z;
            }

            public static Vec operator +(Vec a, Vec b)
            {
                return new Vec(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
            }

            public Vec GravityToward(Vec target)
            {
                return new Vec(Math.Sign(target.X - X), Math.Sign(target.Y - Y), Math.Sign(target.Z - Z));
            }

            public bool Equals(Vec other)
            {
                return X == other.X && Y == other.Y && Z == other.Z;
            }

            public override bool Equals(object obj)
            {
                return obj is Vec other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(X, Y, Z);
            }

            public static bool operator ==(Vec left, Vec right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(Vec left, Vec right)
            {
                return !left.Equals(right);
            }
        }
    }
}