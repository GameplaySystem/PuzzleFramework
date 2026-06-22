using System;
using System.Collections.Generic;
using PuzzleFramework.CoreBoard;
using UnityEngine;

namespace PuzzleFramework.Interaction
{
    /// <summary>
    /// Framework-safe snap query data consumed by the Grid Snap System.
    /// This carries only the generic data needed to evaluate board-aligned placement.
    /// </summary>
    public readonly struct GridSnapRequest
    {
        public GridSnapRequest(
            Vector3 worldPosition,
            GridCoordinate? currentOriginCell,
            IReadOnlyList<GridCoordinate> footprintOffsets,
            GridBoard gridBoard,
            CellOccupancySystem cellOccupancySystem)
        {
            WorldPosition = worldPosition;
            CurrentOriginCell = currentOriginCell;
            FootprintOffsets = footprintOffsets ?? Array.Empty<GridCoordinate>();
            GridBoard = gridBoard ?? throw new ArgumentNullException(nameof(gridBoard));
            CellOccupancySystem = cellOccupancySystem ??
                                  throw new ArgumentNullException(nameof(cellOccupancySystem));
        }

        /// <summary>
        /// World position to evaluate for snapping.
        /// </summary>
        public Vector3 WorldPosition { get; }

        /// <summary>
        /// Current origin cell before the snap evaluation, when one exists.
        /// </summary>
        public GridCoordinate? CurrentOriginCell { get; }

        /// <summary>
        /// Relative footprint offsets used for shape-aware snap validation.
        /// Empty for single-cell snap.
        /// </summary>
        public IReadOnlyList<GridCoordinate> FootprintOffsets { get; }

        /// <summary>
        /// Structural board context used for framework-level snap checks.
        /// </summary>
        public GridBoard GridBoard { get; }

        /// <summary>
        /// Occupancy context used for framework-level snap validation.
        /// </summary>
        public CellOccupancySystem CellOccupancySystem { get; }
    }
}
