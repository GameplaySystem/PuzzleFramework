using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PuzzleFramework.CoreBoard
{
    /// <summary>
    /// Minimal runtime grid foundation for board dimensions, coordinate validation,
    /// cell lookup, and orthogonal neighbor queries.
    /// </summary>
    public sealed class GridBoard
    {
        private static readonly GridCoordinate[] NeighborOffsets =
        {
            new(0, 1),
            new(0, -1),
            new(-1, 0),
            new(1, 0)
        };

        private readonly Dictionary<GridCoordinate, GridCell> _cellsByCoordinate;

        public GridBoard(int width, int height, IEnumerable<GridCoordinate> cellCoordinates)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "Grid width must be positive.");
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height), "Grid height must be positive.");
            }

            if (cellCoordinates == null)
            {
                throw new ArgumentNullException(nameof(cellCoordinates));
            }

            Width = width;
            Height = height;
            _cellsByCoordinate = BuildCellLookup(cellCoordinates);
            Cells = new ReadOnlyCollection<GridCell>(new List<GridCell>(_cellsByCoordinate.Values));
        }

        public int Width { get; }
        public int Height { get; }

        /// <summary>
        /// Existing structural cells registered in the grid.
        /// Sparse layouts are allowed even when a coordinate remains within bounds.
        /// </summary>
        public IReadOnlyList<GridCell> Cells { get; }

        /// <summary>
        /// Returns true when the coordinate lies inside the declared board dimensions.
        /// </summary>
        public bool IsWithinBounds(GridCoordinate coordinate)
        {
            return coordinate.X >= 0 &&
                   coordinate.X < Width &&
                   coordinate.Y >= 0 &&
                   coordinate.Y < Height;
        }

        /// <summary>
        /// Returns true when the coordinate maps to a registered structural cell.
        /// </summary>
        public bool ContainsCell(GridCoordinate coordinate)
        {
            return _cellsByCoordinate.ContainsKey(coordinate);
        }

        /// <summary>
        /// Attempts to resolve the structural cell registered at the coordinate.
        /// </summary>
        public bool TryGetCell(GridCoordinate coordinate, out GridCell cell)
        {
            return _cellsByCoordinate.TryGetValue(coordinate, out cell);
        }

        /// <summary>
        /// Returns orthogonally adjacent registered cells.
        /// This stays at structural adjacency only and does not decide traversability.
        /// </summary>
        public IReadOnlyList<GridCell> GetNeighbors(GridCoordinate coordinate)
        {
            List<GridCell> neighbors = new();

            for (int i = 0; i < NeighborOffsets.Length; i++)
            {
                GridCoordinate neighborCoordinate = coordinate.Offset(
                    NeighborOffsets[i].X,
                    NeighborOffsets[i].Y);

                if (!IsWithinBounds(neighborCoordinate))
                {
                    continue;
                }

                if (_cellsByCoordinate.TryGetValue(neighborCoordinate, out GridCell neighbor))
                {
                    neighbors.Add(neighbor);
                }
            }

            return neighbors;
        }

        private Dictionary<GridCoordinate, GridCell> BuildCellLookup(
            IEnumerable<GridCoordinate> cellCoordinates)
        {
            Dictionary<GridCoordinate, GridCell> cellsByCoordinate = new();

            foreach (GridCoordinate coordinate in cellCoordinates)
            {
                if (!IsWithinBounds(coordinate))
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(cellCoordinates),
                        $"Grid cell coordinate {coordinate} is outside the declared board dimensions.");
                }

                // Duplicate structural coordinates are rejected here because this is grid-owned
                // board-shape validation, not occupancy or gameplay validation.
                if (!cellsByCoordinate.TryAdd(coordinate, new GridCell(coordinate)))
                {
                    throw new ArgumentException(
                        $"Grid contains duplicate cell coordinate {coordinate}.",
                        nameof(cellCoordinates));
                }
            }

            return cellsByCoordinate;
        }
    }
}
