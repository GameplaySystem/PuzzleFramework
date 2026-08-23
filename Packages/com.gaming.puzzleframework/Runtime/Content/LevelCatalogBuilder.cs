using System;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleFramework.Content
{
    /// <summary>
    /// Validates and orders opaque level assets using game-module-supplied metadata.
    /// </summary>
    public static class LevelCatalogBuilder
    {
        public static LevelCatalogBuildResult Build(
            IReadOnlyList<TextAsset> levelAssets,
            ILevelCatalogMetadataReader metadataReader)
        {
            if (metadataReader == null)
            {
                return LevelCatalogBuildResult.Failed(
                    "A level catalog metadata reader is required.");
            }

            if (levelAssets == null || levelAssets.Count == 0)
            {
                return LevelCatalogBuildResult.Failed(
                    "At least one authored level asset is required to build a catalog.");
            }

            List<LevelCatalogEntry> entries = new(levelAssets.Count);
            HashSet<string> levelIds = new(StringComparer.OrdinalIgnoreCase);
            HashSet<int> sequenceNumbers = new();

            for (int i = 0; i < levelAssets.Count; i++)
            {
                TextAsset levelAsset = levelAssets[i];
                if (levelAsset == null)
                {
                    return LevelCatalogBuildResult.Failed(
                        $"Level catalog asset at index {i} is null.");
                }

                LevelCatalogMetadata metadata;
                string failureReason;
                try
                {
                    if (!metadataReader.TryReadMetadata(
                            levelAsset,
                            out metadata,
                            out failureReason))
                    {
                        return LevelCatalogBuildResult.Failed(
                            $"Level asset '{levelAsset.name}' is invalid: {failureReason}");
                    }
                }
                catch (Exception exception)
                {
                    return LevelCatalogBuildResult.Failed(
                        $"Level metadata reader failed for asset '{levelAsset.name}': " +
                        exception.Message);
                }

                string levelId = metadata.LevelId?.Trim();
                if (string.IsNullOrEmpty(levelId))
                {
                    return LevelCatalogBuildResult.Failed(
                        $"Level asset '{levelAsset.name}' has an empty level id.");
                }

                if (metadata.SequenceNumber <= 0)
                {
                    return LevelCatalogBuildResult.Failed(
                        $"Level asset '{levelAsset.name}' has non-positive sequence number " +
                        $"{metadata.SequenceNumber}.");
                }

                if (!levelIds.Add(levelId))
                {
                    return LevelCatalogBuildResult.Failed(
                        $"Level catalog contains duplicate level id '{levelId}'.");
                }

                if (!sequenceNumbers.Add(metadata.SequenceNumber))
                {
                    return LevelCatalogBuildResult.Failed(
                        $"Level catalog contains duplicate sequence number " +
                        $"{metadata.SequenceNumber}.");
                }

                entries.Add(
                    new LevelCatalogEntry(
                        levelAsset,
                        new LevelCatalogMetadata(levelId, metadata.SequenceNumber)));
            }

            entries.Sort((left, right) => left.SequenceNumber.CompareTo(right.SequenceNumber));
            List<string> warnings = BuildSequenceWarnings(entries);
            return LevelCatalogBuildResult.Successful(
                new LevelCatalog(entries.ToArray()),
                warnings.ToArray());
        }

        private static List<string> BuildSequenceWarnings(
            IReadOnlyList<LevelCatalogEntry> entries)
        {
            List<string> warnings = new();
            int expectedSequence = 1;

            for (int i = 0; i < entries.Count; i++)
            {
                int actualSequence = entries[i].SequenceNumber;
                if (actualSequence > expectedSequence)
                {
                    warnings.Add(
                        $"Level catalog sequence is missing {FormatRange(expectedSequence, actualSequence - 1)}.");
                }

                expectedSequence = actualSequence + 1;
            }

            return warnings;
        }

        private static string FormatRange(int first, int last)
        {
            return first == last ? first.ToString() : $"{first}-{last}";
        }
    }
}
