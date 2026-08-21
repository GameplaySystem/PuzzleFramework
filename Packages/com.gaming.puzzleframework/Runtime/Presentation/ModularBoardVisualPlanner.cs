using System;
using System.Collections.Generic;
using PuzzleFramework.CoreBoard;

namespace PuzzleFramework.Presentation
{
    /// <summary>
    /// Converts structural boundary facts into the reusable half-wall and corner profile.
    /// </summary>
    public sealed class ModularBoardVisualPlanner
    {
        private const BoardVertexQuadrants AllQuadrants =
            BoardVertexQuadrants.SouthWest |
            BoardVertexQuadrants.SouthEast |
            BoardVertexQuadrants.NorthWest |
            BoardVertexQuadrants.NorthEast;

        public ModularBoardVisualPlan CreatePlan(WallGenerationResult boundary)
        {
            if (boundary == null)
            {
                throw new ArgumentNullException(nameof(boundary));
            }

            if (boundary.HasDiagonalTouch)
            {
                return ModularBoardVisualPlan.Failed(
                    "The modular board visual profile does not support cells that touch only diagonally.");
            }

            Dictionary<GridCoordinate, MutableCellState> states = new();
            for (int i = 0; i < boundary.ParticipatingCoordinates.Count; i++)
            {
                GridCoordinate coordinate = boundary.ParticipatingCoordinates[i];
                states.Add(coordinate, new MutableCellState(coordinate));
            }

            for (int i = 0; i < boundary.ExposedEdges.Count; i++)
            {
                BoardBoundaryEdge edge = boundary.ExposedEdges[i];
                states[edge.CellCoordinate].HalfWalls |= GetFullEdgeFlags(edge.Direction);
            }

            for (int i = 0; i < boundary.BoundaryVertices.Count; i++)
            {
                BoardBoundaryVertex vertex = boundary.BoundaryVertices[i];
                if (vertex.Kind == BoardBoundaryVertexKind.Convex)
                {
                    ApplyConvexCorner(vertex, states);
                }
                else if (vertex.Kind == BoardBoundaryVertexKind.Concave)
                {
                    ApplyConcaveCorner(vertex, boundary.ExposedEdges, states);
                }
            }

            List<ModularBoardCellVisualState> cellStates = new(states.Count);
            for (int i = 0; i < boundary.ParticipatingCoordinates.Count; i++)
            {
                cellStates.Add(states[boundary.ParticipatingCoordinates[i]].ToImmutable());
            }

            return ModularBoardVisualPlan.Successful(cellStates);
        }

        private static void ApplyConvexCorner(
            BoardBoundaryVertex vertex,
            Dictionary<GridCoordinate, MutableCellState> states)
        {
            BoardVertexQuadrants ownerQuadrant = vertex.ParticipatingQuadrants;
            ResolveOwner(
                vertex.Coordinate,
                ownerQuadrant,
                out GridCoordinate ownerCoordinate,
                out ModularCornerFlags localCorner);
            states[ownerCoordinate].ConvexCorners |= localCorner;
        }

        private static void ApplyConcaveCorner(
            BoardBoundaryVertex vertex,
            IReadOnlyList<BoardBoundaryEdge> exposedEdges,
            Dictionary<GridCoordinate, MutableCellState> states)
        {
            BoardVertexQuadrants missingQuadrant = AllQuadrants & ~vertex.ParticipatingQuadrants;
            BoardVertexQuadrants ownerQuadrant = GetOppositeQuadrant(missingQuadrant);
            ResolveOwner(
                vertex.Coordinate,
                ownerQuadrant,
                out GridCoordinate ownerCoordinate,
                out ModularCornerFlags localCorner);
            states[ownerCoordinate].ConcaveCorners |= localCorner;

            // The concave L owns both arms, so remove only the two half-walls ending here.
            for (int i = 0; i < exposedEdges.Count; i++)
            {
                BoardBoundaryEdge edge = exposedEdges[i];
                if (edge.StartVertex == vertex.Coordinate)
                {
                    states[edge.CellCoordinate].HalfWalls &= ~GetEndpointHalf(edge.Direction, true);
                }
                else if (edge.EndVertex == vertex.Coordinate)
                {
                    states[edge.CellCoordinate].HalfWalls &= ~GetEndpointHalf(edge.Direction, false);
                }
            }
        }

        private static ModularHalfWallFlags GetFullEdgeFlags(BoardEdgeDirection direction)
        {
            return direction switch
            {
                BoardEdgeDirection.North =>
                    ModularHalfWallFlags.NorthWest | ModularHalfWallFlags.NorthEast,
                BoardEdgeDirection.East =>
                    ModularHalfWallFlags.EastNorth | ModularHalfWallFlags.EastSouth,
                BoardEdgeDirection.South =>
                    ModularHalfWallFlags.SouthEast | ModularHalfWallFlags.SouthWest,
                BoardEdgeDirection.West =>
                    ModularHalfWallFlags.WestSouth | ModularHalfWallFlags.WestNorth,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }

        private static ModularHalfWallFlags GetEndpointHalf(
            BoardEdgeDirection direction,
            bool isStartVertex)
        {
            return direction switch
            {
                BoardEdgeDirection.North => isStartVertex
                    ? ModularHalfWallFlags.NorthWest
                    : ModularHalfWallFlags.NorthEast,
                BoardEdgeDirection.East => isStartVertex
                    ? ModularHalfWallFlags.EastSouth
                    : ModularHalfWallFlags.EastNorth,
                BoardEdgeDirection.South => isStartVertex
                    ? ModularHalfWallFlags.SouthWest
                    : ModularHalfWallFlags.SouthEast,
                BoardEdgeDirection.West => isStartVertex
                    ? ModularHalfWallFlags.WestSouth
                    : ModularHalfWallFlags.WestNorth,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }

        private static BoardVertexQuadrants GetOppositeQuadrant(BoardVertexQuadrants quadrant)
        {
            return quadrant switch
            {
                BoardVertexQuadrants.SouthWest => BoardVertexQuadrants.NorthEast,
                BoardVertexQuadrants.SouthEast => BoardVertexQuadrants.NorthWest,
                BoardVertexQuadrants.NorthWest => BoardVertexQuadrants.SouthEast,
                BoardVertexQuadrants.NorthEast => BoardVertexQuadrants.SouthWest,
                _ => throw new ArgumentException(
                    "Expected exactly one missing vertex quadrant.",
                    nameof(quadrant))
            };
        }

        private static void ResolveOwner(
            GridVertexCoordinate vertex,
            BoardVertexQuadrants ownerQuadrant,
            out GridCoordinate ownerCoordinate,
            out ModularCornerFlags localCorner)
        {
            switch (ownerQuadrant)
            {
                case BoardVertexQuadrants.SouthWest:
                    ownerCoordinate = new GridCoordinate(vertex.X - 1, vertex.Y - 1);
                    localCorner = ModularCornerFlags.NorthEast;
                    return;
                case BoardVertexQuadrants.SouthEast:
                    ownerCoordinate = new GridCoordinate(vertex.X, vertex.Y - 1);
                    localCorner = ModularCornerFlags.NorthWest;
                    return;
                case BoardVertexQuadrants.NorthWest:
                    ownerCoordinate = new GridCoordinate(vertex.X - 1, vertex.Y);
                    localCorner = ModularCornerFlags.SouthEast;
                    return;
                case BoardVertexQuadrants.NorthEast:
                    ownerCoordinate = new GridCoordinate(vertex.X, vertex.Y);
                    localCorner = ModularCornerFlags.SouthWest;
                    return;
                default:
                    throw new ArgumentException(
                        "Expected exactly one owner quadrant.",
                        nameof(ownerQuadrant));
            }
        }

        private sealed class MutableCellState
        {
            public MutableCellState(GridCoordinate coordinate)
            {
                Coordinate = coordinate;
            }

            public GridCoordinate Coordinate { get; }
            public ModularHalfWallFlags HalfWalls { get; set; }
            public ModularCornerFlags ConvexCorners { get; set; }
            public ModularCornerFlags ConcaveCorners { get; set; }

            public ModularBoardCellVisualState ToImmutable()
            {
                return new ModularBoardCellVisualState(
                    Coordinate,
                    HalfWalls,
                    ConvexCorners,
                    ConcaveCorners);
            }
        }
    }
}
