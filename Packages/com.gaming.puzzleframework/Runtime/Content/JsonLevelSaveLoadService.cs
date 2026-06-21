using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PuzzleFramework.Content
{
    /// <summary>
    /// JSON-based persistence service for authored level definitions.
    /// This stays at the Content Systems boundary and does not perform runtime construction.
    /// </summary>
    public sealed class JsonLevelSaveLoadService : ILevelSaveLoadService
    {
        /// <inheritdoc />
        public SaveResult Save(LevelDefinition levelDefinition, string path)
        {
            if (!LevelDefinitionValidation.TryValidate(levelDefinition, out string validationFailure))
            {
                return SaveResult.Failed(validationFailure);
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                return SaveResult.Failed("Save path is required.");
            }

            try
            {
                string directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonUtility.ToJson(levelDefinition, true);
                File.WriteAllText(path, json);
                return SaveResult.Successful();
            }
            catch (Exception exception)
            {
                return SaveResult.Failed($"Failed to save level definition: {exception.Message}");
            }
        }

        /// <inheritdoc />
        public LoadResult<LevelDefinition> Load(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return LoadResult<LevelDefinition>.Failed("Load path is required.");
            }

            if (!File.Exists(path))
            {
                return LoadResult<LevelDefinition>.Failed($"Level file does not exist: {path}");
            }

            try
            {
                string json = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return LoadResult<LevelDefinition>.Failed("Level file is empty.");
                }

                LevelDefinition levelDefinition = JsonUtility.FromJson<LevelDefinition>(json);
                if (!LevelDefinitionValidation.TryValidate(levelDefinition, out string validationFailure))
                {
                    return LoadResult<LevelDefinition>.Failed(validationFailure);
                }

                return LoadResult<LevelDefinition>.Successful(levelDefinition);
            }
            catch (Exception exception)
            {
                return LoadResult<LevelDefinition>.Failed(
                    $"Failed to load level definition from JSON: {exception.Message}");
            }
        }
    }

    // This validation layer is limited to persistence/schema integrity checks.
    // It must not absorb runtime construction validation or gameplay-specific rules.
    internal static class LevelDefinitionValidation
    {
        public static bool TryValidate(LevelDefinition levelDefinition, out string failureReason)
        {
            if (levelDefinition == null)
            {
                failureReason = "Level definition is null.";
                return false;
            }

            if (levelDefinition.Metadata == null)
            {
                failureReason = "Level metadata is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(levelDefinition.Metadata.LevelId))
            {
                failureReason = "Level metadata must include a level id.";
                return false;
            }

            if (levelDefinition.Metadata.Version <= 0)
            {
                failureReason = "Level metadata must include a positive version.";
                return false;
            }

            if (levelDefinition.FrameworkData == null)
            {
                failureReason = "Framework data is required.";
                return false;
            }

            if (levelDefinition.FrameworkData.Board == null)
            {
                failureReason = "Board data is required.";
                return false;
            }

            if (levelDefinition.FrameworkData.Board.Width <= 0 ||
                levelDefinition.FrameworkData.Board.Height <= 0)
            {
                failureReason = "Board dimensions must be positive.";
                return false;
            }

            if (levelDefinition.FrameworkData.Board.Cells == null)
            {
                failureReason = "Board cells collection is required.";
                return false;
            }

            if (!ValidateCells(levelDefinition.FrameworkData.Board, out failureReason))
            {
                return false;
            }

            if (!ValidateTimer(levelDefinition.FrameworkData.Timer, out failureReason))
            {
                return false;
            }

            if (levelDefinition.ContentPayload == null)
            {
                failureReason = "Content payload is required.";
                return false;
            }

            if (!ValidateContentPayload(levelDefinition.ContentPayload, out failureReason))
            {
                return false;
            }

            failureReason = string.Empty;
            return true;
        }

        private static bool ValidateCells(BoardDefinitionData boardData, out string failureReason)
        {
            // Duplicate coordinate rejection belongs here because cell collection integrity
            // is authored schema validation, not downstream runtime behavior.
            HashSet<string> occupiedCoordinates = new();

            for (int i = 0; i < boardData.Cells.Count; i++)
            {
                CellDefinitionData cell = boardData.Cells[i];
                if (cell == null)
                {
                    failureReason = $"Board cell at index {i} is null.";
                    return false;
                }

                if (cell.Coordinate == null)
                {
                    failureReason = $"Board cell at index {i} is missing its coordinate.";
                    return false;
                }

                if (cell.Coordinate.X < 0 || cell.Coordinate.X >= boardData.Width ||
                    cell.Coordinate.Y < 0 || cell.Coordinate.Y >= boardData.Height)
                {
                    failureReason =
                        $"Board cell at index {i} is outside the declared board dimensions.";
                    return false;
                }

                string coordinateKey = $"{cell.Coordinate.X},{cell.Coordinate.Y}";
                if (!occupiedCoordinates.Add(coordinateKey))
                {
                    failureReason =
                        $"Board contains duplicate authored cell coordinates at {coordinateKey}.";
                    return false;
                }
            }

            failureReason = string.Empty;
            return true;
        }

        private static bool ValidateTimer(TimerDefinitionData timerData, out string failureReason)
        {
            if (timerData == null)
            {
                failureReason = "Timer data is required.";
                return false;
            }

            if (!timerData.IsEnabled)
            {
                failureReason = string.Empty;
                return true;
            }

            if (timerData.DurationSeconds <= 0f)
            {
                failureReason = "Enabled timer data must include a positive duration.";
                return false;
            }

            if (timerData.WarningThresholdSeconds < 0f)
            {
                failureReason = "Timer warning threshold cannot be negative.";
                return false;
            }

            failureReason = string.Empty;
            return true;
        }

        private static bool ValidateContentPayload(
            SerializedLevelContentPayload contentPayload,
            out string failureReason)
        {
            if (!string.IsNullOrWhiteSpace(contentPayload.PayloadJson) &&
                string.IsNullOrWhiteSpace(contentPayload.ContentTypeId))
            {
                failureReason =
                    "Content payload type id is required when payload JSON is provided.";
                return false;
            }

            failureReason = string.Empty;
            return true;
        }
    }
}
