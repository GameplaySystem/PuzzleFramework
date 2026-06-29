using System.Collections.Generic;
using PuzzleFramework.CoreBoard;
using UnityEngine;

namespace PuzzleFramework.Interaction
{
    /// <summary>
    /// Minimal concrete drag system for grid-constrained movement preview.
    /// This first slice converts between world and grid space, validates structural movement,
    /// and blocks invalid movement at the current valid position without committing occupancy.
    /// </summary>
    public sealed class DragMovementSystem : IDragMovementSystem
    {
        /// <inheritdoc />
        public DragMovementResult Evaluate(DragMovementRequest request)
        {
            GridCoordinate resolvedOrigin =
                request.WorldLayout.WorldToNearestGridCoordinate(request.PointerWorldPosition);
            Vector3 previewWorldPosition = request.WorldLayout.GridToWorldPosition(resolvedOrigin);

            if (!TryValidateFootprint(
                    request,
                    resolvedOrigin,
                    out string failureReason))
            {
                return DragMovementResult.Invalid(
                    request.CurrentWorldPosition,
                    request.CurrentGridPosition,
                    failureReason);
            }

            return DragMovementResult.Valid(previewWorldPosition, resolvedOrigin);
        }

        private static bool TryValidateFootprint(
            DragMovementRequest request,
            GridCoordinate resolvedOrigin,
            out string failureReason)
        {
            HashSet<GridCoordinate> currentFootprint = BuildCurrentFootprint(request);
            HashSet<GridCoordinate> resolvedFootprint = new();

            IReadOnlyList<GridCoordinate> offsets = request.FootprintOffsets.Count > 0
                ? request.FootprintOffsets
                : SingleCellOffsets;

            for (int i = 0; i < offsets.Count; i++)
            {
                GridCoordinate coordinate = resolvedOrigin.Offset(offsets[i].X, offsets[i].Y);
                if (!resolvedFootprint.Add(coordinate))
                {
                    failureReason = $"Drag footprint contains duplicate coordinate {coordinate}.";
                    return false;
                }

                if (!request.GridBoard.IsWithinBounds(coordinate))
                {
                    failureReason =
                        $"Drag target coordinate {coordinate} is outside the declared board dimensions.";
                    return false;
                }

                if (!request.GridBoard.ContainsCell(coordinate))
                {
                    failureReason =
                        $"Drag target coordinate {coordinate} does not map to a registered structural cell.";
                    return false;
                }

                if (request.GridBoard.IsBlocked(coordinate))
                {
                    failureReason =
                        $"Drag target coordinate {coordinate} is structurally blocked.";
                    return false;
                }

                if (request.CellOccupancySystem.IsReserved(coordinate) &&
                    !currentFootprint.Contains(coordinate))
                {
                    failureReason =
                        $"Drag target coordinate {coordinate} is currently reserved.";
                    return false;
                }

                if (request.CellOccupancySystem.IsOccupied(coordinate) &&
                    !currentFootprint.Contains(coordinate))
                {
                    failureReason =
                        $"Drag target coordinate {coordinate} is already occupied.";
                    return false;
                }
            }

            failureReason = string.Empty;
            return true;
        }

        private static HashSet<GridCoordinate> BuildCurrentFootprint(DragMovementRequest request)
        {
            HashSet<GridCoordinate> currentFootprint = new();
            if (!request.CurrentGridPosition.HasValue)
            {
                return currentFootprint;
            }

            IReadOnlyList<GridCoordinate> offsets = request.FootprintOffsets.Count > 0
                ? request.FootprintOffsets
                : SingleCellOffsets;

            for (int i = 0; i < offsets.Count; i++)
            {
                currentFootprint.Add(request.CurrentGridPosition.Value.Offset(offsets[i].X, offsets[i].Y));
            }

            return currentFootprint;
        }

        private static readonly GridCoordinate[] SingleCellOffsets =
        {
            new GridCoordinate(0, 0)
        };
    }
}
