using System;
using System.Collections.Generic;

namespace PuzzleFramework.CoreBoard
{
    /// <summary>
    /// Derives exposed edges and vertex topology from an explicit cell-participation mask.
    /// </summary>
    public sealed class WallGenerationSystem
    {
        private static readonly BoardEdgeDirection[] EdgeOrder =
        {
            BoardEdgeDirection.North,
            BoardEdgeDirection.East,
            BoardEdgeDirection.South,
            BoardEdgeDirection.West
        };

        public WallGenerationResult Generate(
            IEnumerable<GridCoordinate> participatingCoordinates)
        {
            if (participatingCoordinates == null)
            {
                throw new ArgumentNullException(nameof(participatingCoordinates));
            }

            List<GridCoordinate> coordinates = new();
            HashSet<GridCoordinate> coordinateSet = new();
            foreach (GridCoordinate coordinate in participatingCoordinates)
            {
                if (!coordinateSet.Add(coordinate))
                {
                    throw new ArgumentException(
                        $"Boundary input contains duplicate coordinate {coordinate}.",
                        nameof(participatingCoordinates));
                }

                coordinates.Add(coordinate);
            }

            coordinates.Sort(CompareCoordinates);

            List<BoardBoundaryEdge> edges = BuildEdges(coordinates, coordinateSet);
            List<BoardBoundaryVertex> vertices = BuildVertices(coordinates, coordinateSet);
            return new WallGenerationResult(coordinates, edges, vertices);
        }

        private static List<BoardBoundaryEdge> BuildEdges(
            IReadOnlyList<GridCoordinate> coordinates,
            HashSet<GridCoordinate> coordinateSet)
        {
            List<BoardBoundaryEdge> edges = new();

            for (int coordinateIndex = 0; coordinateIndex < coordinates.Count; coordinateIndex++)
            {
                GridCoordinate coordinate = coordinates[coordinateIndex];
                for (int directionIndex = 0; directionIndex < EdgeOrder.Length; directionIndex++)
                {
                    BoardEdgeDirection direction = EdgeOrder[directionIndex];
                    GridCoordinate neighbor = GetNeighbor(coordinate, direction);
                    if (!coordinateSet.Contains(neighbor))
                    {
                        edges.Add(new BoardBoundaryEdge(coordinate, direction));
                    }
                }
            }

            return edges;
        }

        private static List<BoardBoundaryVertex> BuildVertices(
            IReadOnlyList<GridCoordinate> coordinates,
            HashSet<GridCoordinate> coordinateSet)
        {
            HashSet<GridVertexCoordinate> candidateSet = new();
            for (int i = 0; i < coordinates.Count; i++)
            {
                GridCoordinate coordinate = coordinates[i];
                candidateSet.Add(new GridVertexCoordinate(coordinate.X, coordinate.Y));
                candidateSet.Add(new GridVertexCoordinate(coordinate.X + 1, coordinate.Y));
                candidateSet.Add(new GridVertexCoordinate(coordinate.X, coordinate.Y + 1));
                candidateSet.Add(new GridVertexCoordinate(coordinate.X + 1, coordinate.Y + 1));
            }

            List<GridVertexCoordinate> candidates = new(candidateSet);
            candidates.Sort(CompareVertices);

            List<BoardBoundaryVertex> vertices = new();
            for (int i = 0; i < candidates.Count; i++)
            {
                GridVertexCoordinate vertex = candidates[i];
                BoardVertexQuadrants quadrants = ResolveQuadrants(vertex, coordinateSet);
                int count = CountQuadrants(quadrants);
                if (count == 0 || count == 4)
                {
                    continue;
                }

                BoardBoundaryVertexKind kind = ClassifyVertex(count, quadrants);
                vertices.Add(new BoardBoundaryVertex(vertex, kind, quadrants));
            }

            return vertices;
        }

        private static BoardVertexQuadrants ResolveQuadrants(
            GridVertexCoordinate vertex,
            HashSet<GridCoordinate> coordinateSet)
        {
            BoardVertexQuadrants quadrants = BoardVertexQuadrants.None;

            if (coordinateSet.Contains(new GridCoordinate(vertex.X - 1, vertex.Y - 1)))
            {
                quadrants |= BoardVertexQuadrants.SouthWest;
            }

            if (coordinateSet.Contains(new GridCoordinate(vertex.X, vertex.Y - 1)))
            {
                quadrants |= BoardVertexQuadrants.SouthEast;
            }

            if (coordinateSet.Contains(new GridCoordinate(vertex.X - 1, vertex.Y)))
            {
                quadrants |= BoardVertexQuadrants.NorthWest;
            }

            if (coordinateSet.Contains(new GridCoordinate(vertex.X, vertex.Y)))
            {
                quadrants |= BoardVertexQuadrants.NorthEast;
            }

            return quadrants;
        }

        private static BoardBoundaryVertexKind ClassifyVertex(
            int participatingCount,
            BoardVertexQuadrants quadrants)
        {
            if (participatingCount == 1)
            {
                return BoardBoundaryVertexKind.Convex;
            }

            if (participatingCount == 3)
            {
                return BoardBoundaryVertexKind.Concave;
            }

            bool diagonal = quadrants ==
                            (BoardVertexQuadrants.SouthWest | BoardVertexQuadrants.NorthEast) ||
                            quadrants ==
                            (BoardVertexQuadrants.SouthEast | BoardVertexQuadrants.NorthWest);
            return diagonal
                ? BoardBoundaryVertexKind.DiagonalTouch
                : BoardBoundaryVertexKind.Straight;
        }

        private static int CountQuadrants(BoardVertexQuadrants quadrants)
        {
            int value = (int)quadrants;
            int count = 0;
            while (value != 0)
            {
                count += value & 1;
                value >>= 1;
            }

            return count;
        }

        private static GridCoordinate GetNeighbor(
            GridCoordinate coordinate,
            BoardEdgeDirection direction)
        {
            return direction switch
            {
                BoardEdgeDirection.North => coordinate.Offset(0, 1),
                BoardEdgeDirection.East => coordinate.Offset(1, 0),
                BoardEdgeDirection.South => coordinate.Offset(0, -1),
                BoardEdgeDirection.West => coordinate.Offset(-1, 0),
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }

        private static int CompareCoordinates(GridCoordinate left, GridCoordinate right)
        {
            int yComparison = left.Y.CompareTo(right.Y);
            return yComparison != 0 ? yComparison : left.X.CompareTo(right.X);
        }

        private static int CompareVertices(
            GridVertexCoordinate left,
            GridVertexCoordinate right)
        {
            int yComparison = left.Y.CompareTo(right.Y);
            return yComparison != 0 ? yComparison : left.X.CompareTo(right.X);
        }
    }
}
