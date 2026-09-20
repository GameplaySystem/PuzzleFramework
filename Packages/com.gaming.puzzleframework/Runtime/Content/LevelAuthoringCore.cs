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
        public AuthoringEditResult(bool success, string reason, IReadOnlyList<string> affectedItemIds,
            IReadOnlyList<GridCoordinate> affectedCells = null)
        {
            Success = success;
            Reason = reason ?? string.Empty;
            AffectedItemIds = new ReadOnlyCollection<string>(new List<string>(affectedItemIds ?? Array.Empty<string>()));
            AffectedCells = new ReadOnlyCollection<GridCoordinate>(new List<GridCoordinate>(affectedCells ?? Array.Empty<GridCoordinate>()));
        }

        public bool Success { get; }
        public string Reason { get; }
        public IReadOnlyList<string> AffectedItemIds { get; }
        public IReadOnlyList<GridCoordinate> AffectedCells { get; }
        public static AuthoringEditResult Accepted => new(true, string.Empty, Array.Empty<string>());
    }

    /// <summary>Non-mutating report of authored content affected by a structural board edit.</summary>
    public sealed class AuthoringStructuralImpact
    {
        public AuthoringStructuralImpact(string reason, IReadOnlyList<string> itemIds,
            IReadOnlyList<GridCoordinate> cells)
        {
            Reason = reason ?? string.Empty;
            AffectedItemIds = new ReadOnlyCollection<string>(new List<string>(itemIds ?? Array.Empty<string>()));
            AffectedCells = new ReadOnlyCollection<GridCoordinate>(new List<GridCoordinate>(cells ?? Array.Empty<GridCoordinate>()));
        }

        public string Reason { get; }
        public IReadOnlyList<string> AffectedItemIds { get; }
        public IReadOnlyList<GridCoordinate> AffectedCells { get; }
        public bool CanApply => Reason.Length == 0 && AffectedItemIds.Count == 0 && AffectedCells.Count == 0;
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

        /// <summary>Finds an authored footprint at a cell without changing selection.</summary>
        public bool TryFindItemAtCell(GridCoordinate coordinate, out AuthoredFootprint item)
        {
            foreach (AuthoredFootprint candidate in _items.Values)
                if (Contains(candidate, coordinate))
                {
                    item = candidate;
                    return true;
                }
            item = null;
            return false;
        }

        public bool SelectItemAtCell(GridCoordinate coordinate)
        {
            return TryFindItemAtCell(coordinate, out AuthoredFootprint item) && SelectItem(item.Id);
        }

        public bool TryGetItem(string id, out AuthoredFootprint item)
        {
            if (id == null) { item = null; return false; }
            return _items.TryGetValue(id, out item);
        }

        /// <summary>Reports cropped items and non-default cells; the caller decides how to respond.</summary>
        public AuthoringStructuralImpact InspectResize(int width, int height,
            Func<int, int, IReadOnlyList<string>> additionalAffectedIds = null)
        {
            if (width <= 0 || height <= 0)
                return new AuthoringStructuralImpact("Board dimensions must be positive.", null, null);
            List<string> items = new();
            List<GridCoordinate> cells = new();
            foreach (AuthoredFootprint item in _items.Values)
                if (!FitsDimensions(item, width, height)) items.Add(item.Id);
            foreach (KeyValuePair<GridCoordinate, AuthoredCellState> entry in _cells)
                if ((entry.Key.X >= width || entry.Key.Y >= height) &&
                    entry.Value != AuthoredCellState.Active)
                    cells.Add(entry.Key);
            if (additionalAffectedIds != null)
                items.AddRange(additionalAffectedIds(width, height) ?? Array.Empty<string>());
            return new AuthoringStructuralImpact(string.Empty, items, cells);
        }

        /// <summary>Reports footprints affected by a cell-state change without changing the cell.</summary>
        public AuthoringStructuralImpact InspectCellStateChange(GridCoordinate coordinate, AuthoredCellState state,
            Func<GridCoordinate, AuthoredCellState, IReadOnlyList<string>> additionalAffectedIds = null)
        {
            if (!IsWithinBoard(coordinate))
                return new AuthoringStructuralImpact($"Cell {coordinate} is outside the board.", null, null);
            if (!Enum.IsDefined(typeof(AuthoredCellState), state))
                return new AuthoringStructuralImpact("Unknown cell state.", null, null);
            List<string> affected = new();
            if (state != AuthoredCellState.Active)
                foreach (AuthoredFootprint item in _items.Values)
                    if (Contains(item, coordinate)) affected.Add(item.Id);
            if (additionalAffectedIds != null)
                affected.AddRange(additionalAffectedIds(coordinate, state) ?? Array.Empty<string>());
            return new AuthoringStructuralImpact(string.Empty, affected, null);
        }

        public AuthoringEditResult TryResize(int width, int height,
            Func<int, int, IReadOnlyList<string>> additionalAffectedIds = null)
        {
            AuthoringStructuralImpact impact = InspectResize(width, height, additionalAffectedIds);
            if (!impact.CanApply)
                return Reject(impact.Reason.Length > 0 ? impact.Reason : "Resize would crop authored content.",
                    impact.AffectedItemIds, impact.AffectedCells);

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
            AuthoringStructuralImpact impact = InspectCellStateChange(coordinate, state, additionalAffectedIds);
            if (!impact.CanApply)
                return Reject(impact.Reason.Length > 0 ? impact.Reason :
                    $"Cell {coordinate} would invalidate authored items.", impact.AffectedItemIds);
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
            AuthoringEditResult fit = EvaluatePlacement(item);
            if (!fit.Success) return fit;
            _items[item.Id] = item;
            return AuthoringEditResult.Accepted;
        }

        /// <summary>Previews generic structural fit without changing the live session.</summary>
        public AuthoringEditResult EvaluatePlacement(AuthoredFootprint item)
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
            return AuthoringEditResult.Accepted;
        }

        public AuthoringEditResult TryMoveItem(string id, GridCoordinate origin)
        {
            if (!TryGetItem(id, out AuthoredFootprint item)) return Reject($"Unknown item '{id}'.");
            return TryPlaceOrMove(new AuthoredFootprint(id, origin, item.Offsets));
        }

        public AuthoringEditResult TryRotateItemClockwise(string id)
        {
            if (!TryGetItem(id, out AuthoredFootprint item)) return Reject($"Unknown item '{id}'.");
            return TryPlaceOrMove(new AuthoredFootprint(id, item.Origin, RotateClockwise(item.Offsets)));
        }

        /// <summary>Builds a separate validated session before a game swaps its live editor state.</summary>
        public static bool TryRestore(BoardDefinitionData board, IEnumerable<AuthoredFootprint> items,
            LevelMetadata metadata, TimerDefinitionData timer, out LevelAuthoringCore session,
            out string reason)
        {
            session = null;
            if (board == null || board.Width <= 0 || board.Height <= 0 || board.Cells == null)
            {
                reason = "Board dimensions and cells are required.";
                return false;
            }
            long expectedCellCount = (long)board.Width * board.Height;
            if (board.Cells.Count != expectedCellCount)
            {
                reason = "Board must contain exactly one entry per cell.";
                return false;
            }
            LevelAuthoringCore candidate = new(board.Width, board.Height, metadata: metadata, timer: timer);
            HashSet<GridCoordinate> seen = new();
            foreach (CellDefinitionData cell in board.Cells)
            {
                if (cell?.Coordinate == null)
                {
                    reason = "Board contains a null cell or coordinate.";
                    return false;
                }
                GridCoordinate coordinate = new(cell.Coordinate.X, cell.Coordinate.Y);
                if (!candidate.IsWithinBoard(coordinate) || !seen.Add(coordinate) ||
                    !Enum.IsDefined(typeof(AuthoredCellState), cell.CellState))
                {
                    reason = $"Board contains an invalid or duplicate cell {coordinate}.";
                    return false;
                }
                candidate._cells[coordinate] = cell.CellState;
            }
            if (items != null)
            {
                HashSet<string> itemIds = new(StringComparer.Ordinal);
                foreach (AuthoredFootprint item in items)
                {
                    if (item == null || !itemIds.Add(item.Id))
                    {
                        reason = "Authored items must have unique non-null IDs.";
                        return false;
                    }
                    AuthoringEditResult fit = candidate.TryPlaceOrMove(item);
                    if (!fit.Success)
                    {
                        reason = $"Item '{item.Id}' cannot be restored: {fit.Reason}";
                        return false;
                    }
                }
            }
            session = candidate;
            reason = string.Empty;
            return true;
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

        /// <summary>
        /// Derives exposed board geometry from the live session. The caller chooses which
        /// structural cell states participate in its visual profile.
        /// </summary>
        public WallGenerationResult CreateBoundary(Func<AuthoredCellState, bool> participates)
        {
            if (participates == null) throw new ArgumentNullException(nameof(participates));
            List<GridCoordinate> coordinates = new();
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    GridCoordinate coordinate = new(x, y);
                    if (participates(_cells[coordinate])) coordinates.Add(coordinate);
                }
            return new WallGenerationSystem().Generate(coordinates);
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
            return BoardAuthoringPicker.TryPickCell(ray, layout, planeOrigin, out coordinate);
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

        private static AuthoringEditResult Reject(string reason, IReadOnlyList<string> affected = null,
            IReadOnlyList<GridCoordinate> affectedCells = null) =>
            new(false, reason, affected ?? Array.Empty<string>(), affectedCells);
    }
}
