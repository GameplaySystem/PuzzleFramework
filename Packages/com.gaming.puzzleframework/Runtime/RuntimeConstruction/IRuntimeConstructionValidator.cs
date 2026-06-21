using PuzzleFramework.Content;

namespace PuzzleFramework.RuntimeConstruction
{
    /// <summary>
    /// Validates whether loaded authored content can be safely converted into runtime state.
    /// This boundary is build-safety only and does not own puzzle rules or object creation.
    /// </summary>
    public interface IRuntimeConstructionValidator
    {
        /// <summary>
        /// Evaluates build-readiness for a loaded level definition.
        /// </summary>
        RuntimeConstructionValidationResult Validate(LevelDefinition levelDefinition);
    }
}
