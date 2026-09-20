using System;
using NUnit.Framework;
using PuzzleFramework.CoreBoard;
using UnityEngine;

namespace PuzzleFramework.Tests
{
    public sealed class GridWorldLayoutTests
    {
        [Test]
        public void CreateCentered_OddSquareCentersLogicalBoundsAtRequestedPosition()
        {
            GridWorldLayout layout = GridWorldLayout.CreateCentered(
                Vector3.zero,
                5,
                5,
                Vector2.one,
                Vector3.right,
                Vector3.forward);

            AssertVectorApproximately(new Vector3(-2f, 0f, -2f), layout.BoardOrigin);
            AssertVectorApproximately(new Vector3(2f, 0f, 2f),
                layout.GridToWorldPosition(new GridCoordinate(4, 4)));
            AssertVectorApproximately(Vector3.zero,
                Midpoint(
                    layout.GridToWorldPosition(new GridCoordinate(0, 0)),
                    layout.GridToWorldPosition(new GridCoordinate(4, 4))));
        }

        [Test]
        public void CreateCentered_EvenRectangleSupportsArbitraryOrthogonalAxesAndCellSize()
        {
            Vector3 requestedCenter = new(10f, 2f, -4f);
            GridWorldLayout layout = GridWorldLayout.CreateCentered(
                requestedCenter,
                4,
                2,
                new Vector2(2f, 3f),
                Vector3.forward,
                Vector3.left);

            Vector3 firstCell = layout.GridToWorldPosition(new GridCoordinate(0, 0));
            Vector3 lastCell = layout.GridToWorldPosition(new GridCoordinate(3, 1));

            AssertVectorApproximately(new Vector3(11.5f, 2f, -7f), firstCell);
            AssertVectorApproximately(new Vector3(8.5f, 2f, -1f), lastCell);
            AssertVectorApproximately(requestedCenter, Midpoint(firstCell, lastCell));
        }

        [Test]
        public void ExplicitCornerAnchorCentersBoundsAndMapsInteriorPointsToCells()
        {
            GridWorldLayout layout = GridWorldLayout.CreateCentered(Vector3.zero, 4, 2,
                Vector2.one, Vector3.right, Vector3.forward, GridCellAnchor.Corner);
            Assert.AreEqual(GridCellAnchor.Corner, layout.CellAnchor);
            AssertVectorApproximately(new Vector3(-2f, 0f, -1f), layout.BoardOrigin);
            AssertVectorApproximately(new Vector3(-1.5f, 0f, -0.5f),
                layout.CellCenterToWorld(new GridCoordinate(0, 0)));
            Assert.AreEqual(new GridCoordinate(0, 0),
                layout.WorldToCellCoordinate(new Vector3(-1.01f, 0f, -0.01f)));
            Assert.AreEqual(new GridCoordinate(1, 0),
                layout.WorldToCellCoordinate(new Vector3(-1f, 0f, -0.5f)));
        }

        [TestCase(0, 1)]
        [TestCase(1, 0)]
        [TestCase(-1, 1)]
        [TestCase(1, -1)]
        public void CreateCentered_RejectsNonPositiveDimensions(int width, int height)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                GridWorldLayout.CreateCentered(
                    Vector3.zero,
                    width,
                    height,
                    Vector2.one,
                    Vector3.right,
                    Vector3.forward));
        }

        private static Vector3 Midpoint(Vector3 first, Vector3 second)
        {
            return (first + second) * 0.5f;
        }

        private static void AssertVectorApproximately(Vector3 expected, Vector3 actual)
        {
            Assert.That(Vector3.Distance(expected, actual), Is.LessThan(0.0001f));
        }
    }
}
