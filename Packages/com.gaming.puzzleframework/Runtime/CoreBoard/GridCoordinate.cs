using System;

namespace PuzzleFramework.CoreBoard
{
    /// <summary>
    /// Shared board-space coordinate owned by the Grid System.
    /// </summary>
    [Serializable]
    public readonly struct GridCoordinate : IEquatable<GridCoordinate>
    {
        public GridCoordinate(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }
        public int Y { get; }

        /// <summary>
        /// Returns a new coordinate offset from the current coordinate.
        /// </summary>
        public GridCoordinate Offset(int deltaX, int deltaY)
        {
            return new GridCoordinate(X + deltaX, Y + deltaY);
        }

        public bool Equals(GridCoordinate other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is GridCoordinate other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }

        public static bool operator ==(GridCoordinate left, GridCoordinate right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GridCoordinate left, GridCoordinate right)
        {
            return !left.Equals(right);
        }
    }
}
