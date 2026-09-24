using System;
using PuzzleFramework.CoreBoard;
using UnityEngine;

namespace PuzzleFramework.Presentation
{
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
            GridCellAnchor cellAnchor)
        {
            CellSize = cellSize;
            ExtrusionDepth = extrusionDepth;
            BevelWidth = bevelWidth;
            BevelSegments = bevelSegments;
            CellAnchor = cellAnchor;
        }

        public Vector2 CellSize { get; }
        public float ExtrusionDepth { get; }
        public float BevelWidth { get; }
        public int BevelSegments { get; }
        public GridCellAnchor CellAnchor { get; }

        public bool Equals(FootprintMeshSettings other) =>
            CellSize.Equals(other.CellSize) &&
            ExtrusionDepth.Equals(other.ExtrusionDepth) &&
            BevelWidth.Equals(other.BevelWidth) &&
            BevelSegments == other.BevelSegments &&
            CellAnchor == other.CellAnchor;

        public override bool Equals(object obj) =>
            obj is FootprintMeshSettings other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(CellSize, ExtrusionDepth, BevelWidth, BevelSegments, CellAnchor);

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

        internal static FootprintMeshGenerationResult Successful(
            Mesh mesh,
            int contourVertexCount,
            int ringCount) =>
            new(true, mesh, string.Empty, contourVertexCount, ringCount);

        internal static FootprintMeshGenerationResult Failed(string reason) =>
            new(false, null, reason, 0, 0);
    }
}
