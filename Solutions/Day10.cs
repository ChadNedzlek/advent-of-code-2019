using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace AdventOfCode.Solutions
{
    public class Day10
    {
        public static async Task Problem1()
        {
            var data = await Data.GetDataLines();
            bool [,] space = new bool[data.Length,data[0].Length];
            (int x, int y) best = MostSighted(data, space, out int mostSighted);

            Console.WriteLine($"Best location ({best.x}, {best.y}) can sight {mostSighted} asteroids");

        }

        public static async Task Problem2()
        {
            var data = await Data.GetDataLines();
            bool [,] space = new bool[data.Length,data[0].Length];
            (int x, int y) center = MostSighted(data, space, out _);

            int cx = space.GetUpperBound(0) + 1;
            int cy = space.GetUpperBound(1) + 1;

            double?[,] angles = new double?[space.GetUpperBound(0) + 1, space.GetUpperBound(1) + 1];
            (int x, int y)? zap;
            for (int i = 0; i < 199; i++)
            {
                zap = GetAsteroid(angles);
                if (!zap.HasValue)
                {
                    CalculateAngles(space, center, angles);
                }

                zap = GetAsteroid(angles);
                space[zap.Value.x, zap.Value.y] = false;
                angles[zap.Value.x, zap.Value.y] = null;
            }

            zap = GetAsteroid(angles);
            if (!zap.HasValue)
                CalculateAngles(space, center, angles);
            zap = GetAsteroid(angles);

            Console.WriteLine($"200th zap at ({zap.Value.x}, {zap.Value.y}) => {zap.Value.x * 100 + zap.Value.y}");
        }

        private static void CalculateAngles(bool[,] space, (int x, int y) center, double?[,] angles)
        {
            int cx = space.GetUpperBound(0) + 1;
            int cy = space.GetUpperBound(1) + 1;
            for (int dx = -cx; dx < cx; dx++)
            {
                for (int dy = -cy; dy < cy; dy++)
                {
                    if (Gcd(Math.Abs(dx), Math.Abs(dy)) != 1)
                        continue;

                    for (int i = 1; i < Math.Max(cx, cy); i++)
                    {
                        int tx = center.x + dx * i;
                        int ty = center.y + dy * i;
                        if (tx < 0 || tx >= cx || ty < 0 || ty >= cy)
                            break;

                        if (space[tx, ty])
                        {
                            angles[tx, ty] = (Math.Atan2(dy, dx) - Math.Atan2(-1, 0) + 2 * Math.PI) % (2 * Math.PI);
                            break;
                        }
                    }
                }
            }
        }

        private static (int x, int y)? GetAsteroid(double?[,] angles)
        {
            double first = 1000;
            (int x, int y)? best = null;
            for (int x = 0; x <= angles.GetUpperBound(0); x++)
            {
                for (int y = 0; y <= angles.GetUpperBound(1); y++)
                {
                    if (angles[x, y] < first)
                    {
                        first = angles[x, y].Value;
                        best = (x, y);
                    }
                }
            }

            return best;
        }

        private static (int x, int y) MostSighted(string[] data, bool[,] space, out int mostSighted)
        {
            int cy = data.Length;
            int cx = data[0].Length;
            for (int y = 0; y < cy; y++)
            {
                string line = data[y];
                for (int x = 0; x < cx; x++)
                {
                    space[x, y] = line[x] == '#';
                }
            }

            mostSighted = 0;
            (int x, int y) best = (0, 0);
            for (int y = 0; y < cy; y++)
            {
                for (int x = 0; x < cx; x++)
                {
                    int sightable = 0;
                    for (int dx = -cx; dx < cx; dx++)
                    {
                        for (int dy = -cy; dy < cy; dy++)
                        {
                            if (Gcd(Math.Abs(dx), Math.Abs(dy)) != 1)
                                continue;

                            for (int i = 1; i < Math.Max(cx, cy); i++)
                            {
                                int tx = x + dx * i;
                                int ty = y + dy * i;
                                if (tx < 0 || tx >= cx || ty < 0 || ty >= cy)
                                    break;

                                if (space[tx, ty])
                                {
                                    sightable++;
                                    break;
                                }
                            }
                        }
                    }

                    if (sightable > mostSighted)
                    {
                        mostSighted = sightable;
                        best = (x, y);
                    }
                }
            }

            return best;
        }

        private static int Gcd(int a, int b) {
            int t;
            while(b != 0){
                t = a;
                a = b;
                b = t%b;
            }
            return a;
        }
    }
}