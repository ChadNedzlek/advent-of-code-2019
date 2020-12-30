using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Threading.Tasks;

namespace AdventOfCode.Solutions
{
    public class Day15
    {
        public static async Task Problem1()
        {
            var data = await Data.GetDataLines();
            Dictionary<(int x, int y), Tile> map = new Dictionary<(int x, int y), Tile>();
            map.Add((0,0), Tile.Empty);
            IntCodeComputer computer = new IntCodeComputer(data[0].Split(',').Select(long.Parse));
            var run = computer.RunProgramAsync(out var input, out var output);
            // Start going north so we have something to work with
            await input.WriteAsync(DroidInput.North);
            var readOutput = output.ReadAsync().AsTask();
            int x = 0, y = 0, dx = 0, dy = -1;
            int minX = 0, minY = 0, maxX = 0, maxY = 0;
            D.Write('D');
            Queue<int> currentPath = new Queue<int>();

            void DrawMap()
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

                DrawMap();

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

            DrawMap();

            (int x, int y) oxygenLocation = map.First(m => m.Value == Tile.Oxygen).Key;
            var find = CalculateNewPath(map, (minX, maxX, minY, maxY), (0,0), oxygenLocation, int.MaxValue);
            Console.WriteLine();
            Console.WriteLine($"Shorted possible path is {find.Count} long");

            int furthest = 0;
            foreach (var (loc, tile) in map)
            {
                if (tile == Tile.Empty)
                {
                    var path = CalculateNewPath(map, (minX, maxX, minY, maxY), oxygenLocation, loc, int.MaxValue);
                    furthest = Math.Max(furthest, path.Count);
                }
            }
            Console.WriteLine();
            Console.WriteLine($"It will take Oxygen {furthest} minutes to spread");
        }

        private static Queue<int> CalculateNewPath(Dictionary<(int x, int y), Tile> map, (int x, int y) droidLocation)
        {
            var reachableMap = map.Where(p => p.Value != Tile.Unreachable).ToList();
            int minX = reachableMap.Min(k => k.Key.x) - 1;
            int maxX = reachableMap.Max(k => k.Key.x) + 1;
            int minY = reachableMap.Min(k => k.Key.y) - 1;
            int maxY = reachableMap.Max(k => k.Key.y) + 1;
            Queue<int> shortPath = null;
            
            int spanX = Math.Max(maxX - droidLocation.x, droidLocation.x - minX);
            int spanY = Math.Max(maxY - droidLocation.y, droidLocation.y - minY);

            for (int dy = 1; dy <= spanY; dy++)
            {
                for (int dx = 1; dx <= spanX; dx++)
                {
                    Queue<int> SearchLocation(int x, int y)
                    {
                        if (x < minX || x > maxX || y < minY || y > maxY)
                            return shortPath;

                        if (!map.ContainsKey((x, y)))
                        {
                            // A square we haven't filled in!
                            var path = CalculateNewPath(
                                map,
                                (minX, maxX, minY, maxY),
                                droidLocation,
                                (x, y),
                                shortPath?.Count ?? int.MaxValue
                            );
                            if (path != null)
                            {
                                if (shortPath == null || shortPath.Count > path.Count)
                                {
                                    shortPath = path;
                                }
                            }
                            else if (shortPath == null)
                            {
                                map.Add((x, y), Tile.Unreachable);
                            }
                        }

                        return shortPath;
                    }
                    
                    shortPath = SearchLocation(droidLocation.x - dx, droidLocation.y - dy);
                    shortPath = SearchLocation(droidLocation.x - dx, droidLocation.y + dy);
                    shortPath = SearchLocation(droidLocation.x + dx, droidLocation.y - dy);
                    shortPath = SearchLocation(droidLocation.x + dx, droidLocation.y + dy);
                }
            }

            // No reachable points
            return shortPath;
        }

        private static Queue<int> CalculateNewPath(
            Dictionary<(int x, int y), Tile> map,
            (int left, int right, int top, int bottom) bounds,
            (int x, int y) start,
            (int x, int y) target,
            int lengthLimit)
        {
            List<SearchNode> open = new List<SearchNode> {new(start)};
            List<SearchNode> closed = new List<SearchNode>();
            while (open.Count != 0)
            {
                var searchNode = open.OrderBy(
                        n => n.Length + Math.Abs(target.x - n.Location.x) + Math.Abs(target.y - n.Location.y)
                    )
                    .First();
                
                open.Remove(searchNode);
                closed.Add(searchNode);

                if (searchNode.Length >= lengthLimit)
                {
                    continue;
                }

                SearchNode TrySearch((int x, int y) loc)
                {
                    if (loc.x < bounds.left)
                        return null;
                    if (loc.x > bounds.right)
                        return null;
                    if (loc.y < bounds.top)
                        return null;
                    if (loc.y > bounds.bottom)
                        return null;

                    if (map.TryGetValue(loc, out var tile) && tile == Tile.Wall)
                        return null;

                    if (loc == target)
                    {
                        return new SearchNode(target) {BestParent = searchNode};
                    }

                    var o = open.FirstOrDefault(o => o.Location == loc);
                    var c = closed.FirstOrDefault(o => o.Location == loc);

                    if (o != null)
                    {
                        // We already have an open node
                        if (o.Length > searchNode.Length + 1)
                        {
                            // Our path is better, so update it, but leave it open
                            o.BestParent = searchNode;
                        }
                    }
                    else if (c != null)
                    {
                        // We've already "closed" this node
                        if (c.Length > searchNode.Length + 1)
                        {
                            // But we found a better path!
                            c.BestParent = searchNode;
                            closed.Remove(c);
                            open.Add(c);
                        }
                    }
                    else
                    {
                        // First time we've been here, add a new guy
                        open.Add(new SearchNode(loc) {BestParent = searchNode});
                    }

                    return null;
                }

                var neighborLocations = new (int x, int y)[]
                {
                    (searchNode.Location.x - 1, searchNode.Location.y),
                    (searchNode.Location.x + 1, searchNode.Location.y),
                    (searchNode.Location.x, searchNode.Location.y - 1),
                    (searchNode.Location.x, searchNode.Location.y + 1),
                };

                foreach (var n in neighborLocations)
                {
                    var foundTarget = TrySearch(n);

                    if (foundTarget != null)
                    {
                        List<(int x, int y)> reversePath = new List<(int x, int y)>();
                        while (foundTarget != null)
                        {
                            reversePath.Add(foundTarget.Location);
                            foundTarget = foundTarget.BestParent;
                        }

                        Queue<int> steps = new Queue<int>(reversePath.Count - 1);
                        for (int i = reversePath.Count - 1; i > 0; i--)
                        {
                            (int x, int y) from = reversePath[i];
                            (int x, int y) to = reversePath[i-1];
                            if (to.y == from.y - 1)
                                steps.Enqueue(DroidInput.North);
                            else if (to.y == from.y + 1)
                                steps.Enqueue(DroidInput.Sourth);
                            else if (to.x == from.x + 1)
                                steps.Enqueue(DroidInput.East);
                            else
                                steps.Enqueue(DroidInput.West);
                        }

                        return steps;
                    }
                }
            }

            return null;
        }

        public class SearchNode
        {
            public SearchNode((int x, int y) location)
            {
                Location = location;
            }

            public (int x, int y) Location { get; }
            public SearchNode BestParent { get; set; }
            public int Length => BestParent?.Length + 1 ?? 0;
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