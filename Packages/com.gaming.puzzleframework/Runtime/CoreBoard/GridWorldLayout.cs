using System;
using UnityEngine;

namespace PuzzleFramework.CoreBoard
{
    /// <summary>Meaning of the integer grid position in world space.</summary>
    public enum GridCellAnchor
    {
        Center = 0,
        Corner = 1
    }

    /// <summary>
    /// Shared board world-layout data used by interaction systems to convert
    /// between board coordinates and world positions.
    /// This is layout data only and does not define puzzle meaning.
    /// </summary>
    [Serializable]
    public readonly struct GridWorldLayout
    {
        /// <summary>
        /// Creates a layout whose logical rectangular board bounds are centered on the requested
        /// world position. The returned <see cref="BoardOrigin"/> remains the center of cell (0,0).
        /// </summary>
        public static GridWorldLayout CreateCentered(
            Vector3 boardCenter,
            int boardWidth,
            int boardHeight,
            Vector2 cellSize,
            Vector3 boardXAxis,
            Vector3 boardYAxis)
        {
            return CreateCentered(boardCenter, boardWidth, boardHeight, cellSize,
                boardXAxis, boardYAxis, GridCellAnchor.Center);
        }

        /// <summary>Centers the full rectangular board while preserving the requested cell anchor.</summary>
        public static GridWorldLayout CreateCentered(
            Vector3 boardCenter, int boardWidth, int boardHeight, Vector2 cellSize,
            Vector3 boardXAxis, Vector3 boardYAxis, GridCellAnchor cellAnchor)
        {
            if (boardWidth <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(boardWidth),
                    "Centered grid world layout board width must be positive.");
            }

            if (boardHeight <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(boardHeight),
                    "Centered grid world layout board height must be positive.");
            }

            GridWorldLayout centerAnchoredLayout = new(
                boardCenter,
                cellSize,
                boardXAxis,
                boardYAxis,
                cellAnchor);
            float firstCellOffset = cellAnchor == GridCellAnchor.Center ? 0.5f : 0f;
            Vector3 boardOrigin =
                boardCenter -
                (centerAnchoredLayout.BoardXAxis * ((boardWidth * 0.5f - firstCellOffset) * cellSize.x)) -
                (centerAnchoredLayout.BoardYAxis * ((boardHeight * 0.5f - firstCellOffset) * cellSize.y));

            return new GridWorldLayout(
                boardOrigin,
                cellSize,
                centerAnchoredLayout.BoardXAxis,
                centerAnchoredLayout.BoardYAxis,
                cellAnchor);
        }

        public GridWorldLayout(Vector3 boardOrigin, Vector2 cellSize)
            : this(boardOrigin, cellSize, Vector3.right, Vector3.up)
        {
        }

        public GridWorldLayout(
            Vector3 boardOrigin,
            Vector2 cellSize,
            Vector3 boardXAxis,
            Vector3 boardYAxis)
            : this(boardOrigin, cellSize, boardXAxis, boardYAxis, GridCellAnchor.Center)
        {
        }

        /// <summary>Creates a layout with an explicit cell-center or cell-corner origin.</summary>
        public GridWorldLayout(
            Vector3 boardOrigin, Vector2 cellSize, Vector3 boardXAxis,
            Vector3 boardYAxis, GridCellAnchor cellAnchor)
        {
            if (!Enum.IsDefined(typeof(GridCellAnchor), cellAnchor))
                throw new ArgumentOutOfRangeException(nameof(cellAnchor));
            if (cellSize.x <= 0f || cellSize.y <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(cellSize),
                    "Grid world layout cell size must be positive on both axes.");
            }

            if (boardXAxis.sqrMagnitude <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(boardXAxis),
                    "Grid world layout board X axis must be non-zero.");
            }

            if (boardYAxis.sqrMagnitude <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(boardYAxis),
                    "Grid world layout board Y axis must be non-zero.");
            }

            Vector3 normalizedBoardXAxis = boardXAxis.normalized;
            Vector3 normalizedBoardYAxis = boardYAxis.normalized;
            if (Mathf.Abs(Vector3.Dot(normalizedBoardXAxis, normalizedBoardYAxis)) > 0.0001f)
            {
                throw new ArgumentException(
                    "Grid world layout board axes must be orthogonal.",
                    nameof(boardYAxis));
            }

            BoardOrigin = boardOrigin;
            CellSize = cellSize;
            BoardXAxis = normalizedBoardXAxis;
            BoardYAxis = normalizedBoardYAxis;
            CellAnchor = cellAnchor;
        }

        /// <summary>
        /// World-space origin used as the anchor for board coordinate conversion.
        /// </summary>
        public Vector3 BoardOrigin { get; }

        /// <summary>Whether an integer coordinate denotes a cell center or its minimum corner.</summary>
        public GridCellAnchor CellAnchor { get; }

        /// <summary>
        /// World-space cell size used for grid-aligned conversion.
        /// </summary>
        public Vector2 CellSize { get; }

        /// <summary>
        /// Normalized world-space direction used by increasing grid X coordinates.
        /// </summary>
        public Vector3 BoardXAxis { get; }

        /// <summary>
        /// Normalized world-space direction used by increasing grid Y coordinates.
        /// This is a board-local axis, not necessarily Unity's world Y axis.
        /// </summary>
        public Vector3 BoardYAxis { get; }

        /// <summary>
        /// Converts a world-space position into continuous board-local cell units.
        /// Components perpendicular to the configured board axes do not affect the result.
        /// </summary>
        public Vector2 WorldToBoardLocal(Vector3 worldPosition)
        {
            Vector3 relativePosition = worldPosition - BoardOrigin;
            return new Vector2(
                Vector3.Dot(relativePosition, BoardXAxis) / CellSize.x,
                Vector3.Dot(relativePosition, BoardYAxis) / CellSize.y);
        }

        /// <summary>
        /// Converts a world-space position to the nearest grid coordinate using board-local axes.
        /// </summary>
        public GridCoordinate WorldToNearestGridCoordinate(Vector3 worldPosition)
        {
            Vector2 boardLocalPosition = WorldToBoardLocal(worldPosition);
            return new GridCoordinate(
                Mathf.RoundToInt(boardLocalPosition.x),
                Mathf.RoundToInt(boardLocalPosition.y));
        }

        /// <summary>Returns the cell containing the world position under this layout's anchor convention.</summary>
        public GridCoordinate WorldToCellCoordinate(Vector3 worldPosition)
        {
            Vector2 local = WorldToBoardLocal(worldPosition);
            return new GridCoordinate(
                CellAnchor == GridCellAnchor.Center ? Mathf.FloorToInt(local.x + 0.5f) : Mathf.FloorToInt(local.x),
                CellAnchor == GridCellAnchor.Center ? Mathf.FloorToInt(local.y + 0.5f) : Mathf.FloorToInt(local.y));
        }

        /// <summary>World-space center of a cell, independent of the selected origin convention.</summary>
        public Vector3 CellCenterToWorld(GridCoordinate coordinate)
        {
            float offset = CellAnchor == GridCellAnchor.Corner ? 0.5f : 0f;
            return BoardLocalToWorld(new Vector2(coordinate.X + offset, coordinate.Y + offset));
        }

        /// <summary>
        /// Converts continuous board-local cell units into world space.
        /// </summary>
        public Vector3 BoardLocalToWorld(Vector2 boardLocalPosition)
        {
            return BoardOrigin +
                   (BoardXAxis * (boardLocalPosition.x * CellSize.x)) +
                   (BoardYAxis * (boardLocalPosition.y * CellSize.y));
        }

        /// <summary>
        /// Converts a grid coordinate to world space using board-local axes.
        /// </summary>
        public Vector3 GridToWorldPosition(GridCoordinate coordinate)
        {
            return BoardLocalToWorld(new Vector2(coordinate.X, coordinate.Y));
        }
    }
}
