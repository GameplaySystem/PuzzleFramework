using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using PuzzleFramework.CoreBoard;

namespace PuzzleFramework.RuntimeConstruction
{
    /// <summary>
    /// Validated board-space construction data derived from authored content.
    /// This is a shared handoff contract for runtime construction, not a runtime system itself.
    /// </summary>
    public sealed class RuntimeConstructionBoardData
    {
        public RuntimeConstructionBoardData(
            int width,
            int height,
            IList<GridCoordinate> structuralCoordinates,
            IList<GridCoordinate> blockedCoordinates)
        {
            Width = width;
            Height = height;
            StructuralCoordinates = new ReadOnlyCollection<GridCoordinate>(
                new List<GridCoordinate>(
                    structuralCoordinates ?? throw new ArgumentNullException(nameof(structuralCoordinates))));
            BlockedCoordinates = new ReadOnlyCollection<GridCoordinate>(
                new List<GridCoordinate>(
                    blockedCoordinates ?? throw new ArgumentNullException(nameof(blockedCoordinates))));
        }

        /// <summary>
        /// Declared authored board width.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Declared authored board height.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Structural board coordinates that should participate in runtime grid construction.
        /// Inactive authored cells are intentionally excluded.
        /// </summary>
        public IReadOnlyList<GridCoordinate> StructuralCoordinates { get; }

        /// <summary>
        /// Structural coordinates authored as blocked.
        /// Blocked remains board-state information only and does not become occupancy automatically.
        /// </summary>
        public IReadOnlyList<GridCoordinate> BlockedCoordinates { get; }
    }
}
