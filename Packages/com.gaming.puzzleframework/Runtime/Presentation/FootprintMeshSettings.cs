using System;
using PuzzleFramework.CoreBoard;
using UnityEngine;

namespace PuzzleFramework.Presentation
{
    /// <summary>Controls whether generated edge surfaces share or receive their own submesh.</summary>
    public enum FootprintMeshSurfaceLayout
    {
        /// <summary>Publishes every generated triangle through submesh zero.</summary>
        Unified = 0,

        /// <summary>
        /// Publishes caps and flat walls through submesh zero and bevel/corner surfaces through
        /// submesh one so a caller can assign separate presentation materials.
        /// </summary>
        SeparateEdgeSurfaces = 1
    }

    /// <summary>
    /// Generic local-space geometry settings for one footprint mesh. The footprint plane is XY
    /// and extrusion is centered around local Z zero.
    /// </summary>
    public readonly struct FootprintMeshSettings : IEquatable<FootprintMeshSettings>
    {
        public FootprintMeshSettings(
            Vector2 cellSize,
            float extrusionDepth,
            float bevelWidth,
            int bevelSegments,
            GridCellAnchor cellAnchor,
            float cornerBevelWidth = 0f,
            FootprintMeshSurfaceLayout surfaceLayout = FootprintMeshSurfaceLayout.Unified)
        {
            CellSize = cellSize;
            ExtrusionDepth = extrusionDepth;
            BevelWidth = bevelWidth;
            BevelSegments = bevelSegments;
            CellAnchor = cellAnchor;
            CornerBevelWidth = cornerBevelWidth;
            SurfaceLayout = surfaceLayout;
        }

        public Vector2 CellSize { get; }
        public float ExtrusionDepth { get; }
        public float BevelWidth { get; }
        public int BevelSegments { get; }
        public GridCellAnchor CellAnchor { get; }

        /// <summary>Plan-view distance removed from each contour turn to form a vertical chamfer.</summary>
        public float CornerBevelWidth { get; }

        /// <summary>Controls whether edge geometry shares the main submesh.</summary>
        public FootprintMeshSurfaceLayout SurfaceLayout { get; }

        public bool Equals(FootprintMeshSettings other) =>
            CellSize.Equals(other.CellSize) &&
            ExtrusionDepth.Equals(other.ExtrusionDepth) &&
            BevelWidth.Equals(other.BevelWidth) &&
            BevelSegments == other.BevelSegments &&
            CellAnchor == other.CellAnchor &&
            CornerBevelWidth.Equals(other.CornerBevelWidth) &&
            SurfaceLayout == other.SurfaceLayout;

        public override bool Equals(object obj) =>
            obj is FootprintMeshSettings other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(
            CellSize, ExtrusionDepth, BevelWidth, BevelSegments, CellAnchor,
            CornerBevelWidth, SurfaceLayout);

        public static bool operator ==(FootprintMeshSettings left, FootprintMeshSettings right) =>
            left.Equals(right);

        public static bool operator !=(FootprintMeshSettings left, FootprintMeshSettings right) =>
            !left.Equals(right);
    }

    /// <summary>Success/failure result plus stable mesh-complexity diagnostics.</summary>
    public readonly struct FootprintMeshGenerationResult
    {
        private FootprintMeshGenerationResult(
            bool success,
            Mesh mesh,
            string failureReason,
            int contourVertexCount,
            int ringCount)
        {
            Success = success;
            Mesh = mesh;
            FailureReason = failureReason ?? string.Empty;
            ContourVertexCount = contourVertexCount;
            RingCount = ringCount;
        }

        public bool Success { get; }
        public Mesh Mesh { get; }
        public string FailureReason { get; }
        public int ContourVertexCount { get; }
        public int RingCount { get; }
        public int VertexCount => Mesh == null ? 0 : Mesh.vertexCount;
        public int TriangleCount => Mesh == null ? 0 : Mesh.triangles.Length / 3;

        /// <summary>True when submesh one contains the generated bevel and corner surfaces.</summary>
        public bool HasSeparateEdgeSurfaces => Mesh != null && Mesh.subMeshCount == 2;

        internal static FootprintMeshGenerationResult Successful(
            Mesh mesh,
            int contourVertexCount,
            int ringCount) =>
            new(true, mesh, string.Empty, contourVertexCount, ringCount);

        internal static FootprintMeshGenerationResult Failed(string reason) =>
            new(false, null, reason, 0, 0);
    }
}
