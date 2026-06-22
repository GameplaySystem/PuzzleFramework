using System;
using System.Text;
using PuzzleFramework.Content;
using PuzzleFramework.CoreBoard;

namespace PuzzleFramework.RuntimeConstruction
{
    /// <summary>
    /// Minimal runtime builder that validates first, initializes shared board systems,
    /// and returns a framework-owned runtime context.
    /// This first slice intentionally stops before any object factory or gameplay object creation.
    /// </summary>
    public sealed class LevelRuntimeBuilder : ILevelRuntimeBuilder
    {
        private readonly IRuntimeConstructionValidator _validator;

        public LevelRuntimeBuilder(IRuntimeConstructionValidator validator)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        }

        /// <inheritdoc />
        public RuntimeLevelBuildResult Build(LevelDefinition levelDefinition)
        {
            RuntimeConstructionValidationResult validationResult = _validator.Validate(levelDefinition);
            if (!validationResult.CanBuild || validationResult.BoardData == null)
            {
                return RuntimeLevelBuildResult.Failed(BuildValidationFailureReason(validationResult));
            }

            try
            {
                RuntimeLevelContext context = BuildRuntimeLevelContext(validationResult.BoardData);
                return RuntimeLevelBuildResult.Successful(context);
            }
            catch (Exception exception)
            {
                return RuntimeLevelBuildResult.Failed(
                    $"Failed to build runtime level context: {exception.Message}");
            }
        }

        private static RuntimeLevelContext BuildRuntimeLevelContext(
            RuntimeConstructionBoardData boardData)
        {
            GridBoard gridBoard = new(
                boardData.Width,
                boardData.Height,
                boardData.StructuralCoordinates,
                boardData.BlockedCoordinates);

            CellOccupancySystem cellOccupancySystem = new(gridBoard);

            return new RuntimeLevelContext(boardData, gridBoard, cellOccupancySystem);
        }

        private static string BuildValidationFailureReason(
            RuntimeConstructionValidationResult validationResult)
        {
            if (validationResult.Issues == null || validationResult.Issues.Count == 0)
            {
                return "Runtime construction validation failed.";
            }

            StringBuilder builder = new();
            builder.Append("Runtime construction validation failed:");

            for (int i = 0; i < validationResult.Issues.Count; i++)
            {
                RuntimeConstructionValidationIssue issue = validationResult.Issues[i];
                builder.Append(' ');
                builder.Append('[');
                builder.Append(issue.Code);
                builder.Append("] ");
                builder.Append(issue.Message);
            }

            return builder.ToString();
        }
    }
}
