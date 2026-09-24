using System;
using System.Collections.Generic;
using PuzzleFramework.CoreBoard;
using UnityEngine;
using UnityEngine.Rendering;

namespace PuzzleFramework.Presentation
{
    /// <summary>
    /// Converts one simple connected grid footprint into a unified extruded presentation mesh.
    /// Gameplay, materials, colors, colliders, and entity meaning remain caller-owned.
    /// </summary>
    public sealed class FootprintMeshGenerator
    {
        private const float GeometryEpsilon = 0.00001f;
        private readonly WallGenerationSystem _wallGeneration = new();

        public FootprintMeshGenerationResult Generate(
            ShapeFootprint footprint,
            FootprintMeshSettings settings)
        {
            if (footprint == null)
                return FootprintMeshGenerationResult.Failed("A shape footprint is required.");
            if (!TryValidateSettings(settings, out string failure))
                return FootprintMeshGenerationResult.Failed(failure);

            WallGenerationResult boundary;
            try
            {
                boundary = _wallGeneration.Generate(footprint.Offsets);
            }
            catch (Exception exception)
            {
                return FootprintMeshGenerationResult.Failed(
                    $"Footprint boundary generation failed: {exception.Message}");
            }

            if (boundary.HasDiagonalTouch)
            {
                return FootprintMeshGenerationResult.Failed(
                    "Footprint mesh generation does not support diagonal-touch boundary vertices.");
            }

            if (!TryBuildSingleContour(boundary.ExposedEdges, out List<Vector2> latticeContour,
                    out failure))
                return FootprintMeshGenerationResult.Failed(failure);

            List<Vector2> contour = ScaleAndAnchor(latticeContour, settings);
            if (!IsCounterClockwise(contour) || !IsSimplePolygon(contour))
            {
                return FootprintMeshGenerationResult.Failed(
                    "Footprint exterior contour is not a simple counter-clockwise polygon.");
            }

            if (!TryBuildRings(contour, settings, out List<ContourRing> rings, out failure))
                return FootprintMeshGenerationResult.Failed(failure);

            List<Vector2> capContour = rings[0].Points;
            if (!TryTriangulate(capContour, out List<int> capTriangles, out failure))
                return FootprintMeshGenerationResult.Failed(failure);

            Mesh mesh = BuildMesh(rings, capTriangles, settings);
            return FootprintMeshGenerationResult.Successful(
                mesh, contour.Count, rings.Count);
        }

        private static bool TryValidateSettings(
            FootprintMeshSettings settings,
            out string failure)
        {
            if (!Enum.IsDefined(typeof(GridCellAnchor), settings.CellAnchor))
                return Fail("Footprint mesh cell anchor is invalid.", out failure);
            if (!IsFinitePositive(settings.CellSize.x) ||
                !IsFinitePositive(settings.CellSize.y))
                return Fail("Footprint mesh cell size must be finite and positive.", out failure);
            if (!IsFinitePositive(settings.ExtrusionDepth))
                return Fail("Footprint mesh extrusion depth must be finite and positive.", out failure);
            if (!IsFinite(settings.BevelWidth) || settings.BevelWidth < 0f)
                return Fail("Footprint mesh bevel width must be finite and non-negative.", out failure);
            if (settings.BevelSegments <= 0)
                return Fail("Footprint mesh bevel segment count must be positive.", out failure);
            if (settings.BevelWidth * 2f >= Mathf.Min(settings.CellSize.x, settings.CellSize.y) -
                GeometryEpsilon)
                return Fail("Bevel width must stay below half of the smallest cell dimension.", out failure);
            if (settings.BevelWidth * 2f >= settings.ExtrusionDepth - GeometryEpsilon)
                return Fail("Bevel width must stay below half of the extrusion depth.", out failure);

            failure = string.Empty;
            return true;
        }

        private static bool TryBuildSingleContour(
            IReadOnlyList<BoardBoundaryEdge> edges,
            out List<Vector2> contour,
            out string failure)
        {
            contour = null;
            if (edges == null || edges.Count < 4)
                return Fail("Footprint does not have a usable exterior boundary.", out failure);

            Dictionary<GridVertexCoordinate, GridVertexCoordinate> nextByStart = new();
            Dictionary<GridVertexCoordinate, int> incomingCounts = new();
            for (int i = 0; i < edges.Count; i++)
            {
                ResolveCounterClockwiseEdge(edges[i], out GridVertexCoordinate start,
                    out GridVertexCoordinate end);
                if (!nextByStart.TryAdd(start, end))
                    return Fail($"Footprint boundary branches at lattice vertex {start}.", out failure);
                incomingCounts.TryGetValue(end, out int count);
                incomingCounts[end] = count + 1;
            }

            foreach (KeyValuePair<GridVertexCoordinate, GridVertexCoordinate> pair in nextByStart)
            {
                if (!incomingCounts.TryGetValue(pair.Key, out int incoming) || incoming != 1)
                    return Fail($"Footprint boundary is open or ambiguous at {pair.Key}.", out failure);
            }

            GridVertexCoordinate first = FindFirstVertex(nextByStart.Keys);
            GridVertexCoordinate current = first;
            HashSet<GridVertexCoordinate> visitedStarts = new();
            List<GridVertexCoordinate> lattice = new();

            while (visitedStarts.Add(current))
            {
                lattice.Add(current);
                if (!nextByStart.TryGetValue(current, out current))
                    return Fail("Footprint boundary could not be closed.", out failure);
                if (current == first) break;
            }

            if (current != first || visitedStarts.Count != edges.Count)
            {
                return Fail(
                    "Footprint mesh generation requires one connected, hole-free exterior contour.",
                    out failure);
            }

            RemoveCollinearVertices(lattice);
            if (lattice.Count < 3)
                return Fail("Footprint contour collapsed below three vertices.", out failure);

            contour = new List<Vector2>(lattice.Count);
            for (int i = 0; i < lattice.Count; i++)
                contour.Add(new Vector2(lattice[i].X, lattice[i].Y));
            failure = string.Empty;
            return true;
        }

        private static void ResolveCounterClockwiseEdge(
            BoardBoundaryEdge edge,
            out GridVertexCoordinate start,
            out GridVertexCoordinate end)
        {
            int x = edge.CellCoordinate.X;
            int y = edge.CellCoordinate.Y;
            switch (edge.Direction)
            {
                case BoardEdgeDirection.South:
                    start = new GridVertexCoordinate(x, y);
                    end = new GridVertexCoordinate(x + 1, y);
                    return;
                case BoardEdgeDirection.East:
                    start = new GridVertexCoordinate(x + 1, y);
                    end = new GridVertexCoordinate(x + 1, y + 1);
                    return;
                case BoardEdgeDirection.North:
                    start = new GridVertexCoordinate(x + 1, y + 1);
                    end = new GridVertexCoordinate(x, y + 1);
                    return;
                case BoardEdgeDirection.West:
                    start = new GridVertexCoordinate(x, y + 1);
                    end = new GridVertexCoordinate(x, y);
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(edge.Direction));
            }
        }

        private static GridVertexCoordinate FindFirstVertex(
            IEnumerable<GridVertexCoordinate> vertices)
        {
            bool hasValue = false;
            GridVertexCoordinate first = default;
            foreach (GridVertexCoordinate vertex in vertices)
            {
                if (!hasValue || vertex.Y < first.Y ||
                    (vertex.Y == first.Y && vertex.X < first.X))
                {
                    first = vertex;
                    hasValue = true;
                }
            }
            return first;
        }

        private static void RemoveCollinearVertices(List<GridVertexCoordinate> vertices)
        {
            bool removed;
            do
            {
                removed = false;
                for (int i = 0; i < vertices.Count && vertices.Count > 3; i++)
                {
                    GridVertexCoordinate previous = vertices[(i - 1 + vertices.Count) % vertices.Count];
                    GridVertexCoordinate current = vertices[i];
                    GridVertexCoordinate next = vertices[(i + 1) % vertices.Count];
                    int cross = (current.X - previous.X) * (next.Y - current.Y) -
                                (current.Y - previous.Y) * (next.X - current.X);
                    if (cross != 0) continue;
                    vertices.RemoveAt(i);
                    removed = true;
                    break;
                }
            } while (removed);
        }

        private static List<Vector2> ScaleAndAnchor(
            IReadOnlyList<Vector2> latticeContour,
            FootprintMeshSettings settings)
        {
            float anchorOffset = settings.CellAnchor == GridCellAnchor.Center ? -0.5f : 0f;
            List<Vector2> result = new(latticeContour.Count);
            for (int i = 0; i < latticeContour.Count; i++)
            {
                result.Add(new Vector2(
                    (latticeContour[i].x + anchorOffset) * settings.CellSize.x,
                    (latticeContour[i].y + anchorOffset) * settings.CellSize.y));
            }
            return result;
        }

        private static bool TryBuildRings(
            List<Vector2> contour,
            FootprintMeshSettings settings,
            out List<ContourRing> rings,
            out string failure)
        {
            rings = new List<ContourRing>();
            float halfDepth = settings.ExtrusionDepth * 0.5f;
            if (settings.BevelWidth <= GeometryEpsilon)
            {
                rings.Add(new ContourRing(new List<Vector2>(contour), -halfDepth));
                rings.Add(new ContourRing(new List<Vector2>(contour), halfDepth));
                failure = string.Empty;
                return true;
            }

            float bevel = settings.BevelWidth;
            int segments = settings.BevelSegments;

            for (int index = segments; index >= 0; index--)
            {
                float angle = index / (float)segments * Mathf.PI * 0.5f;
                float inset = bevel * (1f - Mathf.Cos(angle));
                float z = -halfDepth + bevel - bevel * Mathf.Sin(angle);
                if (!TryOffsetContour(contour, inset, out List<Vector2> points, out failure))
                    return false;
                rings.Add(new ContourRing(points, z));
            }

            rings.Add(new ContourRing(new List<Vector2>(contour), halfDepth - bevel));
            for (int index = 1; index <= segments; index++)
            {
                float angle = index / (float)segments * Mathf.PI * 0.5f;
                float inset = bevel * (1f - Mathf.Cos(angle));
                float z = halfDepth - bevel + bevel * Mathf.Sin(angle);
                if (!TryOffsetContour(contour, inset, out List<Vector2> points, out failure))
                    return false;
                rings.Add(new ContourRing(points, z));
            }

            failure = string.Empty;
            return true;
        }

        private static bool TryOffsetContour(
            IReadOnlyList<Vector2> contour,
            float distance,
            out List<Vector2> offset,
            out string failure)
        {
            offset = new List<Vector2>(contour.Count);
            if (distance <= GeometryEpsilon)
            {
                offset.AddRange(contour);
                failure = string.Empty;
                return true;
            }

            for (int i = 0; i < contour.Count; i++)
            {
                Vector2 previous = contour[(i - 1 + contour.Count) % contour.Count];
                Vector2 current = contour[i];
                Vector2 next = contour[(i + 1) % contour.Count];
                Vector2 incoming = (current - previous).normalized;
                Vector2 outgoing = (next - current).normalized;
                Vector2 incomingNormal = new(-incoming.y, incoming.x);
                Vector2 outgoingNormal = new(-outgoing.y, outgoing.x);
                Vector2 firstLinePoint = current + incomingNormal * distance;
                Vector2 secondLinePoint = current + outgoingNormal * distance;
                float denominator = Cross(incoming, outgoing);
                if (Mathf.Abs(denominator) <= GeometryEpsilon)
                    return Fail("Footprint contour contains a non-offsettable corner.", out failure);
                float alongIncoming = Cross(secondLinePoint - firstLinePoint, outgoing) / denominator;
                offset.Add(firstLinePoint + incoming * alongIncoming);
            }

            if (!IsCounterClockwise(offset) || !IsSimplePolygon(offset) ||
                Mathf.Abs(SignedArea(offset)) <= GeometryEpsilon)
            {
                return Fail(
                    "Bevel inset collapses or self-intersects the footprint contour.",
                    out failure);
            }

            failure = string.Empty;
            return true;
        }

        private static bool TryTriangulate(
            IReadOnlyList<Vector2> polygon,
            out List<int> triangles,
            out string failure)
        {
            triangles = new List<int>((polygon.Count - 2) * 3);
            List<int> remaining = new(polygon.Count);
            for (int i = 0; i < polygon.Count; i++) remaining.Add(i);

            int guard = polygon.Count * polygon.Count;
            while (remaining.Count > 3 && guard-- > 0)
            {
                bool clipped = false;
                for (int i = 0; i < remaining.Count; i++)
                {
                    int previous = remaining[(i - 1 + remaining.Count) % remaining.Count];
                    int current = remaining[i];
                    int next = remaining[(i + 1) % remaining.Count];
                    if (Cross(polygon[current] - polygon[previous],
                            polygon[next] - polygon[current]) <= GeometryEpsilon)
                        continue;

                    bool containsPoint = false;
                    for (int candidateIndex = 0; candidateIndex < remaining.Count; candidateIndex++)
                    {
                        int candidate = remaining[candidateIndex];
                        if (candidate == previous || candidate == current || candidate == next) continue;
                        if (!PointInTriangle(polygon[candidate], polygon[previous],
                                polygon[current], polygon[next])) continue;
                        containsPoint = true;
                        break;
                    }
                    if (containsPoint) continue;

                    triangles.Add(previous);
                    triangles.Add(current);
                    triangles.Add(next);
                    remaining.RemoveAt(i);
                    clipped = true;
                    break;
                }

                if (!clipped)
                    return Fail("Footprint cap polygon could not be triangulated.", out failure);
            }

            if (remaining.Count != 3)
                return Fail("Footprint cap triangulation did not finish.", out failure);
            triangles.Add(remaining[0]);
            triangles.Add(remaining[1]);
            triangles.Add(remaining[2]);
            failure = string.Empty;
            return true;
        }

        private static Mesh BuildMesh(
            IReadOnlyList<ContourRing> rings,
            IReadOnlyList<int> capTriangles,
            FootprintMeshSettings settings)
        {
            int contourCount = rings[0].Points.Count;
            int ringVertexCount = contourCount * rings.Count;
            List<Vector3> vertices = new(ringVertexCount + contourCount * 2);
            List<Vector2> uvs = new(vertices.Capacity);
            List<int> triangles = new(
                (rings.Count - 1) * contourCount * 6 + capTriangles.Count * 2);

            for (int ringIndex = 0; ringIndex < rings.Count; ringIndex++)
            {
                ContourRing ring = rings[ringIndex];
                for (int i = 0; i < contourCount; i++)
                {
                    Vector2 point = ring.Points[i];
                    vertices.Add(new Vector3(point.x, point.y, ring.Z));
                    uvs.Add(new Vector2(point.x / settings.CellSize.x,
                        point.y / settings.CellSize.y));
                }
            }

            for (int ringIndex = 0; ringIndex < rings.Count - 1; ringIndex++)
            {
                int lower = ringIndex * contourCount;
                int upper = (ringIndex + 1) * contourCount;
                for (int i = 0; i < contourCount; i++)
                {
                    int next = (i + 1) % contourCount;
                    triangles.Add(lower + i);
                    triangles.Add(lower + next);
                    triangles.Add(upper + next);
                    triangles.Add(lower + i);
                    triangles.Add(upper + next);
                    triangles.Add(upper + i);
                }
            }

            int bottomStart = vertices.Count;
            AddCapVertices(rings[0], settings, vertices, uvs);
            int topStart = vertices.Count;
            AddCapVertices(rings[rings.Count - 1], settings, vertices, uvs);
            for (int i = 0; i < capTriangles.Count; i += 3)
            {
                int a = capTriangles[i];
                int b = capTriangles[i + 1];
                int c = capTriangles[i + 2];
                triangles.Add(bottomStart + a);
                triangles.Add(bottomStart + c);
                triangles.Add(bottomStart + b);
                triangles.Add(topStart + a);
                triangles.Add(topStart + b);
                triangles.Add(topStart + c);
            }

            Mesh mesh = new()
            {
                name = $"Footprint Mesh ({contourCount} contour vertices)",
                indexFormat = vertices.Count > ushort.MaxValue
                    ? IndexFormat.UInt32
                    : IndexFormat.UInt16
            };
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0, true);
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void AddCapVertices(
            ContourRing ring,
            FootprintMeshSettings settings,
            ICollection<Vector3> vertices,
            ICollection<Vector2> uvs)
        {
            for (int i = 0; i < ring.Points.Count; i++)
            {
                Vector2 point = ring.Points[i];
                vertices.Add(new Vector3(point.x, point.y, ring.Z));
                uvs.Add(new Vector2(point.x / settings.CellSize.x,
                    point.y / settings.CellSize.y));
            }
        }

        private static bool IsSimplePolygon(IReadOnlyList<Vector2> polygon)
        {
            for (int first = 0; first < polygon.Count; first++)
            {
                int firstNext = (first + 1) % polygon.Count;
                for (int second = first + 1; second < polygon.Count; second++)
                {
                    int secondNext = (second + 1) % polygon.Count;
                    if (first == second || firstNext == second || secondNext == first) continue;
                    if (SegmentsIntersect(polygon[first], polygon[firstNext],
                            polygon[second], polygon[secondNext])) return false;
                }
            }
            return true;
        }

        private static bool SegmentsIntersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            float abC = Cross(b - a, c - a);
            float abD = Cross(b - a, d - a);
            float cdA = Cross(d - c, a - c);
            float cdB = Cross(d - c, b - c);
            return abC * abD < -GeometryEpsilon && cdA * cdB < -GeometryEpsilon;
        }

        private static bool PointInTriangle(Vector2 point, Vector2 a, Vector2 b, Vector2 c)
        {
            float first = Cross(b - a, point - a);
            float second = Cross(c - b, point - b);
            float third = Cross(a - c, point - c);
            return first >= -GeometryEpsilon && second >= -GeometryEpsilon &&
                   third >= -GeometryEpsilon;
        }

        private static bool IsCounterClockwise(IReadOnlyList<Vector2> polygon) =>
            SignedArea(polygon) > GeometryEpsilon;

        private static float SignedArea(IReadOnlyList<Vector2> polygon)
        {
            float twiceArea = 0f;
            for (int i = 0; i < polygon.Count; i++)
            {
                Vector2 current = polygon[i];
                Vector2 next = polygon[(i + 1) % polygon.Count];
                twiceArea += current.x * next.y - next.x * current.y;
            }
            return twiceArea * 0.5f;
        }

        private static float Cross(Vector2 left, Vector2 right) =>
            left.x * right.y - left.y * right.x;

        private static bool IsFinitePositive(float value) => IsFinite(value) && value > 0f;

        private static bool IsFinite(float value) =>
            !float.IsNaN(value) && !float.IsInfinity(value);

        private static bool Fail(string reason, out string failure)
        {
            failure = reason;
            return false;
        }

        private readonly struct ContourRing
        {
            public ContourRing(List<Vector2> points, float z)
            {
                Points = points;
                Z = z;
            }

            public List<Vector2> Points { get; }
            public float Z { get; }
        }
    }
}
