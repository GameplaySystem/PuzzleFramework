using System.Collections.Generic;
using NUnit.Framework;
using PuzzleFramework.Content;
using UnityEngine;

namespace PuzzleFramework.Tests
{
    public sealed class LevelCatalogBuilderTests
    {
        [Test]
        public void Build_OrdersAssetsBySequenceNumber()
        {
            TextAsset levelTwo = CreateAsset("Level 2", 2);
            TextAsset levelOne = CreateAsset("Level 1", 1);

            LevelCatalogBuildResult result = LevelCatalogBuilder.Build(
                new[] { levelTwo, levelOne },
                new TestMetadataReader());

            Assert.That(result.Success, Is.True, result.FailureReason);
            Assert.That(result.Catalog.Count, Is.EqualTo(2));
            Assert.That(result.Catalog.Entries[0].LevelId, Is.EqualTo("Level 1"));
            Assert.That(result.Catalog.Entries[1].LevelId, Is.EqualTo("Level 2"));
            Assert.That(result.Warnings, Is.Empty);
        }

        [Test]
        public void Build_RejectsDuplicateIdsIgnoringCase()
        {
            LevelCatalogBuildResult result = LevelCatalogBuilder.Build(
                new[]
                {
                    CreateAsset("Level 1", 1),
                    CreateAsset("level 1", 2)
                },
                new TestMetadataReader());

            Assert.That(result.Success, Is.False);
            StringAssert.Contains("duplicate level id", result.FailureReason);
        }

        [Test]
        public void Build_RejectsDuplicateSequenceNumbers()
        {
            LevelCatalogBuildResult result = LevelCatalogBuilder.Build(
                new[]
                {
                    CreateAsset("Level 1", 1),
                    CreateAsset("Level 2", 1)
                },
                new TestMetadataReader());

            Assert.That(result.Success, Is.False);
            StringAssert.Contains("duplicate sequence number", result.FailureReason);
        }

        [Test]
        public void Build_ReportsSequenceGapsWithoutRejectingCatalog()
        {
            LevelCatalogBuildResult result = LevelCatalogBuilder.Build(
                new[]
                {
                    CreateAsset("Level 2", 2),
                    CreateAsset("Level 4", 4)
                },
                new TestMetadataReader());

            Assert.That(result.Success, Is.True, result.FailureReason);
            Assert.That(result.Warnings, Has.Count.EqualTo(2));
            StringAssert.Contains("1", result.Warnings[0]);
            StringAssert.Contains("3", result.Warnings[1]);
        }

        [Test]
        public void Build_FailsAtomicallyWhenMetadataReaderRejectsAsset()
        {
            TextAsset invalidAsset = new("invalid") { name = "invalid_level" };

            LevelCatalogBuildResult result = LevelCatalogBuilder.Build(
                new[] { CreateAsset("Level 1", 1), invalidAsset },
                new TestMetadataReader());

            Assert.That(result.Success, Is.False);
            Assert.That(result.Catalog, Is.Null);
            StringAssert.Contains("invalid_level", result.FailureReason);
        }

        [Test]
        public void ResourcesLoader_RejectsEmptyPathBeforeDiscovery()
        {
            LevelCatalogBuildResult result = ResourcesLevelCatalogLoader.Load(
                " ",
                new TestMetadataReader());

            Assert.That(result.Success, Is.False);
            StringAssert.Contains("non-empty", result.FailureReason);
        }

        private static TextAsset CreateAsset(string levelId, int sequenceNumber)
        {
            return new TextAsset($"{levelId}|{sequenceNumber}")
            {
                name = levelId.Replace(' ', '_')
            };
        }

        private sealed class TestMetadataReader : ILevelCatalogMetadataReader
        {
            public bool TryReadMetadata(
                TextAsset levelAsset,
                out LevelCatalogMetadata metadata,
                out string failureReason)
            {
                string[] parts = levelAsset.text.Split('|');
                if (parts.Length != 2 || !int.TryParse(parts[1], out int sequenceNumber))
                {
                    metadata = default;
                    failureReason = "Test metadata is invalid.";
                    return false;
                }

                metadata = new LevelCatalogMetadata(parts[0], sequenceNumber);
                failureReason = string.Empty;
                return true;
            }
        }
    }
}
