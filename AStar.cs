using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Medallion.Collections;

namespace AdventOfCode
{
    public static class AStar
    {
        private class NodeComparer<TCoordinateSystem, TState> : IComparer<SearchNode<TCoordinateSystem, TState>>
        {
            private readonly IMap<TCoordinateSystem, TState> _map;
            private readonly TCoordinateSystem _target;

            public NodeComparer(IMap<TCoordinateSystem, TState> map, TCoordinateSystem target)
            {
                _map = map;
                _target = target;
            }

            public int Compare(SearchNode<TCoordinateSystem, TState> x, SearchNode<TCoordinateSystem, TState> y)
            {
                if (ReferenceEquals(x, y))
                {
                    return 0;
                }

                if (ReferenceEquals(null, y))
                {
                    return 1;
                }

                if (ReferenceEquals(null, x))
                {
                    return -1;
                }

                return ((IComparable) GetEstimatedLength(x)).CompareTo(GetEstimatedLength(y));
            }

            private int GetEstimatedLength(SearchNode<TCoordinateSystem, TState> n)
            {
                return n.Cost + _map.EstimateDistance(n.Location, _target);
            }
        }

        private static SearchNode<TCoordinateSystem, TState> Search<TCoordinateSystem, TState>(
            IMap<TCoordinateSystem, TState> map,
            TCoordinateSystem start,
            TCoordinateSystem target,
            bool estimated = false,
            int lengthLimit = int.MaxValue)
        {
            PriorityQueue<SearchNode<TCoordinateSystem, TState>> open = new PriorityQueue<SearchNode<TCoordinateSystem, TState>>(new NodeComparer<TCoordinateSystem, TState>(map, target));
            Dictionary<TCoordinateSystem, SearchNode<TCoordinateSystem, TState>> allNodes =
                new Dictionary<TCoordinateSystem, SearchNode<TCoordinateSystem, TState>>();

            var rootNode = new SearchNode<TCoordinateSystem, TState>(start);
            open.Enqueue(rootNode);
            allNodes[start] = (rootNode);

            SearchNode<TCoordinateSystem, TState> targetNode = null;

            while (open.Count != 0)
            {
                SearchNode<TCoordinateSystem, TState> searchNode = open.Dequeue();
                searchNode.IsOpen = false;

                if (searchNode.Cost >= lengthLimit)
                {
                    continue;
                }

                SearchNode<TCoordinateSystem, TState> TrySearch(TCoordinateSystem loc)
                {
                    if (!map.IsPassable(loc, searchNode.State))
                    {
                        return null;
                    }

                    map.CalculateStep(searchNode.Location, loc, searchNode.State, out int cost, out TState newState);

                    int newCost = searchNode.Cost + cost;

                    if (map.IsAt(loc, target))
                    {
                        if (targetNode == null)
                        {
                            targetNode = new SearchNode<TCoordinateSystem, TState>(loc, searchNode, cost, newState);
                            if (!estimated)
                                return targetNode;
                        }
                        else if (targetNode.Cost > newCost)
                        {
                            targetNode.UpdateParent(searchNode, cost, newState);
                        }
                    }
                    else
                    {
                        if (allNodes.TryGetValue(loc, out var existing))
                        {
                            {
                                // We already have an open node
                                if (existing.Cost > newCost)
                                {
                                    // Our path is better, so update it, but leave it open
                                    existing.UpdateParent(searchNode, cost, newState);
                                    if (existing.IsOpen)
                                    {
                                        var x = open.Remove(existing);
                                        open.Add(existing);
                                    }
                                    else
                                    {
                                        open.Add(existing);
                                        existing.IsOpen = false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            // First time we've been here, add a new guy
                            var nextNode = new SearchNode<TCoordinateSystem, TState>(loc, searchNode, cost, newState);
                            open.Add(nextNode);
                            allNodes[loc] = nextNode;
                        }
                    }

                    return null;
                }

                foreach (TCoordinateSystem n in map.GetNeighbors(searchNode.Location))
                {
                    SearchNode<TCoordinateSystem, TState> foundTarget = TrySearch(n);

                    if (foundTarget != null)
                    {
                        return foundTarget;
                    }
                }
            }

            return targetNode;
        }

        public static List<TCoordinateSystem> CalculateNewPath<TCoordinateSystem, TState>(
            IMap<TCoordinateSystem, TState> map,
            TCoordinateSystem start,
            TCoordinateSystem target,
            bool estimated = false,
            int lengthLimit = int.MaxValue)
        {
            return CalculateNewPath(map, start, target, out _, estimated, lengthLimit);
        }

        public static List<TCoordinateSystem> CalculateNewPath<TCoordinateSystem, TState>(
            IMap<TCoordinateSystem, TState> map,
            TCoordinateSystem start,
            TCoordinateSystem target,
            out int cost,
            bool estimated = false,
            int lengthLimit = int.MaxValue)
        {
            var foundTarget = Search(map, start, target, estimated, lengthLimit);
            if (foundTarget == null)
            {
                cost = 0;
                return null;
            }

            cost = foundTarget.Cost;

            var reversePath = new List<TCoordinateSystem>();
            while (foundTarget != null)
            {
                reversePath.Insert(0, foundTarget.Location);
                foundTarget = foundTarget.BestParent;
            }
            return reversePath;
        }

        public static int? CalculateNewPathCost<TCoordinateSystem, TState>(
            IMap<TCoordinateSystem, TState> map,
            TCoordinateSystem start,
            TCoordinateSystem target,
            bool estimated = false,
            int lengthLimit = int.MaxValue)
        {
            var foundTarget = Search(map, start, target, estimated, lengthLimit);

            return foundTarget?.Cost + 1;
        }

        private class SearchNode<TCoordinateSystem, TState>
        {
            public SearchNode(TCoordinateSystem location)
            {
                Location = location;
            }

            public SearchNode(TCoordinateSystem location, SearchNode<TCoordinateSystem, TState> parent, int cost, TState state)
            {
                Location = location;
                State = state;
                Cost = parent.Cost + cost;
                State = state;
                BestParent = parent;
            }

            public TCoordinateSystem Location { get; }

            public TState State { get; private set; }
            public SearchNode<TCoordinateSystem, TState> BestParent { get; private set; }
            public int Cost { get; private set; }
            public bool IsOpen { get; set; } = true;

            public void UpdateParent(SearchNode<TCoordinateSystem, TState> parent, int cost, TState state)
            {
                Cost = parent.Cost + cost;
                State = state;
                BestParent = parent;
            }
        }
    }
}