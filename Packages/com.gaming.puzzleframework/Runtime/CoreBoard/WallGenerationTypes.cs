using System;

namespace PuzzleFramework.CoreBoard
{
    /// <summary>
    /// Orthogonal side of a participating grid cell.
    /// </summary>
    public enum BoardEdgeDirection
    {
        North = 0,
        East = 1,
        South = 2,
        West = 3
    }

    /// <summary>
    /// Cells touching a logical grid vertex.
    /// </summary>
    [Flags]
    public enum BoardVertexQuadrants
    {
        None = 0,
        SouthWest = 1 << 0,
        SouthEast = 1 << 1,
        NorthWest = 1 << 2,
        NorthEast = 1 << 3
    }

    /// <summary>
    /// Boundary behavior at a logical grid vertex.
    /// </summary>
    public enum BoardBoundaryVertexKind
    {
        Straight = 0,
        Convex = 1,
        Concave = 2,
        DiagonalTouch = 3
    }

    /// <summary>
    /// Integer coordinate on the logical lattice between cell centers.
    /// Cell (x, y) owns vertices (x, y) through (x + 1, y + 1).
    /// </summary>
    [Serializable]
    public readonly struct GridVertexCoordinate : IEquatable<GridVertexCoordinate>
    {
        public GridVertexCoordinate(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }
        public int Y { get; }

        public bool Equals(GridVertexCoordinate other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is GridVertexCoordinate other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }

        public static bool operator ==(
            GridVertexCoordinate left,
            GridVertexCoordinate right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            GridVertexCoordinate left,
            GridVertexCoordinate right)
        {
            return !left.Equals(right);
        }
    }

    /// <summary>
    /// One exposed full edge owned by a participating cell.
    /// </summary>
    public readonly struct BoardBoundaryEdge
    {
        public BoardBoundaryEdge(
            GridCoordinate cellCoordinate,
            BoardEdgeDirection direction)
        {
            CellCoordinate = cellCoordinate;
            Direction = direction;
            ResolveVertices(
                cellCoordinate,
                direction,
                out GridVertexCoordinate startVertex,
                out GridVertexCoordinate endVertex);
            StartVertex = startVertex;
            EndVertex = endVertex;
        }

        public GridCoordinate CellCoordinate { get; }
        public BoardEdgeDirection Direction { get; }
        public GridVertexCoordinate StartVertex { get; }
        public GridVertexCoordinate EndVertex { get; }

        private static void ResolveVertices(
            GridCoordinate coordinate,
            BoardEdgeDirection direction,
            out GridVertexCoordinate startVertex,
            out GridVertexCoordinate endVertex)
        {
            switch (direction)
            {
                case BoardEdgeDirection.North:
                    startVertex = new GridVertexCoordinate(coordinate.X, coordinate.Y + 1);
                    endVertex = new GridVertexCoordinate(coordinate.X + 1, coordinate.Y + 1);
                    return;
                case BoardEdgeDirection.East:
                    startVertex = new GridVertexCoordinate(coordinate.X + 1, coordinate.Y);
                    endVertex = new GridVertexCoordinate(coordinate.X + 1, coordinate.Y + 1);
                    return;
                case BoardEdgeDirection.South:
                    startVertex = new GridVertexCoordinate(coordinate.X, coordinate.Y);
                    endVertex = new GridVertexCoordinate(coordinate.X + 1, coordinate.Y);
                    return;
                case BoardEdgeDirection.West:
                    startVertex = new GridVertexCoordinate(coordinate.X, coordinate.Y);
                    endVertex = new GridVertexCoordinate(coordinate.X, coordinate.Y + 1);
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }
    }

    /// <summary>
    /// Classified boundary behavior at one logical grid vertex.
    /// </summary>
    public readonly struct BoardBoundaryVertex
    {
        public BoardBoundaryVertex(
            GridVertexCoordinate coordinate,
            BoardBoundaryVertexKind kind,
            BoardVertexQuadrants participatingQuadrants)
        {
            Coordinate = coordinate;
            Kind = kind;
            ParticipatingQuadrants = participatingQuadrants;
        }

        public GridVertexCoordinate Coordinate { get; }
        public BoardBoundaryVertexKind Kind { get; }
        public BoardVertexQuadrants ParticipatingQuadrants { get; }
    }
}
