using System;
using System.Collections.Generic;
using PuzzleFramework.CoreBoard;

namespace PuzzleFramework.Interaction
{
    /// <summary>
    /// Structural reason a swept footprint cannot occupy a contacted cell.
    /// Puzzle-specific enterability is deliberately outside this result.
    /// </summary>
    public enum FootprintClearanceFailure
    {
        None,
        OutOfBounds,
        MissingCell,
        Blocked,
        Reserved,
        Occupied
    }

    /// <summary>One rule-free clearance decision for an ordered contact group.</summary>
    public readonly struct FootprintClearanceResult
    {
        public FootprintClearanceResult(
            FootprintClearanceFailure failure,
            GridCoordinate? blockingCoordinate)
        {
            Failure = failure;
            BlockingCoordinate = blockingCoordinate;
        }

        public FootprintClearanceFailure Failure { get; }
        public GridCoordinate? BlockingCoordinate { get; }
        public bool IsClear => Failure == FootprintClearanceFailure.None;

        public static FootprintClearanceResult Clear =>
            new(FootprintClearanceFailure.None, null);
    }

    /// <summary>
    /// Checks board structure and occupancy for continuous footprint contacts.
    /// The caller supplies its actual committed cells so it does not collide with itself.
    /// This query neither moves occupancy nor interprets game-owned occupants.
    /// </summary>
    public sealed class FootprintClearanceQuery
    {
        private readonly GridBoard _board;
        private readonly CellOccupancySystem _occupancy;
        private readonly HashSet<GridCoordinate> _selfCells;

        public FootprintClearanceQuery(
            GridBoard board,
            CellOccupancySystem occupancy,
            IEnumerable<GridCoordinate> selfCells)
        {
            _board = board ?? throw new ArgumentNullException(nameof(board));
            _occupancy = occupancy ?? throw new ArgumentNullException(nameof(occupancy));
            if (!ReferenceEquals(_occupancy.GridBoard, _board))
            {
                throw new ArgumentException("Occupancy and clearance must use the same board.", nameof(occupancy));
            }

            _selfCells = selfCells != null
                ? new HashSet<GridCoordinate>(selfCells)
                : new HashSet<GridCoordinate>();
        }

        public FootprintClearanceResult Evaluate(IReadOnlyList<GridCoordinate> contactedCells)
        {
            if (contactedCells == null)
            {
                throw new ArgumentNullException(nameof(contactedCells));
            }

            for (int i = 0; i < contactedCells.Count; i++)
            {
                GridCoordinate coordinate = contactedCells[i];
                if (!_board.IsWithinBounds(coordinate))
                {
                    return new FootprintClearanceResult(FootprintClearanceFailure.OutOfBounds, coordinate);
                }

                if (!_board.ContainsCell(coordinate))
                {
                    return new FootprintClearanceResult(FootprintClearanceFailure.MissingCell, coordinate);
                }

                if (_board.IsBlocked(coordinate))
                {
                    return new FootprintClearanceResult(FootprintClearanceFailure.Blocked, coordinate);
                }

                if (!_selfCells.Contains(coordinate) && _occupancy.IsReserved(coordinate))
                {
                    return new FootprintClearanceResult(FootprintClearanceFailure.Reserved, coordinate);
                }

                if (!_selfCells.Contains(coordinate) && _occupancy.IsOccupied(coordinate))
                {
                    return new FootprintClearanceResult(FootprintClearanceFailure.Occupied, coordinate);
                }
            }

            return FootprintClearanceResult.Clear;
        }
    }
}
