using System.Collections.Generic;
using NUnit.Framework;
using PuzzleFramework.CoreBoard;
using PuzzleFramework.Interaction;
using UnityEngine;

namespace PuzzleFramework.Tests
{
    public sealed class SharedMovementPrimitivesTests
    {
        [Test]
        public void TransferFootprint_ChangesOnlySourceAndDestinationAfterFullValidation()
        {
            GridBoard board = BuildBoard(4, 2);
            CellOccupancySystem occupancy = new(board);
            occupancy.Occupy(new GridCoordinate(0, 0));
            occupancy.Occupy(new GridCoordinate(1, 0));
            occupancy.Occupy(new GridCoordinate(3, 0));

            CellOccupancyOperationResult blocked = occupancy.TransferFootprint(
                new[] { new GridCoordinate(0, 0), new GridCoordinate(1, 0) },
                new[] { new GridCoordinate(1, 0), new GridCoordinate(3, 0) });

            Assert.IsFalse(blocked.Success);
            Assert.IsTrue(occupancy.IsOccupied(new GridCoordinate(0, 0)));
            Assert.IsTrue(occupancy.IsOccupied(new GridCoordinate(1, 0)));

            CellOccupancyOperationResult moved = occupancy.TransferFootprint(
                new[] { new GridCoordinate(0, 0), new GridCoordinate(1, 0) },
                new[] { new GridCoordinate(1, 0), new GridCoordinate(2, 0) });

            Assert.IsTrue(moved.Success, moved.FailureReason);
            Assert.IsFalse(occupancy.IsOccupied(new GridCoordinate(0, 0)));
            Assert.IsTrue(occupancy.IsOccupied(new GridCoordinate(1, 0)));
            Assert.IsTrue(occupancy.IsOccupied(new GridCoordinate(2, 0)));
            Assert.IsTrue(occupancy.IsOccupied(new GridCoordinate(3, 0)));
        }

        [Test]
        public void TransferFootprint_RejectsDuplicateAndMissingSourceWithoutMutation()
        {
            CellOccupancySystem occupancy = new(BuildBoard(3, 1));
            occupancy.Occupy(new GridCoordinate(0, 0));

            Assert.IsFalse(occupancy.TransferFootprint(
                new[] { new GridCoordinate(0, 0) },
                new[] { new GridCoordinate(1, 0), new GridCoordinate(1, 0) }).Success);
            Assert.IsFalse(occupancy.TransferFootprint(
                new[] { new GridCoordinate(2, 0) },
                new[] { new GridCoordinate(1, 0) }).Success);
            Assert.IsTrue(occupancy.IsOccupied(new GridCoordinate(0, 0)));
            Assert.IsFalse(occupancy.IsOccupied(new GridCoordinate(1, 0)));
        }

        [Test]
        public void SweepAndClearance_DetectIntermediateOccupiedCell()
        {
            GridBoard board = BuildBoard(5, 1);
            CellOccupancySystem occupancy = new(board);
            occupancy.Occupy(new GridCoordinate(0, 0));
            occupancy.Occupy(new GridCoordinate(2, 0));
            GridWorldLayout layout = new(Vector3.zero, Vector2.one);
            ShapeAwareDragFootprint footprint = new(
                new[] { new GridCoordinate(0, 0) }, 0f);

            IReadOnlyList<SweptFootprintContactGroup> groups =
                new SweptFootprintHelper().EnumerateContactGroups(new SweptFootprintRequest(
                    layout.BoardLocalToWorld(Vector2.zero),
                    layout.BoardLocalToWorld(new Vector2(4f, 0f)),
                    layout,
                    footprint.Rectangles));
            FootprintClearanceQuery clearance = new(
                board, occupancy, new[] { new GridCoordinate(0, 0) });

            bool sawIntermediateBlocker = false;
            foreach (SweptFootprintContactGroup group in groups)
            {
                FootprintClearanceResult result = clearance.Evaluate(group.OverlappedCells);
                if (result.Failure == FootprintClearanceFailure.Occupied &&
                    result.BlockingCoordinate == new GridCoordinate(2, 0))
                {
                    sawIntermediateBlocker = true;
                    break;
                }
            }

            Assert.IsTrue(sawIntermediateBlocker);
        }

        [Test]
        public void BoardProjection_PreservesGrabOffset()
        {
            Ray ray = new(new Vector3(1f, 5f, 2f), Vector3.down);
            Assert.IsTrue(BoardPointerProjection.TryProject(
                ray, Vector3.zero, Vector3.up, out Vector3 pointer));
            Vector3 offset = BoardPointerProjection.CaptureOffset(
                new Vector3(3f, 0f, 2f), pointer);
            Assert.AreEqual(new Vector3(2f, 0f, 0f), offset);
            Assert.AreEqual(new Vector3(4f, 0f, 2f),
                BoardPointerProjection.ApplyOffset(new Vector3(2f, 0f, 2f), offset));
        }

        private static GridBoard BuildBoard(int width, int height)
        {
            List<GridCoordinate> coordinates = new();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    coordinates.Add(new GridCoordinate(x, y));
                }
            }

            return new GridBoard(width, height, coordinates);
        }
    }
}
