using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PuzzleFramework.CoreBoard;
using PuzzleFramework.Presentation;

namespace PuzzleFramework.Tests
{
    public sealed class ModularBoardVisualSystemTests
    {
        private readonly WallGenerationSystem _wallGeneration = new();
        private readonly ModularBoardVisualPlanner _planner = new();

        [Test]
        public void Generate_RejectsDuplicateCoordinates()
        {
            GridCoordinate coordinate = new(0, 0);

            Assert.Throws<ArgumentException>(() =>
                _wallGeneration.Generate(new[] { coordinate, coordinate }));
        }

        [Test]
        public void SingleCell_GeneratesFourEdgesAndAllConvexCorners()
        {
            WallGenerationResult boundary = _wallGeneration.Generate(
                new[] { new GridCoordinate(0, 0) });

            Assert.That(boundary.ExposedEdges.Count, Is.EqualTo(4));
            Assert.That(CountVertices(boundary, BoardBoundaryVertexKind.Convex), Is.EqualTo(4));
            Assert.That(CountVertices(boundary, BoardBoundaryVertexKind.Concave), Is.Zero);

            ModularBoardVisualPlan plan = _planner.CreatePlan(boundary);
            Assert.That(plan.Success, Is.True, plan.FailureReason);
            Assert.That(plan.TryGetCellState(new GridCoordinate(0, 0), out var state), Is.True);
            Assert.That(state.HalfWalls, Is.EqualTo((ModularHalfWallFlags)255));
            Assert.That(state.ConvexCorners, Is.EqualTo(
                ModularCornerFlags.NorthEast |
                ModularCornerFlags.SouthEast |
                ModularCornerFlags.SouthWest |
                ModularCornerFlags.NorthWest));
            Assert.That(state.ConcaveCorners, Is.EqualTo(ModularCornerFlags.None));
        }

        [Test]
        public void FullRectangle_GeneratesOnlyPerimeterAndFourConvexCorners()
        {
            WallGenerationResult boundary = _wallGeneration.Generate(CreateRectangle(3, 2));

            Assert.That(boundary.ExposedEdges.Count, Is.EqualTo(10));
            Assert.That(CountVertices(boundary, BoardBoundaryVertexKind.Convex), Is.EqualTo(4));
            Assert.That(CountVertices(boundary, BoardBoundaryVertexKind.Concave), Is.Zero);
            Assert.That(boundary.HasDiagonalTouch, Is.False);
        }

        [Test]
        public void CenterHole_GeneratesConcaveCornersInAllFourOrientations()
        {
            List<GridCoordinate> coordinates = CreateRectangle(3, 3);
            coordinates.Remove(new GridCoordinate(1, 1));
            WallGenerationResult boundary = _wallGeneration.Generate(coordinates);

            Assert.That(boundary.ExposedEdges.Count, Is.EqualTo(16));
            Assert.That(CountVertices(boundary, BoardBoundaryVertexKind.Convex), Is.EqualTo(4));
            Assert.That(CountVertices(boundary, BoardBoundaryVertexKind.Concave), Is.EqualTo(4));

            ModularBoardVisualPlan plan = _planner.CreatePlan(boundary);
            AssertCorner(plan, new GridCoordinate(0, 0), ModularCornerFlags.NorthEast);
            AssertCorner(plan, new GridCoordinate(2, 0), ModularCornerFlags.NorthWest);
            AssertCorner(plan, new GridCoordinate(0, 2), ModularCornerFlags.SouthEast);
            AssertCorner(plan, new GridCoordinate(2, 2), ModularCornerFlags.SouthWest);

            AssertHalfDisabled(
                plan,
                new GridCoordinate(0, 1),
                ModularHalfWallFlags.EastSouth);
            AssertHalfDisabled(
                plan,
                new GridCoordinate(1, 0),
                ModularHalfWallFlags.NorthWest);
        }

        [Test]
        public void LShape_GeneratesOneConcaveTurn()
        {
            WallGenerationResult boundary = _wallGeneration.Generate(
                new[]
                {
                    new GridCoordinate(0, 0),
                    new GridCoordinate(1, 0),
                    new GridCoordinate(0, 1)
                });

            Assert.That(boundary.ExposedEdges.Count, Is.EqualTo(8));
            Assert.That(CountVertices(boundary, BoardBoundaryVertexKind.Convex), Is.EqualTo(5));
            Assert.That(CountVertices(boundary, BoardBoundaryVertexKind.Concave), Is.EqualTo(1));

            ModularBoardVisualPlan plan = _planner.CreatePlan(boundary);
            AssertCorner(plan, new GridCoordinate(0, 0), ModularCornerFlags.NorthEast);
        }

        [Test]
        public void DisconnectedRegions_GenerateIndependentBoundaries()
        {
            WallGenerationResult boundary = _wallGeneration.Generate(
                new[]
                {
                    new GridCoordinate(0, 0),
                    new GridCoordinate(3, 0)
                });

            Assert.That(boundary.ExposedEdges.Count, Is.EqualTo(8));
            Assert.That(CountVertices(boundary, BoardBoundaryVertexKind.Convex), Is.EqualTo(8));
            Assert.That(boundary.HasDiagonalTouch, Is.False);
            Assert.That(_planner.CreatePlan(boundary).Success, Is.True);
        }

        [Test]
        public void DiagonalTouch_IsPreservedAndRejectedByInitialVisualProfile()
        {
            WallGenerationResult boundary = _wallGeneration.Generate(
                new[]
                {
                    new GridCoordinate(0, 0),
                    new GridCoordinate(1, 1)
                });

            Assert.That(boundary.HasDiagonalTouch, Is.True);
            Assert.That(
                CountVertices(boundary, BoardBoundaryVertexKind.DiagonalTouch),
                Is.EqualTo(1));

            ModularBoardVisualPlan plan = _planner.CreatePlan(boundary);
            Assert.That(plan.Success, Is.False);
            StringAssert.Contains("diagonally", plan.FailureReason);
        }

        [Test]
        public void EmptyMask_ProducesAnEmptySuccessfulPlan()
        {
            WallGenerationResult boundary = _wallGeneration.Generate(
                Array.Empty<GridCoordinate>());
            ModularBoardVisualPlan plan = _planner.CreatePlan(boundary);

            Assert.That(boundary.ExposedEdges, Is.Empty);
            Assert.That(boundary.BoundaryVertices, Is.Empty);
            Assert.That(plan.Success, Is.True, plan.FailureReason);
            Assert.That(plan.CellStates, Is.Empty);
        }

        private static int CountVertices(
            WallGenerationResult boundary,
            BoardBoundaryVertexKind kind)
        {
            return boundary.BoundaryVertices.Count(vertex => vertex.Kind == kind);
        }

        private static List<GridCoordinate> CreateRectangle(int width, int height)
        {
            List<GridCoordinate> coordinates = new();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    coordinates.Add(new GridCoordinate(x, y));
                }
            }

            return coordinates;
        }

        private static void AssertCorner(
            ModularBoardVisualPlan plan,
            GridCoordinate coordinate,
            ModularCornerFlags expectedCorner)
        {
            Assert.That(plan.Success, Is.True, plan.FailureReason);
            Assert.That(plan.TryGetCellState(coordinate, out var state), Is.True);
            Assert.That(state.ConcaveCorners.HasFlag(expectedCorner), Is.True);
        }

        private static void AssertHalfDisabled(
            ModularBoardVisualPlan plan,
            GridCoordinate coordinate,
            ModularHalfWallFlags halfWall)
        {
            Assert.That(plan.TryGetCellState(coordinate, out var state), Is.True);
            Assert.That(state.HalfWalls.HasFlag(halfWall), Is.False);
        }
    }
}
