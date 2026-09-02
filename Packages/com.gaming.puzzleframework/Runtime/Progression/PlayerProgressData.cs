using System;
using System.Collections.Generic;

namespace PuzzleFramework.Progression
{
    /// <summary>Versioned player-owned data, with no catalog or gameplay interpretation.</summary>
    [Serializable]
    public sealed class PlayerProgressSnapshot
    {
        public int FormatVersion;
        public string[] CompletedLevelIds;
        public string ResumeLevelId;
    }

    /// <summary>Owns completion history and a game-selected resume ID in memory, not persistence.</summary>
    public sealed class PlayerProgressData
    {
        public const int CurrentFormatVersion = 1;
        private readonly HashSet<string> _completed = new(StringComparer.Ordinal);
        public string ResumeLevelId { get; private set; } = string.Empty;
        public int CompletedCount => _completed.Count;

        public bool IsCompleted(string levelId) => levelId != null && _completed.Contains(levelId);

        /// <summary>Records an already-decided completion; duplicate reports are harmless.</summary>
        public bool MarkCompleted(string levelId)
        {
            RequireId(levelId);
            return _completed.Add(levelId);
        }

        /// <summary>The caller owns selection policy. An empty string clears the selection.</summary>
        public void SetResumeLevel(string levelId)
        {
            if (levelId != string.Empty) RequireId(levelId);
            ResumeLevelId = levelId;
        }

        public PlayerProgressSnapshot CaptureSnapshot()
        {
            string[] ids = new string[_completed.Count];
            _completed.CopyTo(ids);
            Array.Sort(ids, StringComparer.Ordinal);
            return new PlayerProgressSnapshot
            {
                FormatVersion = CurrentFormatVersion,
                CompletedLevelIds = ids,
                ResumeLevelId = ResumeLevelId
            };
        }

        /// <summary>Restores all or nothing. Validation is schema-only, not catalog availability.</summary>
        public static bool TryRestore(
            PlayerProgressSnapshot snapshot, out PlayerProgressData progress, out string failureReason)
        {
            progress = null;
            if (snapshot == null || snapshot.FormatVersion != CurrentFormatVersion ||
                snapshot.CompletedLevelIds == null || snapshot.ResumeLevelId == null ||
                (snapshot.ResumeLevelId.Length > 0 && string.IsNullOrWhiteSpace(snapshot.ResumeLevelId)))
            {
                failureReason = "Progress requires version 1, completed IDs and a valid resume ID field.";
                return false;
            }

            PlayerProgressData restored = new();
            foreach (string id in snapshot.CompletedLevelIds)
            {
                if (string.IsNullOrWhiteSpace(id) || !restored._completed.Add(id))
                {
                    failureReason = "Completed level IDs must be nonblank and unique.";
                    return false;
                }
            }
            restored.ResumeLevelId = snapshot.ResumeLevelId;
            progress = restored;
            failureReason = string.Empty;
            return true;
        }

        private static void RequireId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("A nonblank level ID is required.", nameof(id));
        }
    }
}
