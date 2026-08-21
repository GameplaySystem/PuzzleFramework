using System;
using PuzzleFramework.CoreBoard;

namespace PuzzleFramework.Presentation
{
    /// <summary>
    /// Directional half-wall slots on one modular board cell.
    /// </summary>
    [Flags]
    public enum ModularHalfWallFlags
    {
        None = 0,
        NorthWest = 1 << 0,
        NorthEast = 1 << 1,
        EastNorth = 1 << 2,
        EastSouth = 1 << 3,
        SouthEast = 1 << 4,
        SouthWest = 1 << 5,
        WestSouth = 1 << 6,
        WestNorth = 1 << 7
    }

    /// <summary>
    /// Cell-local corner slots shared by convex caps and concave elbows.
    /// </summary>
    [Flags]
    public enum ModularCornerFlags
    {
        None = 0,
        NorthEast = 1 << 0,
        SouthEast = 1 << 1,
        SouthWest = 1 << 2,
        NorthWest = 1 << 3
    }

    /// <summary>
    /// Complete modular visual state for one participating cell.
    /// </summary>
    public readonly struct ModularBoardCellVisualState
    {
        public ModularBoardCellVisualState(
            GridCoordinate coordinate,
            ModularHalfWallFlags halfWalls,
            ModularCornerFlags convexCorners,
            ModularCornerFlags concaveCorners)
        {
            Coordinate = coordinate;
            HalfWalls = halfWalls;
            ConvexCorners = convexCorners;
            ConcaveCorners = concaveCorners;
        }

        public GridCoordinate Coordinate { get; }
        public ModularHalfWallFlags HalfWalls { get; }
        public ModularCornerFlags ConvexCorners { get; }
        public ModularCornerFlags ConcaveCorners { get; }
    }
}
