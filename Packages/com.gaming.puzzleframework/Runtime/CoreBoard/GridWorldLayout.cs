using System;
using UnityEngine;

namespace PuzzleFramework.CoreBoard
{
    /// <summary>
    /// Shared board world-layout data used by interaction systems to convert
    /// between board coordinates and world positions.
    /// This is layout data only and does not define puzzle meaning.
    /// </summary>
    [Serializable]
    public readonly struct GridWorldLayout
    {
        public GridWorldLayout(Vector3 boardOrigin, Vector2 cellSize)
        {
            if (cellSize.x <= 0f || cellSize.y <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(cellSize),
                    "Grid world layout cell size must be positive on both axes.");
            }

            BoardOrigin = boardOrigin;
            CellSize = cellSize;
        }

        /// <summary>
        /// World-space origin used as the anchor for board coordinate conversion.
        /// </summary>
        public Vector3 BoardOrigin { get; }

        /// <summary>
        /// World-space cell size used for grid-aligned conversion.
        /// </summary>
        public Vector2 CellSize { get; }
    }
}
