using UnityEngine;

namespace PuzzleFramework.Content
{
    /// <summary>
    /// Unity Resources adapter for the generic level catalog builder.
    /// The path is relative to a Resources folder and excludes file extensions.
    /// </summary>
    public static class ResourcesLevelCatalogLoader
    {
        public static LevelCatalogBuildResult Load(
            string resourcesPath,
            ILevelCatalogMetadataReader metadataReader)
        {
            string normalizedPath = NormalizePath(resourcesPath);
            if (string.IsNullOrEmpty(normalizedPath))
            {
                return LevelCatalogBuildResult.Failed(
                    "A non-empty Resources-relative level catalog path is required.");
            }

            TextAsset[] levelAssets = Resources.LoadAll<TextAsset>(normalizedPath);
            if (levelAssets == null || levelAssets.Length == 0)
            {
                return LevelCatalogBuildResult.Failed(
                    $"No authored level TextAssets were found at Resources path " +
                    $"'{normalizedPath}'.");
            }

            return LevelCatalogBuilder.Build(levelAssets, metadataReader);
        }

        private static string NormalizePath(string resourcesPath)
        {
            return string.IsNullOrWhiteSpace(resourcesPath)
                ? string.Empty
                : resourcesPath.Trim().Replace('\\', '/').Trim('/');
        }
    }
}
