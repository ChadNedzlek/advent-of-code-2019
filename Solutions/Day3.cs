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
            List<List<((int x, int y) start, (int x, int y) end)>> segments =
                new List<List<((int x, int y) start, (int x, int y) end)>>();
            foreach (string line in data)
            {
                List<((int x, int y) start, (int x, int y) end)> wire =
                    new List<((int x, int y) start, (int x, int y) end)>();
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
                    wire.Add(((x, y), (ex, ey)));
                    x = ex;
                    y = ey;
                }
            }

            int distance = int.MaxValue;
            foreach (var a in segments[0])
            foreach (var b in segments[1])
            {
                if (a.start.x == a.end.x && b.start.x == b.end.x)
                {
                    // both vertical
                    continue;
                }

                if (a.start.y == a.end.y && b.start.y == b.end.y)
                {
                    // both horizontal
                    continue;
                }

                var v = a.start.x == a.end.x ? a : b;
                var h = v == a ? b : a;

                int horzLowX = Math.Min(h.start.x, h.end.x), horzHighX = Math.Max(h.start.x, h.end.x);
                int vertLowY = Math.Min(v.start.y, v.end.y), vertHighY = Math.Max(v.start.y, v.end.y);
                int vertX = v.start.x;
                int horzY = h.start.y;
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
            List<List<((int x, int y) start, (int x, int y) end)>> segments =
                new List<List<((int x, int y) start, (int x, int y) end)>>();
            foreach (string line in data)
            {
                List<((int x, int y) start, (int x, int y) end)> wire =
                    new List<((int x, int y) start, (int x, int y) end)>();
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
                    wire.Add(((x, y), (ex, ey)));
                    x = ex;
                    y = ey;
                }
            }

            int distance = int.MaxValue;
            int la = 0;
            foreach (var a in segments[0])
            {
                int lb = 0;
                la += Math.Abs(a.end.x - a.start.x) + Math.Abs(a.end.y - a.start.y);
                foreach (var b in segments[1])
                {
                    lb += Math.Abs(b.end.x - b.start.x) + Math.Abs(b.end.y - b.start.y);
                    if (a.start.x == a.end.x && b.start.x == b.end.x)
                    {
                        // both vertical
                        continue;
                    }

                    if (a.start.y == a.end.y && b.start.y == b.end.y)
                    {
                        // both horizontal
                        continue;
                    }

                    var v = a.start.x == a.end.x ? a : b;
                    var h = v == a ? b : a;

                    int horzLowX = Math.Min(h.start.x, h.end.x), horzHighX = Math.Max(h.start.x, h.end.x);
                    int vertLowY = Math.Min(v.start.y, v.end.y), vertHighY = Math.Max(v.start.y, v.end.y);
                    int vertX = v.start.x;
                    int horzY = h.start.y;
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
                    int saved = Math.Abs(h.end.x - vertX) + Math.Abs(v.end.y - horzY);
                    distance = Math.Min(distance, la + lb - saved);
                }
            }

            Console.WriteLine($"Least wirey intersection : {distance}");
        }
    }
}