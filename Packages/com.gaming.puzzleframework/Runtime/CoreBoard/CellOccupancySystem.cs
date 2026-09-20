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

        /// <summary>
        /// Atomically replaces one occupied footprint with another after validating every cell.
        /// The caller must supply the actual source footprint and separately decide whether
        /// blocked cells, movement and puzzle rules permit the destination. No entity IDs are
        /// stored here. On failure, both occupancy sets remain unchanged.
        /// </summary>
        public CellOccupancyOperationResult TransferFootprint(
            IEnumerable<GridCoordinate> sourceCoordinates,
            IEnumerable<GridCoordinate> destinationCoordinates)
        {
            if (!TryCollectUnique(sourceCoordinates, "Source", out HashSet<GridCoordinate> source, out string failure))
            {
                return CellOccupancyOperationResult.Failed(failure);
            }

            if (!TryCollectUnique(destinationCoordinates, "Destination", out HashSet<GridCoordinate> destination, out failure))
            {
                return CellOccupancyOperationResult.Failed(failure);
            }

            foreach (GridCoordinate coordinate in source)
            {
                if (!TryValidateCoordinate(coordinate, out failure))
                {
                    return CellOccupancyOperationResult.Failed(failure);
                }

                if (!_occupiedCoordinates.Contains(coordinate))
                {
                    return CellOccupancyOperationResult.Failed($"Source coordinate {coordinate} is not occupied.");
                }
            }

            foreach (GridCoordinate coordinate in destination)
            {
                if (!TryValidateCoordinate(coordinate, out failure))
                {
                    return CellOccupancyOperationResult.Failed(failure);
                }

                if (_reservedCoordinates.Contains(coordinate))
                {
                    return CellOccupancyOperationResult.Failed($"Destination coordinate {coordinate} is reserved.");
                }

                if (_occupiedCoordinates.Contains(coordinate) && !source.Contains(coordinate))
                {
                    return CellOccupancyOperationResult.Failed($"Destination coordinate {coordinate} is occupied.");
                }
            }

            // Hash-set mutations cannot fail after complete validation; overlap stays occupied.
            foreach (GridCoordinate coordinate in source)
            {
                if (!destination.Contains(coordinate))
                {
                    _occupiedCoordinates.Remove(coordinate);
                }
            }

            foreach (GridCoordinate coordinate in destination)
            {
                _occupiedCoordinates.Add(coordinate);
            }

            return CellOccupancyOperationResult.Successful();
        }

        private static bool TryCollectUnique(
            IEnumerable<GridCoordinate> coordinates,
            string label,
            out HashSet<GridCoordinate> collected,
            out string failure)
        {
            collected = new HashSet<GridCoordinate>();
            if (coordinates == null)
            {
                failure = $"{label} footprint is required.";
                return false;
            }

            foreach (GridCoordinate coordinate in coordinates)
            {
                if (!collected.Add(coordinate))
                {
                    failure = $"{label} footprint contains duplicate coordinate {coordinate}.";
                    return false;
                }
            }

            if (collected.Count == 0)
            {
                failure = $"{label} footprint must contain at least one coordinate.";
                return false;
            }

            failure = string.Empty;
            return true;
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
