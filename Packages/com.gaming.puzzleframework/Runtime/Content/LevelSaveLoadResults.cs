namespace PuzzleFramework.Content
{
    /// <summary>
    /// Result for persistence operations that do not return data.
    /// </summary>
    public readonly struct SaveResult
    {
        public SaveResult(bool success, string failureReason)
        {
            Success = success;
            FailureReason = failureReason ?? string.Empty;
        }

        /// <summary>
        /// True when the save operation completed successfully.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Failure explanation for callers and editor tooling.
        /// Empty when <see cref="Success"/> is true.
        /// </summary>
        public string FailureReason { get; }

        public static SaveResult Successful()
        {
            return new SaveResult(true, string.Empty);
        }

        public static SaveResult Failed(string failureReason)
        {
            return new SaveResult(false, failureReason);
        }
    }

    /// <summary>
    /// Result for persistence operations that return a data payload.
    /// </summary>
    public readonly struct LoadResult<T>
    {
        public LoadResult(bool success, T data, string failureReason)
        {
            Success = success;
            Data = data;
            FailureReason = failureReason ?? string.Empty;
        }

        /// <summary>
        /// True when the load operation completed successfully.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Loaded data when successful.
        /// </summary>
        public T Data { get; }

        /// <summary>
        /// Failure explanation for callers and editor tooling.
        /// Empty when <see cref="Success"/> is true.
        /// </summary>
        public string FailureReason { get; }

        public static LoadResult<T> Successful(T data)
        {
            return new LoadResult<T>(true, data, string.Empty);
        }

        public static LoadResult<T> Failed(string failureReason)
        {
            return new LoadResult<T>(false, default, failureReason);
        }
    }
}
