using UnityEngine;

namespace PuzzleFramework.Content
{
    /// <summary>
    /// Game-module adapter that extracts generic catalog metadata from an opaque level asset.
    /// Puzzle-specific parsing and validation remain behind this boundary.
    /// </summary>
    public interface ILevelCatalogMetadataReader
    {
        bool TryReadMetadata(
            TextAsset levelAsset,
            out LevelCatalogMetadata metadata,
            out string failureReason);
    }
}
