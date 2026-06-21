namespace PuzzleFramework.Content
{
    /// <summary>
    /// Persistence boundary for authored level definitions.
    /// Implementations save and load content data only; they do not build runtime objects.
    /// </summary>
    public interface ILevelSaveLoadService
    {
        /// <summary>
        /// Saves an authored level definition to the requested path.
        /// </summary>
        SaveResult Save(LevelDefinition levelDefinition, string path);

        /// <summary>
        /// Loads an authored level definition from the requested path.
        /// </summary>
        LoadResult<LevelDefinition> Load(string path);
    }
}
