using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using PuzzleFramework.CoreBoard;
using UnityEngine;

namespace PuzzleFramework.Content
{
    /// <summary>A game-owned item's structural footprint, without interpreting its payload.</summary>
    public sealed class AuthoredFootprint
    {
        public AuthoredFootprint(string id, GridCoordinate origin, IReadOnlyList<GridCoordinate> offsets)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("An authored item needs an ID.", nameof(id));
            Id = id;
            Origin = origin;
            Offsets = new ShapeFootprint(offsets).Offsets;
        }

        public string Id { get; }
        public GridCoordinate Origin { get; }
        public IReadOnlyList<GridCoordinate> Offsets { get; }
    }

    /// <summary>Result of a structural authoring edit; failure never mutates the session.</summary>
    public sealed class AuthoringEditResult
    {
        public AuthoringEditResult(bool success, string reason, IReadOnlyList<string> affectedItemIds)
        {
            Success = success;
            Reason = reason ?? string.Empty;
            AffectedItemIds = new ReadOnlyCollection<string>(new List<string>(affectedItemIds ?? Array.Empty<string>()));
        }

        public bool Success { get; }
        public string Reason { get; }
        public IReadOnlyList<string> AffectedItemIds { get; }
        public static AuthoringEditResult Accepted => new(true, string.Empty, Array.Empty<string>());
    }

    /// <summary>
    /// Small, game-agnostic authoring state for board structure and item footprints.
    /// The caller retains ownership of payloads, tool UI, and serialization.
    /// </summary>
    public sealed class LevelAuthoringCore
    {
        private readonly Dictionary<GridCoordinate, AuthoredCellState> _cells = new();
        private readonly Dictionary<string, AuthoredFootprint> _items = new(StringComparer.Ordinal);

        public LevelAuthoringCore(int width, int height, IEnumerable<AuthoredFootprint> items = null,
            LevelMetadata metadata = null, TimerDefinitionData timer = null)
        {
            if (width <= 0 || height <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            Width = width;
            Height = height;
            Metadata = new LevelMetadata
            {
                LevelId = metadata?.LevelId ?? string.Empty,
                DisplayName = metadata?.DisplayName ?? string.Empty,
                Version = metadata?.Version ?? 1
            };
            Timer = new TimerDefinitionData
            {
                IsEnabled = timer?.IsEnabled ?? false,
                Mode = timer?.Mode ?? TimerMode.Countdown,
                DurationSeconds = timer?.DurationSeconds ?? 0f,
                WarningThresholdSeconds = timer?.WarningThresholdSeconds ?? 0f
            };
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    _cells.Add(new GridCoordinate(x, y), AuthoredCellState.Active);

            if (items == null) return;
            foreach (AuthoredFootprint item in items)
            {
                if (item == null || !_items.TryAdd(item.Id, item))
                    throw new ArgumentException("Authored items must have unique non-null IDs.", nameof(items));
            }
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public LevelMetadata Metadata { get; }
        public TimerDefinitionData Timer { get; }
        public GridCoordinate? SelectedCell { get; private set; }
        public string SelectedItemId { get; private set; }
        public string CurrentToolId { get; set; }
        public IReadOnlyCollection<AuthoredFootprint> Items => _items.Values;

        public AuthoredCellState GetCellState(GridCoordinate coordinate) =>
            _cells.TryGetValue(coordinate, out AuthoredCellState state) ? state : AuthoredCellState.Inactive;

        public void SelectCell(GridCoordinate coordinate)
        {
            SelectedCell = IsWithinBoard(coordinate) ? coordinate : null;
            SelectedItemId = null;
        }

        public bool SelectItem(string id)
        {
            if (id == null || !_items.ContainsKey(id)) return false;
            SelectedItemId = id;
            SelectedCell = null;
            return true;
        }

        public AuthoringEditResult TryResize(int width, int height,
            Func<int, int, IReadOnlyList<string>> additionalAffectedIds = null)
        {
            if (width <= 0 || height <= 0) return Reject("Board dimensions must be positive.");
            List<string> affected = new();
            foreach (AuthoredFootprint item in _items.Values)
                if (!FitsDimensions(item, width, height)) affected.Add(item.Id);
            foreach (KeyValuePair<GridCoordinate, AuthoredCellState> entry in _cells)
                if ((entry.Key.X >= width || entry.Key.Y >= height) &&
                    entry.Value != AuthoredCellState.Active)
                    affected.Add($"cell:{entry.Key}");
            if (additionalAffectedIds != null)
                affected.AddRange(additionalAffectedIds(width, height) ?? Array.Empty<string>());
            if (affected.Count > 0) return Reject("Resize would crop authored items.", affected);

            Dictionary<GridCoordinate, AuthoredCellState> next = new();
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    GridCoordinate coordinate = new(x, y);
                    next.Add(coordinate, _cells.TryGetValue(coordinate, out AuthoredCellState state)
                        ? state : AuthoredCellState.Active);
                }
            _cells.Clear();
            foreach (KeyValuePair<GridCoordinate, AuthoredCellState> entry in next) _cells.Add(entry.Key, entry.Value);
            Width = width;
            Height = height;
            if (SelectedCell.HasValue && !IsWithinBoard(SelectedCell.Value)) SelectedCell = null;
            return AuthoringEditResult.Accepted;
        }

        public AuthoringEditResult TrySetCellState(GridCoordinate coordinate, AuthoredCellState state,
            Func<GridCoordinate, AuthoredCellState, IReadOnlyList<string>> additionalAffectedIds = null)
        {
            if (!IsWithinBoard(coordinate)) return Reject($"Cell {coordinate} is outside the board.");
            if (!Enum.IsDefined(typeof(AuthoredCellState), state)) return Reject("Unknown cell state.");
            List<string> affected = new();
            if (state != AuthoredCellState.Active)
            {
                foreach (AuthoredFootprint item in _items.Values)
                    if (Contains(item, coordinate)) affected.Add(item.Id);
            }
            if (additionalAffectedIds != null)
                affected.AddRange(additionalAffectedIds(coordinate, state) ?? Array.Empty<string>());
            if (affected.Count > 0) return Reject($"Cell {coordinate} would invalidate authored items.", affected);
            _cells[coordinate] = state;
            return AuthoringEditResult.Accepted;
        }

        public void SetMetadata(string levelId, string displayName, int version)
        {
            if (string.IsNullOrWhiteSpace(levelId)) throw new ArgumentException("Level ID is required.", nameof(levelId));
            if (version <= 0) throw new ArgumentOutOfRangeException(nameof(version));
            Metadata.LevelId = levelId.Trim();
            Metadata.DisplayName = (displayName ?? string.Empty).Trim();
            Metadata.Version = version;
        }

        public void SetTimer(bool enabled, TimerMode mode, float durationSeconds, float warningThresholdSeconds)
        {
            if (!Enum.IsDefined(typeof(TimerMode), mode) || float.IsNaN(durationSeconds) ||
                float.IsInfinity(durationSeconds) || durationSeconds < 0f ||
                float.IsNaN(warningThresholdSeconds) || float.IsInfinity(warningThresholdSeconds) ||
                warningThresholdSeconds < 0f)
                throw new ArgumentException("Timer settings are invalid.");
            Timer.IsEnabled = enabled;
            Timer.Mode = mode;
            Timer.DurationSeconds = durationSeconds;
            Timer.WarningThresholdSeconds = warningThresholdSeconds;
        }

        public AuthoringEditResult TryPlaceOrMove(AuthoredFootprint item)
        {
            if (item == null) return Reject("Item is required.");
            foreach (GridCoordinate offset in item.Offsets)
            {
                GridCoordinate coordinate = item.Origin.Offset(offset.X, offset.Y);
                if (!IsWithinBoard(coordinate) || GetCellState(coordinate) != AuthoredCellState.Active)
                    return Reject($"Footprint cell {coordinate} is not active.");
                foreach (AuthoredFootprint other in _items.Values)
                    if (other.Id != item.Id && Contains(other, coordinate))
                        return Reject($"Footprint cell {coordinate} overlaps '{other.Id}'.", new[] { other.Id });
            }
            _items[item.Id] = item;
            return AuthoringEditResult.Accepted;
        }

        public bool Erase(string id)
        {
            if (id == null || !_items.Remove(id)) return false;
            if (SelectedItemId == id) SelectedItemId = null;
            return true;
        }

        public BoardDefinitionData CreateBoardSnapshot()
        {
            BoardDefinitionData board = new() { Width = Width, Height = Height };
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    GridCoordinate coordinate = new(x, y);
                    board.Cells.Add(new CellDefinitionData
                    {
                        Coordinate = new CellCoordinateData { X = x, Y = y },
                        CellState = _cells[coordinate]
                    });
                }
            return board;
        }

        /// <summary>Copies shared authoring fields and transports the caller-owned payload unchanged.</summary>
        public LevelDefinition CreateLevelSnapshot(string contentTypeId, string payloadJson)
        {
            return new LevelDefinition
            {
                Metadata = new LevelMetadata
                {
                    LevelId = Metadata.LevelId,
                    DisplayName = Metadata.DisplayName,
                    Version = Metadata.Version
                },
                FrameworkData = new LevelFrameworkData
                {
                    Board = CreateBoardSnapshot(),
                    Timer = new TimerDefinitionData
                    {
                        IsEnabled = Timer.IsEnabled,
                        Mode = Timer.Mode,
                        DurationSeconds = Timer.DurationSeconds,
                        WarningThresholdSeconds = Timer.WarningThresholdSeconds
                    }
                },
                ContentPayload = new SerializedLevelContentPayload
                {
                    ContentTypeId = contentTypeId ?? string.Empty,
                    PayloadJson = payloadJson ?? string.Empty
                }
            };
        }

        public static IReadOnlyList<GridCoordinate> RotateClockwise(IReadOnlyList<GridCoordinate> offsets)
        {
            ShapeFootprint footprint = new(offsets);
            List<GridCoordinate> rotated = new(footprint.CellCount);
            foreach (GridCoordinate offset in footprint.Offsets)
                rotated.Add(new GridCoordinate(offset.Y, -offset.X));
            return rotated;
        }

        public static bool TryPickCell(Ray ray, GridWorldLayout layout, Vector3 planeOrigin,
            out GridCoordinate coordinate)
        {
            coordinate = default;
            Vector3 normal = Vector3.Cross(layout.BoardYAxis, layout.BoardXAxis).normalized;
            if (!Interaction.BoardPointerProjection.TryProject(ray, planeOrigin, normal, out Vector3 point))
                return false;
            Vector2 local = layout.WorldToBoardLocal(point);
            coordinate = new GridCoordinate(Mathf.RoundToInt(local.x), Mathf.RoundToInt(local.y));
            return true;
        }

        private bool IsWithinBoard(GridCoordinate coordinate) =>
            coordinate.X >= 0 && coordinate.X < Width && coordinate.Y >= 0 && coordinate.Y < Height;

        private static bool Contains(AuthoredFootprint item, GridCoordinate coordinate)
        {
            foreach (GridCoordinate offset in item.Offsets)
                if (item.Origin.Offset(offset.X, offset.Y) == coordinate) return true;
            return false;
        }

        private static bool FitsDimensions(AuthoredFootprint item, int width, int height)
        {
            foreach (GridCoordinate offset in item.Offsets)
            {
                GridCoordinate cell = item.Origin.Offset(offset.X, offset.Y);
                if (cell.X < 0 || cell.X >= width || cell.Y < 0 || cell.Y >= height) return false;
            }
            return true;
        }

        private static AuthoringEditResult Reject(string reason, IReadOnlyList<string> affected = null) =>
            new(false, reason, affected ?? Array.Empty<string>());
    }
}
