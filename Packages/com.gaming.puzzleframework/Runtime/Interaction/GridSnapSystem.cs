using System.Collections.Generic;
using PuzzleFramework.CoreBoard;
using UnityEngine;

namespace PuzzleFramework.Interaction
{
    /// <summary>
    /// Minimal concrete snap system for board-aligned placement.
    /// This first slice converts between world and grid space, validates structural placement,
    /// and returns snap data without committing occupancy.
    /// </summary>
    public sealed class GridSnapSystem : IGridSnapSystem
    {
        /// <inheritdoc />
        public GridSnapResult Evaluate(GridSnapRequest request)
        {
            GridCoordinate resolvedOrigin =
                request.WorldLayout.WorldToNearestGridCoordinate(request.WorldPosition);
            Vector3 snappedWorldPosition = request.WorldLayout.GridToWorldPosition(resolvedOrigin);

            if (!TryValidateFootprint(
                    request,
                    resolvedOrigin,
                    out string failureReason))
            {
                return GridSnapResult.Invalid(
                    request.CurrentOriginCell,
                    request.CurrentOriginCell.HasValue
                        ? request.WorldLayout.GridToWorldPosition(request.CurrentOriginCell.Value)
                        : snappedWorldPosition,
                    failureReason);
            }

            return GridSnapResult.Valid(resolvedOrigin, snappedWorldPosition);
        }

        private static bool TryValidateFootprint(
            GridSnapRequest request,
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
                    failureReason = $"Snap footprint contains duplicate coordinate {coordinate}.";
                    return false;
                }

                if (!request.GridBoard.IsWithinBounds(coordinate))
                {
                    failureReason =
                        $"Snap target coordinate {coordinate} is outside the declared board dimensions.";
                    return false;
                }

                if (!request.GridBoard.ContainsCell(coordinate))
                {
                    failureReason =
                        $"Snap target coordinate {coordinate} does not map to a registered structural cell.";
                    return false;
                }

                if (request.GridBoard.IsBlocked(coordinate))
                {
                    failureReason =
                        $"Snap target coordinate {coordinate} is structurally blocked.";
                    return false;
                }

                if (request.CellOccupancySystem.IsReserved(coordinate) &&
                    !currentFootprint.Contains(coordinate))
                {
                    failureReason =
                        $"Snap target coordinate {coordinate} is currently reserved.";
                    return false;
                }

                if (request.CellOccupancySystem.IsOccupied(coordinate) &&
                    !currentFootprint.Contains(coordinate))
                {
                    failureReason =
                        $"Snap target coordinate {coordinate} is already occupied.";
                    return false;
                }
            }

            failureReason = string.Empty;
            return true;
        }

        private static HashSet<GridCoordinate> BuildCurrentFootprint(GridSnapRequest request)
        {
            HashSet<GridCoordinate> currentFootprint = new();
            if (!request.CurrentOriginCell.HasValue)
            {
                return currentFootprint;
            }

            IReadOnlyList<GridCoordinate> offsets = request.FootprintOffsets.Count > 0
                ? request.FootprintOffsets
                : SingleCellOffsets;

            for (int i = 0; i < offsets.Count; i++)
            {
                currentFootprint.Add(request.CurrentOriginCell.Value.Offset(offsets[i].X, offsets[i].Y));
            }

            return currentFootprint;
        }

        private static readonly GridCoordinate[] SingleCellOffsets =
        {
            new GridCoordinate(0, 0)
        };
    }
}
