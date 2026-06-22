using PuzzleFramework.Content;

namespace PuzzleFramework.RuntimeConstruction
{
    /// <summary>
    /// Coordinates conversion of loaded authored content into framework-owned runtime state.
    /// This boundary owns construction flow, not gameplay meaning or low-level object factories.
    /// </summary>
    public interface ILevelRuntimeBuilder
    {
        /// <summary>
        /// Builds the shared runtime level context for a loaded level definition.
        /// </summary>
        RuntimeLevelBuildResult Build(LevelDefinition levelDefinition);
    }
}
