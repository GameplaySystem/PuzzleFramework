using PuzzleFramework.CoreBoard;

namespace PuzzleFramework.RuntimeConstruction
{
    /// <summary>
    /// Single build-safety validation issue discovered before runtime construction begins.
    /// </summary>
    public readonly struct RuntimeConstructionValidationIssue
    {
        public RuntimeConstructionValidationIssue(
            string code,
            string message,
            GridCoordinate? coordinate)
        {
            Code = code ?? string.Empty;
            Message = message ?? string.Empty;
            Coordinate = coordinate;
        }

        /// <summary>
        /// Stable identifier for the type of build-safety failure.
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Human-readable explanation of the build-safety failure.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Coordinate associated with the issue when the failure is board-location specific.
        /// </summary>
        public GridCoordinate? Coordinate { get; }
    }
}
