namespace PuzzleFramework.Interaction
{
    /// <summary>
    /// Evaluates framework-level board-aligned placement and returns snap data.
    /// This boundary owns alignment and validation only and does not commit occupancy by default.
    /// </summary>
    public interface IGridSnapSystem
    {
        /// <summary>
        /// Evaluates a snap request and returns the snap result data.
        /// </summary>
        GridSnapResult Evaluate(GridSnapRequest request);
    }
}
