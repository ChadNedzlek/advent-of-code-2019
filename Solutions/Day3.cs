using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdventOfCode.Solutions
{
    public class Day3
    {
        public static async Task Problem1()
        {
            var data = await Data.GetDataLines();
            List<List<(IntCo2 start, IntCo2 end)>> segments =
                new List<List<(IntCo2 start, IntCo2 end)>>();
            foreach (string line in data)
            {
                List<(IntCo2 start, IntCo2 end)> wire =
                    new List<(IntCo2 start, IntCo2 end)>();
                segments.Add(wire);

                int x = 0, y = 0;
                foreach (string move in line.Split(','))
                {
                    int dx = 0, dy = 0;
                    switch (move[0])
                    {
                        case 'U':
                            dy = 1;
                            break;
                        case 'D':
                            dy = -1;
                            break;
                        case 'L':
                            dx = -1;
                            break;
                        case 'R':
                            dx = 1;
                            break;
                    }

                    int l = int.Parse(move[1..]);
                    int ex = x + dx * l, ey = y + dy * l;
                    wire.Add((new (x, y), (ex, ey)));
                    x = ex;
                    y = ey;
                }
            }

            int distance = int.MaxValue;
            foreach (var a in segments[0])
            foreach (var b in segments[1])
            {
                if (a.start.X == a.end.X && b.start.X == b.end.X)
                {
                    // both vertical
                    continue;
                }

                if (a.start.Y == a.end.Y && b.start.Y == b.end.Y)
                {
                    // both horizontal
                    continue;
                }

                var v = a.start.X == a.end.X ? a : b;
                var h = v == a ? b : a;

                int horzLowX = Math.Min(h.start.X, h.end.X), horzHighX = Math.Max(h.start.X, h.end.X);
                int vertLowY = Math.Min(v.start.Y, v.end.Y), vertHighY = Math.Max(v.start.Y, v.end.Y);
                int vertX = v.start.X;
                int horzY = h.start.Y;
                if (vertX < horzLowX || vertX > horzHighX)
                {
                    // the vertical one is left or right of the horizontal one
                    continue;
                }

                if (horzY < vertLowY || horzY > vertHighY)
                {
                    // the horizontal one is above or below the vertical one
                    continue;
                }

                if (vertX == 0 && horzY == 0)
                {
                    // ignore root
                    continue;
                }

                distance = Math.Min(distance, Math.Abs(vertX) + Math.Abs(horzY));
            }

            Console.WriteLine($"Closest intersection : {distance}");
        }

        public static async Task Problem2()
        {
            var data = await Data.GetDataLines();
            List<List<(IntCo2 start, IntCo2 end)>> segments =
                new List<List<(IntCo2 start, IntCo2 end)>>();
            foreach (string line in data)
            {
                List<(IntCo2 start, IntCo2 end)> wire =
                    new List<(IntCo2 start, IntCo2 end)>();
                segments.Add(wire);

                int x = 0, y = 0;
                foreach (string move in line.Split(','))
                {
                    int dx = 0, dy = 0;
                    switch (move[0])
                    {
                        case 'U':
                            dy = 1;
                            break;
                        case 'D':
                            dy = -1;
                            break;
                        case 'L':
                            dx = -1;
                            break;
                        case 'R':
                            dx = 1;
                            break;
                    }

                    int l = int.Parse(move[1..]);
                    int ex = x + dx * l, ey = y + dy * l;
                    wire.Add((new (x, y), (ex, ey)));
                    x = ex;
                    y = ey;
                }
            }

            int distance = int.MaxValue;
            int la = 0;
            foreach (var a in segments[0])
            {
                int lb = 0;
                la += Math.Abs(a.end.X - a.start.X) + Math.Abs(a.end.Y - a.start.Y);
                foreach (var b in segments[1])
                {
                    lb += Math.Abs(b.end.X - b.start.X) + Math.Abs(b.end.Y - b.start.Y);
                    if (a.start.X == a.end.X && b.start.X == b.end.X)
                    {
                        // both vertical
                        continue;
                    }

                    if (a.start.Y == a.end.Y && b.start.Y == b.end.Y)
                    {
                        // both horizontal
                        continue;
                    }

                    var v = a.start.X == a.end.X ? a : b;
                    var h = v == a ? b : a;

                    int horzLowX = Math.Min(h.start.X, h.end.X), horzHighX = Math.Max(h.start.X, h.end.X);
                    int vertLowY = Math.Min(v.start.Y, v.end.Y), vertHighY = Math.Max(v.start.Y, v.end.Y);
                    int vertX = v.start.X;
                    int horzY = h.start.Y;
                    if (vertX < horzLowX || vertX > horzHighX)
                    {
                        // the vertical one is left or right of the horizontal one
                        continue;
                    }

                    if (horzY < vertLowY || horzY > vertHighY)
                    {
                        // the horizontal one is above or below the vertical one
                        continue;
                    }

                    if (vertX == 0 && horzY == 0)
                    {
                        // ignore root
                        continue;
                    }

                    // We didn't use this part of the wire
                    int saved = Math.Abs(h.end.X - vertX) + Math.Abs(v.end.Y - horzY);
                    distance = Math.Min(distance, la + lb - saved);
                }
            }

            Console.WriteLine($"Least wirey intersection : {distance}");
        }
    }
}