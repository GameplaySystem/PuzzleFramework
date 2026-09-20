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

        [Test]
        public void StructuralInspectionReportsItemsAndCellsWithoutEnforcingResponse()
        {
            LevelAuthoringCore core = new(3, 2);
            Assert.IsTrue(core.TrySetCellState(new GridCoordinate(2, 0), AuthoredCellState.Blocked).Success);
            Assert.IsTrue(core.TryPlaceOrMove(new AuthoredFootprint("item", new GridCoordinate(2, 1),
                new[] { new GridCoordinate(0, 0) })).Success);

            AuthoringStructuralImpact impact = core.InspectResize(2, 2,
                (_, _) => new[] { "game-owned-id" });
            CollectionAssert.AreEquivalent(new[] { "item", "game-owned-id" }, impact.AffectedItemIds);
            CollectionAssert.AreEqual(new[] { new GridCoordinate(2, 0) }, impact.AffectedCells);
            Assert.IsFalse(impact.CanApply);
            Assert.AreEqual(3, core.Width);
            AuthoringEditResult rejected = core.TryResize(2, 2);
            Assert.IsFalse(rejected.Success);
            CollectionAssert.AreEqual(new[] { new GridCoordinate(2, 0) }, rejected.AffectedCells);
            Assert.AreEqual(AuthoredCellState.Blocked, core.GetCellState(new GridCoordinate(2, 0)));

            Assert.IsTrue(core.Erase("item"));
            Assert.IsTrue(core.TrySetCellState(new GridCoordinate(2, 0), AuthoredCellState.Active).Success);
            Assert.IsTrue(core.TryResize(2, 2).Success);
        }

        [Test]
        public void LiveSessionPreviewMoveRotateSelectAndEraseAreAtomic()
        {
            LevelAuthoringCore core = new(3, 3);
            Assert.IsTrue(core.TryPlaceOrMove(new AuthoredFootprint("l", new GridCoordinate(1, 1),
                new[] { new GridCoordinate(0, 0), new GridCoordinate(1, 0), new GridCoordinate(0, 1) })).Success);
            Assert.IsTrue(core.TryPlaceOrMove(new AuthoredFootprint("stop", new GridCoordinate(1, 0),
                new[] { new GridCoordinate(0, 0) })).Success);
            Assert.IsTrue(core.TryPlaceOrMove(new AuthoredFootprint("stop2", new GridCoordinate(0, 1),
                new[] { new GridCoordinate(0, 0) })).Success);
            Assert.IsTrue(core.SelectItemAtCell(new GridCoordinate(2, 1)));
            Assert.AreEqual("l", core.SelectedItemId);
            Assert.IsFalse(core.EvaluatePlacement(new AuthoredFootprint("l", new GridCoordinate(0, 1),
                new[] { new GridCoordinate(0, 0) })).Success);
            Assert.IsFalse(core.TryMoveItem("l", new GridCoordinate(2, 1)).Success);
            Assert.IsFalse(core.TryRotateItemClockwise("l").Success);
            Assert.IsTrue(core.TryGetItem("l", out AuthoredFootprint unchanged));
            Assert.AreEqual(new GridCoordinate(1, 1), unchanged.Origin);
            CollectionAssert.Contains(unchanged.Offsets, new GridCoordinate(0, 1));
            Assert.IsTrue(core.Erase("stop"));
            Assert.IsTrue(core.TryRotateItemClockwise("l").Success);
            Assert.IsTrue(core.TryGetItem("l", out AuthoredFootprint rotated));
            CollectionAssert.Contains(rotated.Offsets, new GridCoordinate(0, -1));
            Assert.IsTrue(core.Erase("l"));
            Assert.IsNull(core.SelectedItemId);
        }

        [Test]
        public void RestoreRejectsDuplicateCellsAndOverlappingItemsWithoutPublishingSession()
        {
            LevelAuthoringCore source = new(2, 2);
            BoardDefinitionData board = source.CreateBoardSnapshot();
            board.Cells[1].Coordinate.X = 0;
            Assert.IsFalse(LevelAuthoringCore.TryRestore(board, null, null, null,
                out LevelAuthoringCore duplicate, out _));
            Assert.IsNull(duplicate);

            board = source.CreateBoardSnapshot();
            Assert.IsFalse(LevelAuthoringCore.TryRestore(board, new[]
            {
                new AuthoredFootprint("a", new GridCoordinate(0, 0), new[] { new GridCoordinate(0, 0) }),
                new AuthoredFootprint("b", new GridCoordinate(0, 0), new[] { new GridCoordinate(0, 0) })
            }, null, null, out LevelAuthoringCore overlap, out _));
            Assert.IsNull(overlap);

            Assert.IsTrue(LevelAuthoringCore.TryRestore(board, new[]
            {
                new AuthoredFootprint("a", new GridCoordinate(1, 1), new[] { new GridCoordinate(0, 0) })
            }, null, null, out LevelAuthoringCore restored, out _));
            Assert.IsTrue(restored.TryFindItemAtCell(new GridCoordinate(1, 1), out _));
        }

        [Test]
        public void AnchorAwareCellAndEdgePickingUsesDerivedLiveBoundary()
        {
            LevelAuthoringCore core = new(3, 3);
            Assert.IsTrue(core.TrySetCellState(new GridCoordinate(1, 1), AuthoredCellState.Inactive).Success);
            WallGenerationResult boundary = core.CreateBoundary(state => state == AuthoredCellState.Active);
            GridWorldLayout corner = new(Vector3.zero, Vector2.one, Vector3.right,
                Vector3.forward, GridCellAnchor.Corner);
            Ray cellRay = new(new Vector3(0.75f, 4f, 0.75f), Vector3.down);
            Assert.IsTrue(BoardAuthoringPicker.TryPickCell(cellRay, corner, Vector3.zero,
                out GridCoordinate cell));
            Assert.AreEqual(new GridCoordinate(0, 0), cell);

            Ray edgeRay = new(new Vector3(1.25f, 4f, 1.01f), Vector3.down);
            Assert.IsTrue(BoardAuthoringPicker.TryPickBoundaryEdge(edgeRay, corner, Vector3.zero,
                boundary, 0.1f, out BoardBoundaryEdge edge));
            Assert.AreEqual(new GridCoordinate(1, 0), edge.CellCoordinate);
            Assert.AreEqual(BoardEdgeDirection.North, edge.Direction);

            GridWorldLayout center = new(Vector3.zero, Vector2.one, Vector3.right,
                Vector3.forward, GridCellAnchor.Center);
            Ray centeredRay = new(new Vector3(1.25f, 4f, 0.51f), Vector3.down);
            Assert.IsTrue(BoardAuthoringPicker.TryPickBoundaryEdge(centeredRay, center, Vector3.zero,
                boundary, 0.1f, out BoardBoundaryEdge centeredEdge));
            Assert.AreEqual(edge.CellCoordinate, centeredEdge.CellCoordinate);
            Assert.AreEqual(edge.Direction, centeredEdge.Direction);
        }

        [Test]
        public void ToolHostDispatchesOnlyTheSelectedRegisteredTool()
        {
            LevelAuthoringCore session = new(2, 2);
            AuthoringToolHost host = new(session);
            RecordingTool first = new("first");
            RecordingTool second = new("second");
            host.Register(first);
            host.Register(second);
            Assert.IsFalse(host.SelectTool("missing"));
            Assert.IsFalse(host.Apply(AuthoringTarget.ForCell(new GridCoordinate(0, 0))).Success);
            Assert.IsTrue(host.SelectTool("second"));
            AuthoringTarget target = AuthoringTarget.ForCell(new GridCoordinate(1, 0));
            Assert.IsTrue(host.Preview(target).Success);
            Assert.IsTrue(host.Apply(target).Success);
            Assert.AreEqual(0, first.Calls);
            Assert.AreEqual(2, second.Calls);
            Assert.AreSame(session, second.LastSession);
            Assert.AreEqual(new GridCoordinate(1, 0), second.LastTarget.Cell);
        }

        private sealed class RecordingTool : IAuthoringTool
        {
            public RecordingTool(string id) { Id = id; }
            public string Id { get; }
            public int Calls { get; private set; }
            public LevelAuthoringCore LastSession { get; private set; }
            public AuthoringTarget LastTarget { get; private set; }
            public AuthoringEditResult Preview(LevelAuthoringCore session, AuthoringTarget target) => Record(session, target);
            public AuthoringEditResult Apply(LevelAuthoringCore session, AuthoringTarget target) => Record(session, target);
            private AuthoringEditResult Record(LevelAuthoringCore session, AuthoringTarget target)
            {
                Calls++;
                LastSession = session;
                LastTarget = target;
                return AuthoringEditResult.Accepted;
            }
        }
    }
}
