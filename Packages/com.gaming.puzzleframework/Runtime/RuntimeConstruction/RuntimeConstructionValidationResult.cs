using System;
using System.Collections.Generic;

namespace PuzzleFramework.RuntimeConstruction
{
    /// <summary>
    /// Result returned by the runtime construction validation entry point.
    /// </summary>
    public readonly struct RuntimeConstructionValidationResult
    {
        public RuntimeConstructionValidationResult(
            bool canBuild,
            RuntimeConstructionBoardData boardData,
            IReadOnlyList<RuntimeConstructionValidationIssue> issues)
        {
            CanBuild = canBuild;
            BoardData = boardData;
            Issues = issues ?? Array.Empty<RuntimeConstructionValidationIssue>();
        }

        /// <summary>
        /// True when runtime construction may safely proceed.
        /// </summary>
        public bool CanBuild { get; }

        /// <summary>
        /// Validated shared board construction data when <see cref="CanBuild"/> is true.
        /// Null when validation fails.
        /// </summary>
        public RuntimeConstructionBoardData BoardData { get; }

        /// <summary>
        /// Build-safety issues collected during validation.
        /// Empty when <see cref="CanBuild"/> is true.
        /// </summary>
        public IReadOnlyList<RuntimeConstructionValidationIssue> Issues { get; }

        public static RuntimeConstructionValidationResult Successful(
            RuntimeConstructionBoardData boardData)
        {
            return new RuntimeConstructionValidationResult(
                true,
                boardData,
                Array.Empty<RuntimeConstructionValidationIssue>());
        }

        public static RuntimeConstructionValidationResult Failed(
            IReadOnlyList<RuntimeConstructionValidationIssue> issues)
        {
            return new RuntimeConstructionValidationResult(false, null, issues);
        }
    }
}
