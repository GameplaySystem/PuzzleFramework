namespace PuzzleFramework.CoreBoard
{
    /// <summary>
    /// Minimal runtime cell reference used by the Grid System.
    /// The Grid System owns structural lookup only; usage meaning belongs elsewhere.
    /// </summary>
    public sealed class GridCell
    {
        public GridCell(GridCoordinate coordinate)
        {
            Coordinate = coordinate;
        }

        public GridCoordinate Coordinate { get; }
    }
}
