using System;
using System.Collections.Generic;

namespace PuzzleFramework.Content
{
    /// <summary>
    /// Root authored level definition transported by Content Systems.
    /// It groups shared framework-owned data and an opaque game-module payload.
    /// </summary>
    [Serializable]
    public sealed class LevelDefinition
    {
        public LevelMetadata Metadata = new();
        public LevelFrameworkData FrameworkData = new();
        public SerializedLevelContentPayload ContentPayload = new();
    }

    /// <summary>
    /// Shared metadata required to identify and version an authored level file.
    /// </summary>
    [Serializable]
    public sealed class LevelMetadata
    {
        public string LevelId = string.Empty;
        public string DisplayName = string.Empty;
        public int Version = 1;
    }

    /// <summary>
    /// Framework-owned authored data that remains generic across multiple puzzle games.
    /// </summary>
    [Serializable]
    public sealed class LevelFrameworkData
    {
        public BoardDefinitionData Board = new();
        public TimerDefinitionData Timer = new();
    }

    /// <summary>
    /// Authored board dimensions and per-cell schema data.
    /// This is content data only, not a runtime grid model.
    /// </summary>
    [Serializable]
    public sealed class BoardDefinitionData
    {
        public int Width;
        public int Height;
        public List<CellDefinitionData> Cells = new();
    }

    /// <summary>
    /// Authored cell entry stored in the level file.
    /// </summary>
    [Serializable]
    public sealed class CellDefinitionData
    {
        public CellCoordinateData Coordinate = new();
        public AuthoredCellState CellState = AuthoredCellState.Active;
    }

    /// <summary>
    /// Shared board-space coordinate used by authored content.
    /// </summary>
    [Serializable]
    public sealed class CellCoordinateData
    {
        public int X;
        public int Y;
    }

    /// <summary>
    /// Authored cell participation state.
    /// </summary>
    public enum AuthoredCellState
    {
        Inactive = 0,
        Active = 1,
        Blocked = 2
    }

    /// <summary>
    /// Optional generic timer configuration stored with authored level content.
    /// </summary>
    [Serializable]
    public sealed class TimerDefinitionData
    {
        public bool IsEnabled;
        public TimerMode Mode = TimerMode.Countdown;
        public float DurationSeconds;
        public float WarningThresholdSeconds;
    }

    /// <summary>
    /// Shared timer mode options for authored content.
    /// </summary>
    public enum TimerMode
    {
        Countdown = 0,
        CountUp = 1
    }

    /// <summary>
    /// Opaque game-module payload transported by the framework without interpretation.
    /// </summary>
    [Serializable]
    public sealed class SerializedLevelContentPayload
    {
        public string ContentTypeId = string.Empty;
        public string PayloadJson = string.Empty;
    }
}
