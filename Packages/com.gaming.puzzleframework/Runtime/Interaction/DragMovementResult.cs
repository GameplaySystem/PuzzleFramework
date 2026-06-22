using PuzzleFramework.CoreBoard;
using UnityEngine;

namespace PuzzleFramework.Interaction
{
    /// <summary>
    /// Framework-safe drag preview result returned by the Drag Movement System.
    /// The preview position may represent the requested position or the last valid position,
    /// depending on the movement policy used by the system implementation.
    /// </summary>
    public readonly struct DragMovementResult
    {
        public DragMovementResult(
            bool isValid,
            Vector3 previewWorldPosition,
            GridCoordinate? previewGridPosition,
            string failureReason)
        {
            IsValid = isValid;
            PreviewWorldPosition = previewWorldPosition;
            PreviewGridPosition = previewGridPosition;
            FailureReason = failureReason ?? string.Empty;
        }

        /// <summary>
        /// True when the requested drag preview is framework-valid.
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// World position that should be previewed after the drag evaluation.
        /// </summary>
        public Vector3 PreviewWorldPosition { get; }

        /// <summary>
        /// Grid coordinate associated with the preview position, when grid-aware movement is active.
        /// </summary>
        public GridCoordinate? PreviewGridPosition { get; }

        /// <summary>
        /// Failure explanation when <see cref="IsValid"/> is false.
        /// </summary>
        public string FailureReason { get; }

        public static DragMovementResult Valid(
            Vector3 previewWorldPosition,
            GridCoordinate? previewGridPosition)
        {
            return new DragMovementResult(true, previewWorldPosition, previewGridPosition, string.Empty);
        }

        public static DragMovementResult Invalid(
            Vector3 previewWorldPosition,
            GridCoordinate? previewGridPosition,
            string failureReason)
        {
            return new DragMovementResult(
                false,
                previewWorldPosition,
                previewGridPosition,
                failureReason);
        }
    }
}
