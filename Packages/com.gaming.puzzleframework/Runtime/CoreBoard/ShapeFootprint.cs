using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PuzzleFramework.CoreBoard
{
    /// <summary>
    /// Shared multi-cell footprint definition expressed as offsets relative to an origin cell.
    /// This owns structural shape data only and does not decide how the footprint is used.
    /// </summary>
    public sealed class ShapeFootprint
    {
        private static readonly GridCoordinate[] DefaultSingleCellOffsets =
        {
            new GridCoordinate(0, 0)
        };

        /// <summary>
        /// Reusable single-cell footprint for callers that do not need a multi-cell shape.
        /// </summary>
        public static ShapeFootprint SingleCell { get; } =
            new ShapeFootprint(DefaultSingleCellOffsets);

        public ShapeFootprint(IReadOnlyList<GridCoordinate> offsets)
        {
            if (offsets == null)
            {
                throw new ArgumentNullException(nameof(offsets));
            }

            if (offsets.Count == 0)
            {
                throw new ArgumentException(
                    "Shape footprint must contain at least one offset.",
                    nameof(offsets));
            }

            HashSet<GridCoordinate> visitedOffsets = new();
            List<GridCoordinate> normalizedOffsets = new(offsets.Count);

            for (int i = 0; i < offsets.Count; i++)
            {
                GridCoordinate offset = offsets[i];
                if (!visitedOffsets.Add(offset))
                {
                    throw new ArgumentException(
                        $"Shape footprint contains duplicate offset {offset}.",
                        nameof(offsets));
                }

                normalizedOffsets.Add(offset);
            }

            Offsets = new ReadOnlyCollection<GridCoordinate>(normalizedOffsets);
        }

        /// <summary>
        /// Structural offsets relative to a caller-owned origin cell.
        /// </summary>
        public IReadOnlyList<GridCoordinate> Offsets { get; }

        /// <summary>
        /// Number of cells occupied by the footprint.
        /// </summary>
        public int CellCount => Offsets.Count;

        /// <summary>
        /// Resolves the footprint into board coordinates at the provided origin cell.
        /// </summary>
        public IReadOnlyList<GridCoordinate> ResolveCoordinates(GridCoordinate origin)
        {
            List<GridCoordinate> resolvedCoordinates = new(Offsets.Count);

            for (int i = 0; i < Offsets.Count; i++)
            {
                resolvedCoordinates.Add(origin.Offset(Offsets[i].X, Offsets[i].Y));
            }

            return resolvedCoordinates;
        }
    }
}
