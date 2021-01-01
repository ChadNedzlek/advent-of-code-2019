using System.Collections;
using System.Collections.Generic;

namespace AdventOfCode
{
    public interface IMap<TCoordinateSystem, TState>
    {
        bool IsPassable(TCoordinateSystem location, TState sate);
        IEnumerable<TCoordinateSystem> GetNeighbors(TCoordinateSystem location);
        int EstimateDistance(TCoordinateSystem a, TCoordinateSystem b);
        void CalculateStep(TCoordinateSystem from, TCoordinateSystem to, TState currentState, out int cost, out TState newState);
        bool IsAt(TCoordinateSystem loc, TCoordinateSystem target);
    }
}