using PuzzleFramework.CoreBoard;
using UnityEngine;

namespace PuzzleFramework.Interaction
{
    /// <summary>
    /// Framework-safe snap result returned by the Grid Snap System.
    /// This reports snap validity and the resolved board-aligned preview position.
    /// </summary>
    public readonly struct GridSnapResult
    {
        public GridSnapResult(
            bool isValid,
            GridCoordinate? resolvedOriginCell,
            Vector3 snappedWorldPosition,
            string failureReason)
        {
            IsValid = isValid;
            ResolvedOriginCell = resolvedOriginCell;
            SnappedWorldPosition = snappedWorldPosition;
            FailureReason = failureReason ?? string.Empty;
        }

        /// <summary>
        /// True when the snap resolves to a framework-valid placement.
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// Resolved origin cell after snap evaluation, when one is available.
        /// </summary>
        public GridCoordinate? ResolvedOriginCell { get; }

        /// <summary>
        /// World position that corresponds to the resolved snap result.
        /// </summary>
        public Vector3 SnappedWorldPosition { get; }

        /// <summary>
        /// Failure explanation when <see cref="IsValid"/> is false.
        /// </summary>
        public string FailureReason { get; }

        public static GridSnapResult Valid(
            GridCoordinate? resolvedOriginCell,
            Vector3 snappedWorldPosition)
        {
            return new GridSnapResult(true, resolvedOriginCell, snappedWorldPosition, string.Empty);
        }

        public static GridSnapResult Invalid(
            GridCoordinate? resolvedOriginCell,
            Vector3 snappedWorldPosition,
            string failureReason)
        {
            return new GridSnapResult(
                false,
                resolvedOriginCell,
                snappedWorldPosition,
                failureReason);
        }
    }
}
