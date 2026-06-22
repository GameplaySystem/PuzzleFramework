namespace PuzzleFramework.CoreBoard
{
    /// <summary>
    /// Minimal runtime cell reference used by the Grid System.
    /// The Grid System owns structural lookup and board-level cell metadata only;
    /// usage meaning and gameplay meaning belong elsewhere.
    /// </summary>
    public sealed class GridCell
    {
        public GridCell(GridCoordinate coordinate, bool isBlocked)
        {
            Coordinate = coordinate;
            IsBlocked = isBlocked;
        }

        /// <summary>
        /// Structural board coordinate for this runtime cell.
        /// </summary>
        public GridCoordinate Coordinate { get; }

        /// <summary>
        /// True when the structural cell is authored as blocked.
        /// The Grid System exposes this board metadata but does not decide what blocked means.
        /// </summary>
        public bool IsBlocked { get; }
    }
}
