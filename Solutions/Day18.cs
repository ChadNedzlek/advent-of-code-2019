using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AdventOfCode.Solutions
{
    public class Day18
    {
        public readonly struct KeyContainer
            : IEquatable<KeyContainer>
        {
            private readonly int _store;
            public int Count { get; }

            private KeyContainer(int store, int count)
            {
                _store = store;
                Count = count;
            }

            private int GetMask(char key)
            {
                Debug.Assert(char.IsLower(key));
                return 1 << (key - 'a');
            }

            public static char KeyForDoor(char door)
            {
                // This is a fast "to lower" implementation if
                // we know we are ASCII letters
                return (char) (door | 0x20);
            }

            public bool HasKey(char key)
            {
                Debug.Assert(char.IsLower(key));
                return (_store & GetMask(key)) != 0;
            }

            public static bool IsKey(char tile)
            {
                return char.IsLower(tile);
            }

            public bool HasKeyForDoor(char door)
            {
                Debug.Assert(char.IsUpper(door));
                return (_store & GetMask(KeyForDoor(door))) != 0;
            }

            public KeyContainer WithKey(char key)
            {
                Debug.Assert(_store != (_store | GetMask(key)));
                return new KeyContainer(_store | GetMask(key), Count + 1);
            }

            public bool Equals(KeyContainer other)
            {
                return _store == other._store;
            }

            public override bool Equals(object obj)
            {
                return obj is KeyContainer other && Equals(other);
            }

            public override int GetHashCode()
            {
                return _store;
            }

            public static bool operator ==(KeyContainer left, KeyContainer right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(KeyContainer left, KeyContainer right)
            {
                return !left.Equals(right);
            }

            public override string ToString()
            {
                return string.Join(',', Enumerable.Range('a', 26).Select(i => (char)i).Where(HasKey));
            }
        }

        public readonly struct DoorSpace : IEquatable<DoorSpace>
        {
            public DoorSpace(IntCo2 location, KeyContainer keys)
            {
                Location = location;
                Keys = keys;
            }

            public IntCo2 Location { get; }
            public KeyContainer Keys { get; }

            public bool Equals(DoorSpace other)
            {
                return Location.Equals(other.Location) && Keys == other.Keys;
            }

            public override bool Equals(object obj)
            {
                return obj is DoorSpace other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Location.GetHashCode() * 397) ^ Keys.GetHashCode();
                }
            }

            public static bool operator ==(DoorSpace left, DoorSpace right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(DoorSpace left, DoorSpace right)
            {
                return !left.Equals(right);
            }

            public override string ToString()
            {
                return $"[{Location} K{Keys}]";
            }
        }

        public class MapWithDoors : IMap<DoorSpace, KeyContainer>
        {
            private readonly char[,] _tiles;
            public ImmutableDictionary<char, IntCo2> DoorLocations { get; }
            public ImmutableDictionary<char, IntCo2> KeyLocations { get; }

            public MapWithDoors(char[,] tiles)
            {
                _tiles = tiles;
                var db = ImmutableDictionary.CreateBuilder<char, IntCo2>();
                var kb = ImmutableDictionary.CreateBuilder<char, IntCo2>();
                for (int x = 0; x <= tiles.GetUpperBound(0); x++)
                {
                    for (int y = 0; y <= tiles.GetUpperBound(1); y++)
                    {
                        char tile = tiles[x, y];
                        if (char.IsUpper(tile))
                        {
                            db.Add(tile, new (x, y));
                        }

                        if (char.IsLower(tile))
                        {
                            kb.Add(tile, new (x, y));
                        }
                    }
                }

                DoorLocations = db.ToImmutable();
                KeyLocations = kb.ToImmutable();
            }

            public bool IsPassable(DoorSpace p, KeyContainer keysHeld)
            {
                if (p.Keys != keysHeld)
                    return false;

                var tile = _tiles[p.Location.X, p.Location.Y];
                if (tile == '.' || char.IsLower(tile))
                {
                    return true;
                }
                else if (tile == '#')
                {
                    return false;
                }
                else
                {
                    return keysHeld.HasKey(KeyContainer.KeyForDoor(tile));
                }
            }

            public IEnumerable<DoorSpace> GetNeighbors(DoorSpace location)
            {
                yield return new(new(location.Location.X - 1, location.Location.Y), location.Keys);
                yield return new(new(location.Location.X + 1, location.Location.Y), location.Keys);
                yield return new(new(location.Location.X, location.Location.Y - 1), location.Keys);
                yield return new(new(location.Location.X, location.Location.Y + 1), location.Keys);

                char tile = _tiles[location.Location.X, location.Location.Y];
                if (KeyContainer.IsKey(tile) && !location.Keys.HasKey(tile))
                {
                    yield return new(location.Location, location.Keys.WithKey(tile));
                }
            }

            public int EstimateDistance(DoorSpace a, DoorSpace b)
            {
                return a.Location.OrthogonalDistance(b.Location) + Math.Abs(b.Keys.Count - a.Keys.Count) * 1000;
            }

            public void CalculateStep(DoorSpace @from, DoorSpace to, KeyContainer currentState, out int cost, out KeyContainer newState)
            {
                cost = from.Location.OrthogonalDistance(to.Location);
                char tile = _tiles[to.Location.X, to.Location.Y];
                if (char.IsLower(tile) && !currentState.HasKey(tile))
                {
                    // We are moving into a space with a key, that's good!
                    newState = currentState.WithKey(tile);
                }
                else
                {
                    newState = currentState;
                }
            }

            public bool IsAt(DoorSpace loc, DoorSpace target)
            {
                if (loc.Equals(target)) return true;

                // All spaces that have all the keys are the same
                if (loc.Keys.Count == KeyLocations.Count && target.Keys.Count == KeyLocations.Count)
                    return true;

                return false;
            }
        }

        public struct MultiDoorSpace : IEquatable<MultiDoorSpace>
        {
            public MultiDoorSpace(ImmutableArray<IntCo2> robots)
            {
                Robots = robots;
                Keys = default;
            }

            public MultiDoorSpace(params IntCo2[] robots)
            {
                Robots = ImmutableArray.Create(robots);
                Keys = default;
            }

            private MultiDoorSpace(ImmutableArray<IntCo2> robots, KeyContainer keys)
            {
                Robots = robots;
                Keys = keys;
            }

            public ImmutableArray<IntCo2> Robots { get; }
            public KeyContainer Keys { get; }

            public MultiDoorSpace MoveRobot(int iRobot, IntCo2 location)
            {
                return new MultiDoorSpace(Robots.SetItem(iRobot, location), Keys);
            }

            public MultiDoorSpace WithKey(char key)
            {
                return new(Robots, Keys.WithKey(key));
            }

            public bool Equals(MultiDoorSpace other)
            {
                return Robots.SequenceEqual(other.Robots) && Keys.Equals(other.Keys);
            }

            public override bool Equals(object obj)
            {
                return obj is MultiDoorSpace other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int roboCode = 0;
                    foreach (IntCo2 r in Robots)
                    {
                        roboCode = HashCode.Combine(roboCode, r.GetHashCode());
                    }
                    return HashCode.Combine(roboCode, Keys.GetHashCode());
                }
            }

            public static bool operator ==(MultiDoorSpace left, MultiDoorSpace right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(MultiDoorSpace left, MultiDoorSpace right)
            {
                return !left.Equals(right);
            }
        }

        public class MultipleRobotDoorMap : IMap<MultiDoorSpace, KeyContainer>
        {
            private readonly char[,] _tiles;
            public ImmutableDictionary<char, IntCo2> DoorLocations { get; }
            public ImmutableDictionary<char, IntCo2> KeyLocations { get; }

            public MultipleRobotDoorMap(char[,] tiles)
            {
                _tiles = tiles;
                var db = ImmutableDictionary.CreateBuilder<char, IntCo2>();
                var kb = ImmutableDictionary.CreateBuilder<char, IntCo2>();
                for (int x = 0; x <= tiles.GetUpperBound(0); x++)
                {
                    for (int y = 0; y <= tiles.GetUpperBound(1); y++)
                    {
                        char tile = tiles[x, y];
                        if (char.IsUpper(tile))
                        {
                            db.Add(tile, new (x, y));
                        }

                        if (char.IsLower(tile))
                        {
                            kb.Add(tile, new (x, y));
                        }
                    }
                }

                DoorLocations = db.ToImmutable();
                KeyLocations = kb.ToImmutable();
            }

            public bool IsPassable(MultiDoorSpace p, KeyContainer keysHeld)
            {
                if (p.Keys != keysHeld)
                    return false;

                foreach (var robot in p.Robots)
                {
                    var tile = _tiles[robot.X, robot.Y];

                    if (tile == '#')
                    {
                        return false;
                    }

                    if (tile == '.' || char.IsLower(tile))
                    {
                        continue;
                    }

                    if (!keysHeld.HasKey(KeyContainer.KeyForDoor(tile)))
                    {
                        return false;
                    }
                }

                return true;
            }

            public IEnumerable<MultiDoorSpace> GetNeighbors(MultiDoorSpace location)
            {
                for (int i = 0; i < location.Robots.Length; i++)
                {
                    IntCo2 lRobot = location.Robots[i];
                    yield return location.MoveRobot(i, new(lRobot.X - 1, lRobot.Y));
                    yield return location.MoveRobot(i, new(lRobot.X + 1, lRobot.Y));
                    yield return location.MoveRobot(i, new(lRobot.X, lRobot.Y - 1));
                    yield return location.MoveRobot(i, new(lRobot.X, lRobot.Y + 1));

                    char tile = _tiles[lRobot.X, lRobot.Y];
                    if (KeyContainer.IsKey(tile) && !location.Keys.HasKey(tile))
                    {
                        yield return location.WithKey(tile);
                    }
                }

            }

            public int EstimateDistance(MultiDoorSpace a, MultiDoorSpace b)
            {
                int distance = RobotDistance(a, b);

                return distance + Math.Abs(b.Keys.Count - a.Keys.Count) * 1000;
            }

            private static int RobotDistance(MultiDoorSpace a, MultiDoorSpace b)
            {
                int distance = 0;
                for (int index = 0; index < a.Robots.Length; index++)
                {
                    distance += a.Robots[index].OrthogonalDistance(b.Robots[index]);
                }

                return distance;
            }

            public void CalculateStep(MultiDoorSpace @from, MultiDoorSpace to, KeyContainer currentState, out int cost, out KeyContainer newState)
            {
                cost = RobotDistance(from, to);
                newState = currentState;
                foreach (IntCo2 location in to.Robots)
                {
                    char tile = _tiles[location.X, location.Y];
                    if (char.IsLower(tile) && !currentState.HasKey(tile))
                    {
                        // We are moving into a space with a key, that's good!
                        newState = newState.WithKey(tile);
                    }
                }
            }

            public bool IsAt(MultiDoorSpace loc, MultiDoorSpace target)
            {
                if (loc.Equals(target)) return true;

                // All spaces that have all the keys are the same
                if (loc.Keys.Count == KeyLocations.Count && target.Keys.Count == KeyLocations.Count)
                    return true;

                return false;
            }
        }
        
        public static async Task Problem1()
        {
            var data = await Data.GetDataLines();
            char[,] tiles = new char[data[0].Length, data.Length];
            IntCo2 center = new (0, 0);
            KeyContainer allTheKey = new ();
            tiles.ForEach(
                (x, y) =>
                {
                    char c = data[y][x];
                    if (c == '@')
                    {
                        center = new (x, y);
                        c = '.';
                    }

                    if (char.IsLower(c))
                        allTheKey = allTheKey.WithKey(c);

                    tiles[x, y] = c;
                });

            MapWithDoors map = new MapWithDoors(tiles);
            
            Stopwatch searchTime = Stopwatch.StartNew();
            var path = AStar.CalculateNewPath(
                map,
                new(center, new KeyContainer()),
                new(new(0, 0), allTheKey),
                out int cost,
                estimated: true
            );

            searchTime.Stop();

            ShowWalk(path, tiles);
            
            Console.WriteLine($"Length : {cost}");
            D.WriteLine($"Search time : {searchTime.Elapsed}");
        }

        public static async Task Problem2()
        {
            var data = await Data.GetDataLines();
            char[,] tiles = new char[data[0].Length, data.Length];
            IntCo2 center = new (0, 0);
            tiles.ForEach(
                (x, y) =>
                {
                    char c = data[y][x];
                    if (c == '@')
                    {
                        center = new (x, y);
                        c = '.';
                    }

                    tiles[x, y] = c;
                });
            
            tiles[center.X, center.Y] = '#';
            tiles[center.X - 1, center.Y] = '#';
            tiles[center.X + 1, center.Y] = '#';
            tiles[center.X, center.Y - 1] = '#';
            tiles[center.X, center.Y + 1] = '#';

            MultipleRobotDoorMap map = new MultipleRobotDoorMap(tiles);
            
            Stopwatch searchTime = Stopwatch.StartNew();
            var start = new MultiDoorSpace(
                new IntCo2(center.X - 1, center.Y - 1),
                new IntCo2(center.X - 1, center.Y + 1),
                new IntCo2(center.X + 1, center.Y + 1),
                new IntCo2(center.X + 1, center.Y - 1)
            );

            var end = map.KeyLocations.Aggregate(start, (r, k) => r.WithKey(k.Key));
            var path = AStar.CalculateNewPath(
                map,
                start,
                end,
                out int cost,
                estimated: true
            );
            searchTime.Stop();

            ShowWalk(path, tiles);

            D.WriteLine($"Path (length={path.Count}) : {string.Join(',', path)}");
            Console.WriteLine($"Length : {cost}");
            D.WriteLine($"Search time : {searchTime.Elapsed}");
        }

        [Conditional("DEBUG")]
        private static void ShowWalk(List<DoorSpace> path, char[,] tiles)
        {
            for (int index = 0; index < path.Count; index++)
            {
                Console.SetCursorPosition(0, 0);
                var p = path[index];
                if (index != 0)
                {
                    var pp = path[index - 1];
                    tiles[pp.Location.X, pp.Location.Y] = '.';
                }

                tiles[p.Location.X, p.Location.Y] = '@';

                for (int y = 0; y <= tiles.GetUpperBound(1); y++)
                {
                    for (int x = 0; x <= tiles.GetUpperBound(0); x++)
                    {
                        Console.Write(tiles[x, y]);
                    }

                    Console.WriteLine();
                }

                Console.WriteLine($"Keys: {p.Keys}");
                Console.WriteLine($"Step: {index}");

                Thread.Sleep(100);
            }
        }

        [Conditional("DEBUG")]
        private static void ShowWalk(List<MultiDoorSpace> path, char[,] tiles)
        {
            for (int y = 0; y <= tiles.GetUpperBound(1); y++)
            {
                for (int x = 0; x <= tiles.GetUpperBound(0); x++)
                {
                    Console.Write(tiles[x, y]);
                }

                Console.WriteLine();
            }

            for (int index = 0; index < path.Count; index++)
            {
                Console.SetCursorPosition(0, 0);
                var p = path[index];
                if (index != 0)
                {
                    var pp = path[index - 1];
                    foreach (IntCo2 robot in pp.Robots)
                    {
                        tiles[robot.X, robot.Y] = '.';
                    }
                }
                
                foreach (IntCo2 robot in p.Robots)
                {
                    tiles[robot.X, robot.Y] = '@';
                }

                for (int y = 0; y <= tiles.GetUpperBound(1); y++)
                {
                    for (int x = 0; x <= tiles.GetUpperBound(0); x++)
                    {
                        Console.Write(tiles[x, y]);
                    }

                    Console.WriteLine();
                }

                Console.WriteLine($"Keys: {p.Keys}");
                Console.WriteLine($"Step: {index}");

                Thread.Sleep(100);
            }
        }

        private class SearchNode : IComparable<SearchNode>, IComparable
        {
            public int Length { get; }
            public IntCo2 Current { get; }
            public ImmutableList<char> KeyOrder { get; }
            public MapWithDoors Map { get; }
            public (int,int,int) Signature { get; }
            public int Heuristic { get; }

            public SearchNode(int length, IntCo2 current, MapWithDoors map, ImmutableList<char> keyOrder)
            {
                Length = length;
                Current = current;
                Map = map;
                KeyOrder = keyOrder;
                Signature = (current.X, current.Y, keyOrder.Aggregate(0, (agg, c) => agg + (1 << (c - 'a'))));
                Heuristic = Length - keyOrder.Count * 1000;
            }

            public int CompareTo(SearchNode other)
            {
                return Heuristic.CompareTo(other.Heuristic);
            }

            public int CompareTo(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return 1;
                }

                return obj is SearchNode other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(SearchNode)}");
            }

            public static bool operator <(SearchNode left, SearchNode right)
            {
                return left.CompareTo(right) < 0;
            }

            public static bool operator >(SearchNode left, SearchNode right)
            {
                return left.CompareTo(right) > 0;
            }

            public static bool operator <=(SearchNode left, SearchNode right)
            {
                return left.CompareTo(right) <= 0;
            }

            public static bool operator >=(SearchNode left, SearchNode right)
            {
                return left.CompareTo(right) >= 0;
            }
        }
    }

    public static class ArrayUtil
    {
        public static void ForEach<T>(this T[,] array, Action<int, int> callback)
        {
            int mx = array.GetUpperBound(0);
            int my = array.GetUpperBound(1);
            for (int x = 0; x <= mx; x++)
            {
                for (int y = 0; y <= my; y++)
                {
                    callback(x, y);
                }
            }
        }
        
        public static void ForEach<T>(this T[,] array, Action<T> callback)
        {
            int mx = array.GetUpperBound(0);
            int my = array.GetUpperBound(1);
            for (int x = 0; x <= mx; x++)
            {
                for (int y = 0; y <= my; y++)
                {
                    callback(array[x, y]);
                }
            }
        }

        public static void ForEach<T>(this T[,] array, Action<int,int,T> callback)
        {
            int mx = array.GetUpperBound(0);
            int my = array.GetUpperBound(1);
            for (int x = 0; x <= mx; x++)
            {
                for (int y = 0; y <= my; y++)
                {
                    callback(x, y, array[x, y]);
                }
            }
        }
    }
}