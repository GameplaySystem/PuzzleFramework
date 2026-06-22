using System;
using System.Collections.Generic;
using PuzzleFramework.CoreBoard;
using UnityEngine;

namespace PuzzleFramework.Interaction
{
    /// <summary>
    /// Framework-safe drag preview query data consumed by the Drag Movement System.
    /// This stays generic and does not depend on a puzzle-specific object type.
    /// </summary>
    public readonly struct DragMovementRequest
    {
        public DragMovementRequest(
            Vector3 pointerWorldPosition,
            Vector3 originalWorldPosition,
            Vector3 currentWorldPosition,
            GridCoordinate? originalGridPosition,
            GridCoordinate? currentGridPosition,
            IReadOnlyList<GridCoordinate> footprintOffsets,
            GridWorldLayout worldLayout)
        {
            PointerWorldPosition = pointerWorldPosition;
            OriginalWorldPosition = originalWorldPosition;
            CurrentWorldPosition = currentWorldPosition;
            OriginalGridPosition = originalGridPosition;
            CurrentGridPosition = currentGridPosition;
            FootprintOffsets = footprintOffsets ?? Array.Empty<GridCoordinate>();
            WorldLayout = worldLayout;
        }

        /// <summary>
        /// Current pointer world position driving the preview query.
        /// </summary>
        public Vector3 PointerWorldPosition { get; }

        /// <summary>
        /// World position where the drag began.
        /// </summary>
        public Vector3 OriginalWorldPosition { get; }

        /// <summary>
        /// Current world position of the dragged target before this preview update.
        /// </summary>
        public Vector3 CurrentWorldPosition { get; }

        /// <summary>
        /// Original grid coordinate where the drag began, when grid-aware movement is active.
        /// </summary>
        public GridCoordinate? OriginalGridPosition { get; }

        /// <summary>
        /// Current grid coordinate of the dragged target before this preview update, when available.
        /// </summary>
        public GridCoordinate? CurrentGridPosition { get; }

        /// <summary>
        /// Relative footprint offsets used when drag preview must consider multi-cell occupancy.
        /// Empty for single-cell drag.
        /// </summary>
        public IReadOnlyList<GridCoordinate> FootprintOffsets { get; }

        /// <summary>
        /// Shared board world-layout values used for grid-aware drag conversion.
        /// </summary>
        public GridWorldLayout WorldLayout { get; }
    }
}
