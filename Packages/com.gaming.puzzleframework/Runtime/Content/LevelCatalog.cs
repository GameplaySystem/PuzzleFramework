using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace PuzzleFramework.Content
{
    /// <summary>
    /// Generic identity and deterministic order supplied by a game-module metadata reader.
    /// </summary>
    public readonly struct LevelCatalogMetadata
    {
        public LevelCatalogMetadata(string levelId, int sequenceNumber)
        {
            LevelId = levelId ?? string.Empty;
            SequenceNumber = sequenceNumber;
        }

        public string LevelId { get; }

        public int SequenceNumber { get; }
    }

    /// <summary>
    /// One opaque authored level asset and its validated generic catalog metadata.
    /// </summary>
    public sealed class LevelCatalogEntry
    {
        internal LevelCatalogEntry(TextAsset asset, LevelCatalogMetadata metadata)
        {
            Asset = asset;
            LevelId = metadata.LevelId;
            SequenceNumber = metadata.SequenceNumber;
        }

        public TextAsset Asset { get; }

        public string LevelId { get; }

        public int SequenceNumber { get; }
    }

    /// <summary>
    /// Immutable, deterministically ordered collection of shipped authored level assets.
    /// </summary>
    public sealed class LevelCatalog
    {
        private readonly ReadOnlyCollection<LevelCatalogEntry> _entries;

        internal LevelCatalog(LevelCatalogEntry[] entries)
        {
            LevelCatalogEntry[] ownedEntries = entries == null
                ? Array.Empty<LevelCatalogEntry>()
                : (LevelCatalogEntry[])entries.Clone();
            _entries = Array.AsReadOnly(ownedEntries);
        }

        public int Count => _entries.Count;

        public IReadOnlyList<LevelCatalogEntry> Entries => _entries;

        public bool TryGetEntry(int index, out LevelCatalogEntry entry)
        {
            if (index < 0 || index >= _entries.Count)
            {
                entry = null;
                return false;
            }

            entry = _entries[index];
            return true;
        }
    }

    /// <summary>
    /// Atomic result of discovering and validating a level catalog.
    /// </summary>
    public readonly struct LevelCatalogBuildResult
    {
        private LevelCatalogBuildResult(
            bool success,
            LevelCatalog catalog,
            IReadOnlyList<string> warnings,
            string failureReason)
        {
            Success = success;
            Catalog = catalog;
            Warnings = warnings ?? Array.Empty<string>();
            FailureReason = failureReason ?? string.Empty;
        }

        public bool Success { get; }

        public LevelCatalog Catalog { get; }

        public IReadOnlyList<string> Warnings { get; }

        public string FailureReason { get; }

        public static LevelCatalogBuildResult Successful(
            LevelCatalog catalog,
            IReadOnlyList<string> warnings)
        {
            return new LevelCatalogBuildResult(true, catalog, warnings, string.Empty);
        }

        public static LevelCatalogBuildResult Failed(string failureReason)
        {
            return new LevelCatalogBuildResult(false, null, Array.Empty<string>(), failureReason);
        }
    }
}
