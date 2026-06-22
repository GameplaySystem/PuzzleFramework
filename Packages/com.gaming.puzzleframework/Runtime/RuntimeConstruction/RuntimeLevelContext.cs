using System;
using PuzzleFramework.CoreBoard;

namespace PuzzleFramework.RuntimeConstruction
{
    /// <summary>
    /// Shared runtime handoff context produced by level construction.
    /// This groups framework-owned runtime state and does not carry gameplay-specific meaning.
    /// </summary>
    public sealed class RuntimeLevelContext
    {
        public RuntimeLevelContext(
            RuntimeConstructionBoardData boardData,
            GridBoard gridBoard,
            CellOccupancySystem cellOccupancySystem)
        {
            BoardData = boardData ?? throw new ArgumentNullException(nameof(boardData));
            GridBoard = gridBoard ?? throw new ArgumentNullException(nameof(gridBoard));
            CellOccupancySystem = cellOccupancySystem ??
                                  throw new ArgumentNullException(nameof(cellOccupancySystem));
        }

        /// <summary>
        /// Validated shared board construction data used to build the runtime board state.
        /// </summary>
        public RuntimeConstructionBoardData BoardData { get; }

        /// <summary>
        /// Runtime structural grid for the loaded level.
        /// Blocked structural cells are exposed through the grid's cell metadata.
        /// </summary>
        public GridBoard GridBoard { get; }

        /// <summary>
        /// Runtime occupancy state layered on top of the structural grid.
        /// </summary>
        public CellOccupancySystem CellOccupancySystem { get; }
    }
}
