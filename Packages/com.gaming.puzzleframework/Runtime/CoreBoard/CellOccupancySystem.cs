using System;
using System.Collections.Generic;

namespace PuzzleFramework.CoreBoard
{
    /// <summary>
    /// Tracks structural cell usage on top of <see cref="GridBoard"/>.
    /// This layer owns occupied and reserved state only; it does not decide gameplay meaning.
    /// </summary>
    public sealed class CellOccupancySystem
    {
        private readonly HashSet<GridCoordinate> _occupiedCoordinates = new();
        private readonly HashSet<GridCoordinate> _reservedCoordinates = new();

        public CellOccupancySystem(GridBoard gridBoard)
        {
            GridBoard = gridBoard ?? throw new ArgumentNullException(nameof(gridBoard));
        }

        /// <summary>
        /// Grid structure that authoritatively defines which coordinates are valid cells.
        /// </summary>
        public GridBoard GridBoard { get; }

        /// <summary>
        /// Structural coordinates currently marked as occupied.
        /// </summary>
        public IReadOnlyCollection<GridCoordinate> OccupiedCoordinates => _occupiedCoordinates;

        /// <summary>
        /// Structural coordinates currently marked as reserved.
        /// </summary>
        public IReadOnlyCollection<GridCoordinate> ReservedCoordinates => _reservedCoordinates;

        /// <summary>
        /// Returns true when the coordinate is structurally occupied.
        /// Invalid coordinates always return false rather than throwing.
        /// </summary>
        public bool IsOccupied(GridCoordinate coordinate)
        {
            return GridBoard.ContainsCell(coordinate) && _occupiedCoordinates.Contains(coordinate);
        }

        /// <summary>
        /// Returns true when the coordinate is structurally reserved.
        /// Invalid coordinates always return false rather than throwing.
        /// </summary>
        public bool IsReserved(GridCoordinate coordinate)
        {
            return GridBoard.ContainsCell(coordinate) && _reservedCoordinates.Contains(coordinate);
        }

        /// <summary>
        /// Returns true when the coordinate is occupied or reserved.
        /// </summary>
        public bool IsInUse(GridCoordinate coordinate)
        {
            return IsOccupied(coordinate) || IsReserved(coordinate);
        }

        /// <summary>
        /// Marks a coordinate as occupied if it is a valid structural cell and currently free.
        /// </summary>
        public CellOccupancyOperationResult Occupy(GridCoordinate coordinate)
        {
            if (!TryValidateCoordinate(coordinate, out string failureReason))
            {
                return CellOccupancyOperationResult.Failed(failureReason);
            }

            if (_occupiedCoordinates.Contains(coordinate))
            {
                return CellOccupancyOperationResult.Failed(
                    $"Coordinate {coordinate} is already occupied.");
            }

            if (_reservedCoordinates.Contains(coordinate))
            {
                return CellOccupancyOperationResult.Failed(
                    $"Coordinate {coordinate} is reserved and cannot also be occupied.");
            }

            _occupiedCoordinates.Add(coordinate);
            return CellOccupancyOperationResult.Successful();
        }

        /// <summary>
        /// Removes occupied state from a coordinate.
        /// </summary>
        public CellOccupancyOperationResult Release(GridCoordinate coordinate)
        {
            if (!TryValidateCoordinate(coordinate, out string failureReason))
            {
                return CellOccupancyOperationResult.Failed(failureReason);
            }

            if (!_occupiedCoordinates.Remove(coordinate))
            {
                return CellOccupancyOperationResult.Failed(
                    $"Coordinate {coordinate} is not currently occupied.");
            }

            return CellOccupancyOperationResult.Successful();
        }

        /// <summary>
        /// Marks a coordinate as reserved if it is a valid structural cell and currently free.
        /// </summary>
        public CellOccupancyOperationResult Reserve(GridCoordinate coordinate)
        {
            if (!TryValidateCoordinate(coordinate, out string failureReason))
            {
                return CellOccupancyOperationResult.Failed(failureReason);
            }

            if (_reservedCoordinates.Contains(coordinate))
            {
                return CellOccupancyOperationResult.Failed(
                    $"Coordinate {coordinate} is already reserved.");
            }

            if (_occupiedCoordinates.Contains(coordinate))
            {
                return CellOccupancyOperationResult.Failed(
                    $"Coordinate {coordinate} is occupied and cannot be reserved.");
            }

            _reservedCoordinates.Add(coordinate);
            return CellOccupancyOperationResult.Successful();
        }

        /// <summary>
        /// Removes reserved state from a coordinate.
        /// </summary>
        public CellOccupancyOperationResult Unreserve(GridCoordinate coordinate)
        {
            if (!TryValidateCoordinate(coordinate, out string failureReason))
            {
                return CellOccupancyOperationResult.Failed(failureReason);
            }

            if (!_reservedCoordinates.Remove(coordinate))
            {
                return CellOccupancyOperationResult.Failed(
                    $"Coordinate {coordinate} is not currently reserved.");
            }

            return CellOccupancyOperationResult.Successful();
        }

        /// <summary>
        /// Checks whether every coordinate in the footprint maps to a valid structural cell and is free.
        /// This is occupancy validation only and does not decide gameplay legality.
        /// </summary>
        public FootprintAvailabilityResult CanAcceptFootprint(
            IEnumerable<GridCoordinate> footprintCoordinates)
        {
            if (footprintCoordinates == null)
            {
                return FootprintAvailabilityResult.Rejected(
                    null,
                    "Footprint coordinates are required.");
            }

            HashSet<GridCoordinate> visitedCoordinates = new();

            foreach (GridCoordinate coordinate in footprintCoordinates)
            {
                if (!visitedCoordinates.Add(coordinate))
                {
                    return FootprintAvailabilityResult.Rejected(
                        coordinate,
                        $"Footprint contains duplicate coordinate {coordinate}.");
                }

                if (!TryValidateCoordinate(coordinate, out string failureReason))
                {
                    return FootprintAvailabilityResult.Rejected(coordinate, failureReason);
                }

                if (_occupiedCoordinates.Contains(coordinate))
                {
                    return FootprintAvailabilityResult.Rejected(
                        coordinate,
                        $"Coordinate {coordinate} is already occupied.");
                }

                if (_reservedCoordinates.Contains(coordinate))
                {
                    return FootprintAvailabilityResult.Rejected(
                        coordinate,
                        $"Coordinate {coordinate} is currently reserved.");
                }
            }

            return FootprintAvailabilityResult.Accepted();
        }

        private bool TryValidateCoordinate(GridCoordinate coordinate, out string failureReason)
        {
            if (!GridBoard.IsWithinBounds(coordinate))
            {
                failureReason =
                    $"Coordinate {coordinate} is outside the declared board dimensions.";
                return false;
            }

            if (!GridBoard.ContainsCell(coordinate))
            {
                failureReason =
                    $"Coordinate {coordinate} does not map to a registered structural cell.";
                return false;
            }

            failureReason = string.Empty;
            return true;
        }
    }
}
