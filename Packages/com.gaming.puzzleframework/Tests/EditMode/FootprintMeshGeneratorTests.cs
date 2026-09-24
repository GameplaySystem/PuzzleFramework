using System.Collections.Generic;
using NUnit.Framework;
using PuzzleFramework.CoreBoard;
using PuzzleFramework.Presentation;
using UnityEngine;

namespace PuzzleFramework.Tests.EditMode
{
    public sealed class FootprintMeshGeneratorTests
    {
        private readonly FootprintMeshGenerator _generator = new();
        private readonly List<Mesh> _meshes = new();

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < _meshes.Count; i++)
                if (_meshes[i] != null) Object.DestroyImmediate(_meshes[i]);
            _meshes.Clear();
        }

        [TestCaseSource(nameof(SupportedFootprints))]
        public void ConnectedFootprints_GenerateOneCoherentReadableMesh(GridCoordinate[] offsets)
        {
            FootprintMeshGenerationResult result = Generate(offsets, Beveled(2));

            Assert.That(result.Success, Is.True, result.FailureReason);
            Assert.That(result.Mesh, Is.Not.Null);
            Assert.That(result.Mesh.subMeshCount, Is.EqualTo(1));
            Assert.That(result.VertexCount, Is.GreaterThan(0));
            Assert.That(result.TriangleCount, Is.GreaterThan(0));
            Assert.That(result.Mesh.normals.Length, Is.EqualTo(result.VertexCount));
            foreach (Vector3 normal in result.Mesh.normals)
                Assert.That(normal.sqrMagnitude, Is.GreaterThan(0.9f));
        }

        [Test]
        public void CornerAndCenterAnchors_ProducePredictablePivotAndOuterDimensions()
        {
            GridCoordinate[] rectangle2x3 =
            {
                new(0, 0), new(1, 0),
                new(0, 1), new(1, 1),
                new(0, 2), new(1, 2)
            };
            FootprintMeshGenerationResult corner = Generate(rectangle2x3,
                new FootprintMeshSettings(new Vector2(1f, 2f), 0.4f, 0.05f, 2,
                    GridCellAnchor.Corner));
            Assert.That(corner.Success, Is.True, corner.FailureReason);
            AssertVector(corner.Mesh.bounds.min, new Vector3(0f, 0f, -0.2f));
            AssertVector(corner.Mesh.bounds.max, new Vector3(2f, 6f, 0.2f));

            FootprintMeshGenerationResult center = Generate(
                new[] { new GridCoordinate(0, 0) },
                new FootprintMeshSettings(Vector2.one, 0.4f, 0.05f, 2,
                    GridCellAnchor.Center));
            Assert.That(center.Success, Is.True, center.FailureReason);
            AssertVector(center.Mesh.bounds.min, new Vector3(-0.5f, -0.5f, -0.2f));
            AssertVector(center.Mesh.bounds.max, new Vector3(0.5f, 0.5f, 0.2f));
        }

        [Test]
        public void AdjacentCells_RemoveSharedBorderAndGenerateNoInternalFaces()
        {
            FootprintMeshGenerationResult result = Generate(
                new[] { new GridCoordinate(0, 0), new GridCoordinate(1, 0) },
                new FootprintMeshSettings(Vector2.one, 0.24f, 0f, 1,
                    GridCellAnchor.Corner));

            Assert.That(result.Success, Is.True, result.FailureReason);
            Assert.That(result.ContourVertexCount, Is.EqualTo(4));
            Assert.That(result.RingCount, Is.EqualTo(2));
            Assert.That(result.VertexCount, Is.EqualTo(16));
            Assert.That(result.TriangleCount, Is.EqualTo(12));
            AssertVector(result.Mesh.bounds.size, new Vector3(2f, 1f, 0.24f));
        }

        [Test]
        public void BevelSegments_IncreaseOnlyPresentationComplexityPredictably()
        {
            GridCoordinate[] square =
            {
                new(0, 0), new(1, 0), new(0, 1), new(1, 1)
            };
            FootprintMeshGenerationResult one = Generate(square, Beveled(1));
            FootprintMeshGenerationResult three = Generate(square, Beveled(3));

            Assert.That(one.Success, Is.True, one.FailureReason);
            Assert.That(three.Success, Is.True, three.FailureReason);
            Assert.That(one.ContourVertexCount, Is.EqualTo(4));
            Assert.That(one.RingCount, Is.EqualTo(4));
            Assert.That(one.VertexCount, Is.EqualTo(24));
            Assert.That(one.TriangleCount, Is.EqualTo(28));
            Assert.That(three.RingCount, Is.EqualTo(8));
            Assert.That(three.VertexCount, Is.EqualTo(40));
            Assert.That(three.TriangleCount, Is.EqualTo(60));
        }

        [Test]
        public void EquivalentOffsetOrdering_ReusesCachedMeshButSettingsDoNot()
        {
            using FootprintMeshCache cache = new();
            ShapeFootprint first = new(new[]
            {
                new GridCoordinate(0, 0), new GridCoordinate(1, 0), new GridCoordinate(0, 1)
            });
            ShapeFootprint reordered = new(new[]
            {
                new GridCoordinate(0, 1), new GridCoordinate(0, 0), new GridCoordinate(1, 0)
            });

            FootprintMeshGenerationResult original = cache.GetOrCreate(first, Beveled(2));
            FootprintMeshGenerationResult equivalent = cache.GetOrCreate(reordered, Beveled(2));
            FootprintMeshGenerationResult different = cache.GetOrCreate(first, Beveled(3));

            Assert.That(original.Success, Is.True, original.FailureReason);
            Assert.That(equivalent.Success, Is.True, equivalent.FailureReason);
            Assert.That(different.Success, Is.True, different.FailureReason);
            Assert.That(equivalent.Mesh, Is.SameAs(original.Mesh));
            Assert.That(equivalent.ContourVertexCount, Is.EqualTo(original.ContourVertexCount));
            Assert.That(equivalent.RingCount, Is.EqualTo(original.RingCount));
            Assert.That(different.Mesh, Is.Not.SameAs(original.Mesh));
            Assert.That(cache.CachedMeshCount, Is.EqualTo(2));
        }

        [Test]
        public void MultipleContoursAndDiagonalTouch_AreRejectedWithoutPublishingMesh()
        {
            FootprintMeshGenerationResult disconnected = _generator.Generate(
                new ShapeFootprint(new[]
                {
                    new GridCoordinate(0, 0), new GridCoordinate(2, 0)
                }), Beveled(2));
            FootprintMeshGenerationResult hole = _generator.Generate(
                new ShapeFootprint(Ring3x3()), Beveled(2));
            FootprintMeshGenerationResult diagonal = _generator.Generate(
                new ShapeFootprint(new[]
                {
                    new GridCoordinate(0, 0), new GridCoordinate(1, 1)
                }), Beveled(2));

            Assert.That(disconnected.Success, Is.False);
            Assert.That(disconnected.Mesh, Is.Null);
            Assert.That(hole.Success, Is.False);
            Assert.That(hole.Mesh, Is.Null);
            Assert.That(diagonal.Success, Is.False);
            Assert.That(diagonal.Mesh, Is.Null);
        }

        private FootprintMeshGenerationResult Generate(
            IReadOnlyList<GridCoordinate> offsets,
            FootprintMeshSettings settings)
        {
            FootprintMeshGenerationResult result =
                _generator.Generate(new ShapeFootprint(offsets), settings);
            if (result.Mesh != null) _meshes.Add(result.Mesh);
            return result;
        }

        private static FootprintMeshSettings Beveled(int segments) =>
            new(Vector2.one, 0.24f, 0.04f, segments, GridCellAnchor.Corner);

        private static IEnumerable<TestCaseData> SupportedFootprints()
        {
            yield return Case("1x1", new[] { new GridCoordinate(0, 0) });
            yield return Case("1x2", new[] { new GridCoordinate(0, 0), new GridCoordinate(1, 0) });
            yield return Case("2x2", Rectangle(2, 2));
            yield return Case("2x3", Rectangle(2, 3));
            yield return Case("L", new[]
            {
                new GridCoordinate(0, 0), new GridCoordinate(0, 1), new GridCoordinate(1, 0)
            });
            yield return Case("T", new[]
            {
                new GridCoordinate(0, 1), new GridCoordinate(1, 1),
                new GridCoordinate(2, 1), new GridCoordinate(1, 0)
            });
            yield return Case("S", new[]
            {
                new GridCoordinate(0, 0), new GridCoordinate(1, 0),
                new GridCoordinate(1, 1), new GridCoordinate(2, 1)
            });
            yield return Case("Z", new[]
            {
                new GridCoordinate(0, 1), new GridCoordinate(1, 1),
                new GridCoordinate(1, 0), new GridCoordinate(2, 0)
            });
            yield return Case("stair", new[]
            {
                new GridCoordinate(0, 0), new GridCoordinate(1, 0),
                new GridCoordinate(1, 1), new GridCoordinate(2, 1),
                new GridCoordinate(2, 2)
            });
        }

        private static TestCaseData Case(string name, GridCoordinate[] offsets) =>
            new TestCaseData(offsets).SetName($"ConnectedFootprint_{name}");

        private static GridCoordinate[] Rectangle(int width, int height)
        {
            List<GridCoordinate> cells = new();
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++) cells.Add(new GridCoordinate(x, y));
            return cells.ToArray();
        }

        private static GridCoordinate[] Ring3x3()
        {
            List<GridCoordinate> cells = new(Rectangle(3, 3));
            cells.Remove(new GridCoordinate(1, 1));
            return cells.ToArray();
        }

        private static void AssertVector(Vector3 actual, Vector3 expected)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(0.0001f));
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(0.0001f));
            Assert.That(actual.z, Is.EqualTo(expected.z).Within(0.0001f));
        }
    }
}
