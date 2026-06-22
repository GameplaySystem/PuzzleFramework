namespace PuzzleFramework.RuntimeConstruction
{
    /// <summary>
    /// Result contract for a future level runtime builder.
    /// This wraps the shared runtime handoff context without defining orchestration behavior.
    /// </summary>
    public readonly struct RuntimeLevelBuildResult
    {
        public RuntimeLevelBuildResult(
            bool success,
            RuntimeLevelContext context,
            string failureReason)
        {
            Success = success;
            Context = context;
            FailureReason = failureReason ?? string.Empty;
        }

        /// <summary>
        /// True when runtime construction completed successfully.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Shared runtime handoff context when construction succeeds.
        /// Null when <see cref="Success"/> is false.
        /// </summary>
        public RuntimeLevelContext Context { get; }

        /// <summary>
        /// Failure explanation for callers when runtime construction does not succeed.
        /// </summary>
        public string FailureReason { get; }

        public static RuntimeLevelBuildResult Successful(RuntimeLevelContext context)
        {
            return new RuntimeLevelBuildResult(true, context, string.Empty);
        }

        public static RuntimeLevelBuildResult Failed(string failureReason)
        {
            return new RuntimeLevelBuildResult(false, null, failureReason);
        }
    }
}
