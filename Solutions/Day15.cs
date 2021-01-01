using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AdventOfCode.Solutions
{
    public class Day15
    {
        public class TileMap : IMap<IntCo2, int>
        {
            public TileMap(Dictionary<IntCo2, Tile> map)
            {
                _map = map;
            }

            private readonly Dictionary<IntCo2, Tile> _map;
            public Tile this[IntCo2 key]
            {
                get => _map[key];
                set => _map[key] = value;
            }

            public bool IsPassable(IntCo2 loc, int ignoredState)
            {
                Rectangle rect = GetBounds();
                if (loc.X < rect.Left || loc.X > rect.Right || loc.Y < rect.Top || loc.Y > rect.Bottom)
                    return false;

                return !_map.TryGetValue((loc.X, loc.Y), out var t) || t != Tile.Wall;
            }

            private Rectangle? _cachedBounds = null;
            public Rectangle GetBounds()
            {
                if (!_cachedBounds.HasValue)
                {
                    var reachableMap = _map.Where(p => p.Value != Tile.Unreachable).ToList();
                    _cachedBounds = new Rectangle(
                        top: reachableMap.Min(k => k.Key.Y) - 1,
                        bottom: reachableMap.Max(k => k.Key.Y) + 1,
                        left: reachableMap.Min(k => k.Key.X) - 1,
                        right: reachableMap.Max(k => k.Key.X) + 1
                    );
                }

                return _cachedBounds.Value;
            }

            public IEnumerable<IntCo2> GetNeighbors(IntCo2 location)
            {
                return new IntCo2[]
                {
                    new(location.X - 1, location.Y),
                    new(location.X + 1, location.Y),
                    new(location.X, location.Y - 1),
                    new(location.X, location.Y + 1),
                };
            }

            public int EstimateDistance(IntCo2 a, IntCo2 b)
            {
                return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
            }

            public void CalculateStep(IntCo2 from, IntCo2 to, int currentState, out int cost, out int newState)
            {
                cost = EstimateDistance(from, to);
                newState = 0;
            }

            public bool IsAt(IntCo2 loc, IntCo2 target)
            {
                return loc == target;
            }

            public bool TryGetValue(IntCo2 loc, out Tile tile) => _map.TryGetValue(loc, out tile);

            public void Add(IntCo2 location, Tile tile)
            {
                _map.Add(location, tile);
                _cachedBounds = null;
            }
        }

        public static async Task Problem1()
        {
            Stopwatch s = Stopwatch.StartNew();
            var data = await Data.GetDataLines();
            Dictionary<IntCo2, Tile> tiles = new Dictionary<IntCo2, Tile>();
            tiles.Add((0,0), Tile.Empty);
            IntCodeComputer computer = new IntCodeComputer(data[0].Split(',').Select(long.Parse));
            var run = computer.RunProgramAsync(out var input, out var output);
            // Start going north so we have something to work with
            await input.WriteAsync(DroidInput.North);
            var readOutput = output.ReadAsync().AsTask();
            int x = 0, y = 0, dx = 0, dy = -1;
            int minX = 0, minY = 0, maxX = 0, maxY = 0;
            D.Write('D');
            Queue<int> currentPath = new Queue<int>();
            TileMap map = new TileMap(tiles);

            while (true)
            {
                var t = await Task.WhenAny(run, readOutput);
                if (t == run)
                {
                    D.WriteLine("Program halted");
                    break;
                }

                void UpdateMap(bool moved, Tile value){
                    map[(x + dx, y + dy)] = value;
                    minX = Math.Min(x + dx, minX);
                    maxX = Math.Max(x + dx, maxX);
                    minY = Math.Min(y + dy, minY);
                    maxY = Math.Max(y + dy, maxY);
                    if (moved)
                    {
                        x += dx;
                        y += dy;
                    }
                    dx = 0;
                    dy = 0;
                }

                var o = await readOutput;
                switch (o)
                {
                    case DroidOutput.HitWall:
                        UpdateMap(false, Tile.Wall);
                        currentPath = null;
                        break;
                    case DroidOutput.Moved:
                        UpdateMap(true, Tile.Empty);
                        break;
                    case DroidOutput.FoundOxygen:
                        UpdateMap(true, Tile.Oxygen);
                        break;

                }

                readOutput = output.ReadAsync().AsTask();

                DrawMap(minY, maxY, minX, maxX, map, x, y);

                if (currentPath == null || currentPath.Count == 0)
                {
                    currentPath = CalculateNewPath(map, (x, y));
                    if (currentPath == null)
                        break;
                }

                int step = currentPath.Dequeue();
                await input.WriteAsync(step);
                switch (step)
                {
                    case DroidInput.North:
                        dy = -1;
                        break;
                    case DroidInput.Sourth:
                        dy = 1;
                        break;
                    case DroidInput.West:
                        dx = -1;
                        break;
                    case DroidInput.East:
                        dx = 1;
                        break;
                }
            }

            DrawMap(minY, maxY, minX, maxX, map, x, y);

            IntCo2 oxygenLocation = tiles.First(m => m.Value == Tile.Oxygen).Key;
            var find = AStar.CalculateNewPath(map, (0,0), oxygenLocation);
            Console.WriteLine();
            Console.WriteLine($"Shorted possible path is {find.Count} long");

            int furthest = 0;
            foreach (var (loc, tile) in tiles)
            {
                if (tile == Tile.Empty)
                {
                    var path = AStar.CalculateNewPath(map, oxygenLocation, loc);
                    furthest = Math.Max(furthest, path.Count);
                }
            }
            Console.WriteLine();
            Console.WriteLine($"It will take Oxygen {furthest} minutes to spread");
            s.Stop();
            Console.WriteLine();
            Console.WriteLine($"Solved in {s.Elapsed}");
        }

        [Conditional("DEBUG")]
        private static void DrawMap(int minY, int maxY, int minX, int maxX, TileMap map, int x, int y)
        {
            D.SetCursorPosition(0, 0);
            for (int ry = minY; ry <= maxY; ry++)
            {
                for (int rx = minX; rx <= maxX; rx++)
                {
                    if (map.TryGetValue((rx, ry), out Tile tile))
                    {
                        switch (tile)
                        {
                            case Tile.Wall:
                                D.Write('\u2588');
                                break;
                            case Tile.Empty:
                                if (rx == x && ry == y)
                                {
                                    D.Write('D');
                                }
                                else
                                {
                                    D.Write(' ');
                                }

                                break;
                            case Tile.Oxygen:
                                D.Write('O');
                                break;
                            case Tile.Unreachable:
                                D.Write('-');
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    else
                    {
                        D.Write('?');
                    }
                }

                D.WriteLine("");
            }
        }

        private static Queue<int> CalculateNewPath(TileMap map, IntCo2 droidLocation)
        {
            Queue<int> shortPath = null;
            var bounds = map.GetBounds();
            
            int spanX = Math.Max(bounds.Right - droidLocation.X, droidLocation.X - bounds.Left);
            int spanY = Math.Max(bounds.Bottom - droidLocation.Y, droidLocation.Y - bounds.Top);

            for (int dy = 1; dy <= spanY; dy++)
            {
                for (int dx = 1; dx <= spanX; dx++)
                {
                    Queue<int> SearchLocation(int x, int y)
                    {
                        if (x < bounds.Left || x > bounds.Right || y < bounds.Top || y > bounds.Bottom)
                            return shortPath;

                        if (!map.TryGetValue((x, y), out _))
                        {
                            // A square we haven't filled in!
                            List<IntCo2> path = AStar.CalculateNewPath(
                                map,
                                droidLocation,
                                (x, y),
                                lengthLimit: shortPath?.Count ?? int.MaxValue
                            );

                            if (path != null)
                            {
                                Queue<int> steps = new Queue<int>(path.Count - 1);
                                for (int i = 1; i < path.Count; i++)
                                {
                                    IntCo2 from = path[i-1];
                                    IntCo2 to = path[i];
                                    if (to.Y == from.Y - 1)
                                        steps.Enqueue(DroidInput.North);
                                    else if (to.Y == from.Y + 1)
                                        steps.Enqueue(DroidInput.Sourth);
                                    else if (to.X == from.X + 1)
                                        steps.Enqueue(DroidInput.East);
                                    else
                                        steps.Enqueue(DroidInput.West);
                                }

                                if (shortPath == null || shortPath.Count > path.Count)
                                {
                                    shortPath = steps;
                                }
                            }
                            else if (shortPath == null)
                            {
                                map.Add((x, y), Tile.Unreachable);
                            }
                        }

                        return shortPath;
                    }
                    
                    shortPath = SearchLocation(droidLocation.X - dx, droidLocation.Y - dy);
                    shortPath = SearchLocation(droidLocation.X - dx, droidLocation.Y + dy);
                    shortPath = SearchLocation(droidLocation.X + dx, droidLocation.Y - dy);
                    shortPath = SearchLocation(droidLocation.X + dx, droidLocation.Y + dy);
                }
            }

            // No reachable points
            return shortPath;
        }

        public enum Tile
        {
            Wall,
            Empty,
            Oxygen,
            Unreachable,
        }

        public static class DroidInput
        {
            public const int North = 1;
            public const int Sourth = 2;
            public const int West = 3;
            public const int East = 4;
        }

        public static class DroidOutput
        {
            public const int HitWall = 0;
            public const int Moved = 1;
            public const int FoundOxygen = 2;
        }
    }
}