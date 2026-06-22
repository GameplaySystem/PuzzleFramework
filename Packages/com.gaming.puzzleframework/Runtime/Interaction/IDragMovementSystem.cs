namespace PuzzleFramework.Interaction
{
    /// <summary>
    /// Evaluates framework-level drag preview movement from reusable drag-query data.
    /// This boundary owns preview validation only and does not commit final placement.
    /// </summary>
    public interface IDragMovementSystem
    {
        /// <summary>
        /// Evaluates a drag preview request and returns the preview movement result.
        /// </summary>
        DragMovementResult Evaluate(DragMovementRequest request);
    }
}
