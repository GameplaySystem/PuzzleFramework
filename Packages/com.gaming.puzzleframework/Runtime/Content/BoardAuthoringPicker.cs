using System;
using PuzzleFramework.CoreBoard;
using PuzzleFramework.Interaction;
using UnityEngine;

namespace PuzzleFramework.Content
{
    /// <summary>Projects pointer rays onto a board using its explicit cell-anchor convention.</summary>
    public static class BoardAuthoringPicker
    {
        public static bool TryPickCell(Ray ray, GridWorldLayout layout, Vector3 planeOrigin,
            out GridCoordinate coordinate)
        {
            coordinate = default;
            if (!TryProject(ray, layout, planeOrigin, out Vector3 point)) return false;
            coordinate = layout.WorldToCellCoordinate(point);
            return true;
        }

        /// <summary>
        /// Picks an exposed structural edge, including edges of internal gaps. Game rules decide
        /// which of those edges may own authored content.
        /// </summary>
        public static bool TryPickBoundaryEdge(Ray ray, GridWorldLayout layout, Vector3 planeOrigin,
            WallGenerationResult boundary, float maxDistanceInCells, out BoardBoundaryEdge edge)
        {
            if (boundary == null) throw new ArgumentNullException(nameof(boundary));
            if (maxDistanceInCells < 0f || float.IsNaN(maxDistanceInCells))
                throw new ArgumentOutOfRangeException(nameof(maxDistanceInCells));
            edge = default;
            if (!TryProject(ray, layout, planeOrigin, out Vector3 point)) return false;
            Vector2 local = layout.WorldToBoardLocal(point);
            float vertexOffset = layout.CellAnchor == GridCellAnchor.Center ? -0.5f : 0f;
            float bestDistanceSquared = maxDistanceInCells * maxDistanceInCells;
            bool found = false;
            foreach (BoardBoundaryEdge candidate in boundary.ExposedEdges)
            {
                Vector2 start = new(candidate.StartVertex.X + vertexOffset,
                    candidate.StartVertex.Y + vertexOffset);
                Vector2 end = new(candidate.EndVertex.X + vertexOffset,
                    candidate.EndVertex.Y + vertexOffset);
                Vector2 segment = end - start;
                float progress = Mathf.Clamp01(Vector2.Dot(local - start, segment) / segment.sqrMagnitude);
                float distanceSquared = (local - (start + segment * progress)).sqrMagnitude;
                if (distanceSquared > bestDistanceSquared || (found && distanceSquared == bestDistanceSquared))
                    continue;
                bestDistanceSquared = distanceSquared;
                edge = candidate;
                found = true;
            }
            return found;
        }

        private static bool TryProject(Ray ray, GridWorldLayout layout, Vector3 planeOrigin,
            out Vector3 point)
        {
            Vector3 normal = Vector3.Cross(layout.BoardYAxis, layout.BoardXAxis).normalized;
            return BoardPointerProjection.TryProject(ray, planeOrigin, normal, out point);
        }
    }
}
