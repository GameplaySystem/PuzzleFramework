using System;
using System.IO;
using NUnit.Framework;
using PuzzleFramework.Progression;

namespace PuzzleFramework.Tests
{
    public sealed class ProgressionSystemTests
    {
        private string _directory;
        private string _path;
        private JsonProgressSaveLoadService _store;

        [SetUp]
        public void SetUp()
        {
            _directory = Path.Combine(Path.GetTempPath(), "PuzzleProgressTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_directory);
            _path = Path.Combine(_directory, "progress.json");
            _store = new JsonProgressSaveLoadService();
        }

        [TearDown]
        public void TearDown() => Directory.Delete(_directory, true);

        [Test]
        public void CompletionIsIdempotentAndSnapshotDoesNotAliasState()
        {
            PlayerProgressData data = new();
            Assert.That(data.MarkCompleted("level-a"), Is.True);
            Assert.That(data.MarkCompleted("level-a"), Is.False);
            PlayerProgressSnapshot snapshot = data.CaptureSnapshot();
            snapshot.CompletedLevelIds[0] = "changed";
            Assert.That(data.IsCompleted("level-a"), Is.True);
            Assert.That(data.IsCompleted("changed"), Is.False);
            Assert.That(PlayerProgressData.TryRestore(data.CaptureSnapshot(), out var copy, out _), Is.True);
            copy.MarkCompleted("level-b");
            Assert.That(data.CompletedCount, Is.EqualTo(1));
        }

        [Test]
        public void RestoreRejectsDuplicateIdsAtomically()
        {
            var snapshot = new PlayerProgressSnapshot
            { FormatVersion = 1, CompletedLevelIds = new[] { "a", "a" }, ResumeLevelId = "b" };
            Assert.That(PlayerProgressData.TryRestore(snapshot, out var data, out _), Is.False);
            Assert.That(data, Is.Null);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(" ")]
        public void CompletionRejectsBlankIds(string id) =>
            Assert.Throws<ArgumentException>(() => new PlayerProgressData().MarkCompleted(id));

        [Test]
        public void MissingFileIsNotFound() => Assert.That(_store.Load(_path).Status, Is.EqualTo(ProgressLoadStatus.NotFound));

        [TestCase("")]
        [TestCase("not json")]
        [TestCase("{}")]
        [TestCase("{\"FormatVersion\":1}")]
        [TestCase("{\"FormatVersion\":1,\"CompletedLevelIds\":[]}")]
        [TestCase("{\"FormatVersion\":1,\"ResumeLevelId\":\"\"}")]
        [TestCase("{\"FormatVersion\":1,\"CompletedLevelIds\":[\"a\",\"a\"],\"ResumeLevelId\":\"b\"}")]
        public void InvalidDataDoesNotBecomeFreshProgress(string json)
        {
            File.WriteAllText(_path, json);
            Assert.That(_store.Load(_path).Status, Is.EqualTo(ProgressLoadStatus.InvalidData));
            Assert.That(_store.Load(_path).Progress, Is.Null);
            Assert.That(File.ReadAllText(_path), Is.EqualTo(json));
        }

        [Test]
        public void UnsupportedVersionIsDistinct()
        {
            File.WriteAllText(_path, "{\"FormatVersion\":99}");
            Assert.That(_store.Load(_path).Status, Is.EqualTo(ProgressLoadStatus.UnsupportedVersion));
        }

        [Test]
        public void FirstSaveReplacementAndColdLoadRoundTrip()
        {
            PlayerProgressData data = new();
            data.MarkCompleted("a"); data.SetResumeLevel("b");
            Assert.That(_store.Save(data, _path).Success, Is.True);
            data.MarkCompleted("b"); data.SetResumeLevel("c");
            var save = _store.Save(data, _path);
            Assert.That(save.Success, Is.True, save.FailureReason);
            var loaded = new JsonProgressSaveLoadService().Load(_path);
            Assert.That(loaded.Status, Is.EqualTo(ProgressLoadStatus.Loaded), loaded.FailureReason);
            Assert.That(loaded.Progress.CompletedCount, Is.EqualTo(2));
            Assert.That(loaded.Progress.ResumeLevelId, Is.EqualTo("c"));
            Assert.That(Directory.GetFiles(_directory, "*.tmp"), Is.Empty);
        }

        [Test]
        public void InvalidSaveDoesNotReplaceExistingFile()
        {
            File.WriteAllText(_path, "keep me");
            Assert.That(_store.Save(null, _path).Success, Is.False);
            Assert.That(File.ReadAllText(_path), Is.EqualTo("keep me"));
        }

        [Test]
        public void IoErrorsAreReportedAndTemporaryFilesAreRemoved()
        {
            Assert.That(_store.Load(_directory).Status, Is.EqualTo(ProgressLoadStatus.IoFailure));
            Assert.That(_store.Save(new PlayerProgressData(), _directory).Success, Is.False);
            Assert.That(Directory.GetFiles(Path.GetDirectoryName(_directory), Path.GetFileName(_directory) + ".*.tmp"), Is.Empty);
        }

        [Test]
        public void FailedReplacementPreservesPreviousSave()
        {
            PlayerProgressData data = new();
            Assert.That(_store.Save(data, _path).Success, Is.True);
            string before = File.ReadAllText(_path);
            using (var locked = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                data.MarkCompleted("a");
                Assert.That(_store.Save(data, _path).Success, Is.False);
            }
            Assert.That(File.ReadAllText(_path), Is.EqualTo(before));
            Assert.That(Directory.GetFiles(_directory, "*.tmp"), Is.Empty);
        }
    }
}
