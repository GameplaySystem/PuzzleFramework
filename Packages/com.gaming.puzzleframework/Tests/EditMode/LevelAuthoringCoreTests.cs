using System.Collections.Generic;
using NUnit.Framework;
using PuzzleFramework.Content;
using PuzzleFramework.CoreBoard;
using UnityEngine;

namespace PuzzleFramework.Tests
{
    public sealed class LevelAuthoringCoreTests
    {
        [Test]
        public void StructuralEditsRejectAffectedItemsWithoutMutation()
        {
            LevelAuthoringCore core = new(4, 3, new[]
            {
                new AuthoredFootprint("piece-a", new GridCoordinate(2, 1),
                    new[] { new GridCoordinate(0, 0), new GridCoordinate(1, 0) })
            });

            AuthoringEditResult resize = core.TryResize(3, 3);
            Assert.IsFalse(resize.Success);
            CollectionAssert.Contains(resize.AffectedItemIds, "piece-a");
            Assert.AreEqual(4, core.Width);

            AuthoringEditResult paint = core.TrySetCellState(
                new GridCoordinate(3, 1), AuthoredCellState.Blocked);
            Assert.IsFalse(paint.Success);
            CollectionAssert.Contains(paint.AffectedItemIds, "piece-a");
            Assert.AreEqual(AuthoredCellState.Active, core.GetCellState(new GridCoordinate(3, 1)));
        }

        [Test]
        public void FailedMoveKeepsItemAndSuccessfulMoveCanBeSelectedAndErased()
        {
            LevelAuthoringCore core = new(3, 3);
            Assert.IsTrue(core.TryPlaceOrMove(new AuthoredFootprint("a", new GridCoordinate(0, 0),
                new[] { new GridCoordinate(0, 0) })).Success);
            Assert.IsTrue(core.TryPlaceOrMove(new AuthoredFootprint("b", new GridCoordinate(1, 0),
                new[] { new GridCoordinate(0, 0) })).Success);
            Assert.IsFalse(core.TryPlaceOrMove(new AuthoredFootprint("a", new GridCoordinate(1, 0),
                new[] { new GridCoordinate(0, 0) })).Success);
            Assert.IsTrue(core.SelectItem("a"));
            Assert.AreEqual("a", core.SelectedItemId);
            Assert.IsTrue(core.Erase("a"));
            Assert.IsNull(core.SelectedItemId);
            Assert.AreEqual(1, core.Items.Count);
        }

        [Test]
        public void RotateAndResizePreserveExplicitBoardStates()
        {
            IReadOnlyList<GridCoordinate> rotated = LevelAuthoringCore.RotateClockwise(new[]
            {
                new GridCoordinate(0, 0), new GridCoordinate(1, 0), new GridCoordinate(1, 1)
            });
            CollectionAssert.AreEquivalent(new[]
            {
                new GridCoordinate(0, 0), new GridCoordinate(0, -1), new GridCoordinate(1, -1)
            }, rotated);

            LevelAuthoringCore core = new(2, 2);
            Assert.IsTrue(core.TrySetCellState(new GridCoordinate(0, 0), AuthoredCellState.Inactive).Success);
            Assert.IsFalse(core.TryResize(0, 2).Success);
            Assert.IsTrue(core.TryResize(3, 2).Success);
            Assert.AreEqual(AuthoredCellState.Inactive, core.GetCellState(new GridCoordinate(0, 0)));
            Assert.AreEqual(AuthoredCellState.Active, core.GetCellState(new GridCoordinate(2, 0)));
            Assert.IsFalse(core.TryResize(0, 0).Success);
        }

        [Test]
        public void GameOwnedStructuralVetoAndLevelSnapshotRemainOpaque()
        {
            LevelAuthoringCore core = new(3, 2);
            AuthoringEditResult resize = core.TryResize(2, 2,
                (_, _) => new[] { "portal-7" });
            Assert.IsFalse(resize.Success);
            CollectionAssert.Contains(resize.AffectedItemIds, "portal-7");
            Assert.AreEqual(3, core.Width);

            AuthoringEditResult paint = core.TrySetCellState(
                new GridCoordinate(1, 0), AuthoredCellState.Inactive,
                (_, _) => new[] { "portal-7" });
            Assert.IsFalse(paint.Success);
            Assert.AreEqual(AuthoredCellState.Active, core.GetCellState(new GridCoordinate(1, 0)));

            core.SetMetadata("level-1", "First", 2);
            core.SetTimer(true, TimerMode.Countdown, 60f, 10f);
            LevelDefinition snapshot = core.CreateLevelSnapshot("module.payload", "{\"opaque\":true}");
            Assert.AreEqual(6, snapshot.FrameworkData.Board.Cells.Count);
            Assert.AreEqual("{\"opaque\":true}", snapshot.ContentPayload.PayloadJson);
            Assert.AreEqual(60f, snapshot.FrameworkData.Timer.DurationSeconds);
        }

        [Test]
        public void CellPickingUsesBoardLayoutAndDoesNotClampToBounds()
        {
            GridWorldLayout layout = new(Vector3.zero, Vector2.one,
                Vector3.right, Vector3.forward);
            Ray ray = new(new Vector3(2.1f, 5f, 1.9f), Vector3.down);
            Assert.IsTrue(LevelAuthoringCore.TryPickCell(ray, layout, Vector3.zero,
                out GridCoordinate coordinate));
            Assert.AreEqual(new GridCoordinate(2, 2), coordinate);
        }
    }
}
