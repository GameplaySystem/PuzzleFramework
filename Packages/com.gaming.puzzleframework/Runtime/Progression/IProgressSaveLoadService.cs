namespace PuzzleFramework.Progression
{
    public enum ProgressLoadStatus
    {
        Loaded,
        NotFound,
        InvalidData,
        UnsupportedVersion,
        IoFailure
    }

    public readonly struct ProgressLoadResult
    {
        public ProgressLoadResult(ProgressLoadStatus status, PlayerProgressData progress, string reason = "")
        {
            Status = status;
            Progress = progress;
            FailureReason = reason ?? string.Empty;
        }
        public ProgressLoadStatus Status { get; }
        public PlayerProgressData Progress { get; }
        public string FailureReason { get; }
    }

    public readonly struct ProgressSaveResult
    {
        public ProgressSaveResult(bool success, string reason = "")
        {
            Success = success;
            FailureReason = reason ?? string.Empty;
        }
        public bool Success { get; }
        public string FailureReason { get; }
    }

    /// <summary>Persists player-owned state at a caller-selected path; never loads authored levels.</summary>
    public interface IProgressSaveLoadService
    {
        ProgressLoadResult Load(string path);
        ProgressSaveResult Save(PlayerProgressData progress, string path);
    }
}
