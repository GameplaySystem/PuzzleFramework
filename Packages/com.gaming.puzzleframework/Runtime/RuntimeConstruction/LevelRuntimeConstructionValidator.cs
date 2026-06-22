using System;
using System.Collections.Generic;
using PuzzleFramework.Content;
using PuzzleFramework.CoreBoard;

namespace PuzzleFramework.RuntimeConstruction
{
    /// <summary>
    /// Build-safety validator for shared board construction inputs.
    /// It validates only the data required to construct runtime board state and must not absorb
    /// persistence/schema validation or gameplay-rule validation.
    /// </summary>
    public sealed class LevelRuntimeConstructionValidator : IRuntimeConstructionValidator
    {
        /// <inheritdoc />
        public RuntimeConstructionValidationResult Validate(LevelDefinition levelDefinition)
        {
            List<RuntimeConstructionValidationIssue> issues = new();

            if (levelDefinition == null)
            {
                issues.Add(new RuntimeConstructionValidationIssue(
                    "MissingLevelDefinition",
                    "Loaded level definition is required for runtime construction validation.",
                    null));
                return RuntimeConstructionValidationResult.Failed(issues);
            }

            if (levelDefinition.FrameworkData == null)
            {
                issues.Add(new RuntimeConstructionValidationIssue(
                    "MissingFrameworkData",
                    "Framework data is required for runtime construction validation.",
                    null));
                return RuntimeConstructionValidationResult.Failed(issues);
            }

            if (levelDefinition.FrameworkData.Board == null)
            {
                issues.Add(new RuntimeConstructionValidationIssue(
                    "MissingBoardData",
                    "Board data is required for runtime construction validation.",
                    null));
                return RuntimeConstructionValidationResult.Failed(issues);
            }

            if (levelDefinition.FrameworkData.Board.Cells == null)
            {
                issues.Add(new RuntimeConstructionValidationIssue(
                    "MissingBoardCells",
                    "Board cells collection is required for runtime construction validation.",
                    null));
                return RuntimeConstructionValidationResult.Failed(issues);
            }

            if (!TryBuildBoardData(
                    levelDefinition.FrameworkData.Board,
                    issues,
                    out RuntimeConstructionBoardData boardData))
            {
                return RuntimeConstructionValidationResult.Failed(issues);
            }

            return RuntimeConstructionValidationResult.Successful(boardData);
        }

        private static bool TryBuildBoardData(
            BoardDefinitionData boardDefinition,
            List<RuntimeConstructionValidationIssue> issues,
            out RuntimeConstructionBoardData boardData)
        {
            List<GridCoordinate> structuralCoordinates = new();
            List<GridCoordinate> blockedCoordinates = new();

            for (int i = 0; i < boardDefinition.Cells.Count; i++)
            {
                CellDefinitionData cell = boardDefinition.Cells[i];
                if (cell == null)
                {
                    issues.Add(new RuntimeConstructionValidationIssue(
                        "MissingCellEntry",
                        $"Board cell at index {i} is missing.",
                        null));
                    continue;
                }

                if (cell.Coordinate == null)
                {
                    issues.Add(new RuntimeConstructionValidationIssue(
                        "MissingCellCoordinate",
                        $"Board cell at index {i} is missing its coordinate.",
                        null));
                    continue;
                }

                GridCoordinate coordinate = new(cell.Coordinate.X, cell.Coordinate.Y);
                if (!Enum.IsDefined(typeof(AuthoredCellState), cell.CellState))
                {
                    issues.Add(new RuntimeConstructionValidationIssue(
                        "UnsupportedCellState",
                        $"Board cell at {coordinate} uses an unsupported authored cell state value.",
                        coordinate));
                    continue;
                }

                if (cell.CellState == AuthoredCellState.Inactive)
                {
                    continue;
                }

                structuralCoordinates.Add(coordinate);
                if (cell.CellState == AuthoredCellState.Blocked)
                {
                    blockedCoordinates.Add(coordinate);
                }
            }

            if (issues.Count > 0)
            {
                boardData = null;
                return false;
            }

            try
            {
                // Grid construction is used here strictly as a build-safety probe so runtime
                // construction can fail early before any builder or factory work begins.
                _ = new GridBoard(
                    boardDefinition.Width,
                    boardDefinition.Height,
                    structuralCoordinates,
                    blockedCoordinates);
            }
            catch (Exception exception)
            {
                issues.Add(new RuntimeConstructionValidationIssue(
                    "InvalidStructuralBoard",
                    $"Board data cannot be constructed into a runtime grid: {exception.Message}",
                    null));
                boardData = null;
                return false;
            }

            boardData = new RuntimeConstructionBoardData(
                boardDefinition.Width,
                boardDefinition.Height,
                structuralCoordinates,
                blockedCoordinates);

            return true;
        }
    }
}
