namespace PuzzleFramework.RuntimeFlow
{
    /// <summary>
    /// Reports whether a timer control operation was accepted and what status
    /// remains authoritative afterward.
    /// </summary>
    public readonly struct TimerOperationResult
    {
        public TimerOperationResult(
            bool isSuccess,
            bool stateChanged,
            TimerStatus previousStatus,
            TimerStatus currentStatus,
            string failureReason)
        {
            IsSuccess = isSuccess;
            StateChanged = stateChanged;
            PreviousStatus = previousStatus;
            CurrentStatus = currentStatus;
            FailureReason = failureReason ?? string.Empty;
        }

        /// <summary>
        /// True when the requested operation was structurally valid.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// True when the accepted operation changed the authoritative timer status.
        /// </summary>
        public bool StateChanged { get; }

        /// <summary>
        /// Status before the operation attempt.
        /// </summary>
        public TimerStatus PreviousStatus { get; }

        /// <summary>
        /// Authoritative status after the operation attempt.
        /// </summary>
        public TimerStatus CurrentStatus { get; }

        /// <summary>
        /// Failure explanation when <see cref="IsSuccess"/> is false.
        /// </summary>
        public string FailureReason { get; }

        public static TimerOperationResult Successful(
            TimerStatus previousStatus,
            TimerStatus currentStatus,
            bool stateChanged)
        {
            return new TimerOperationResult(
                true,
                stateChanged,
                previousStatus,
                currentStatus,
                string.Empty);
        }

        public static TimerOperationResult Failed(
            TimerStatus currentStatus,
            string failureReason)
        {
            return new TimerOperationResult(
                false,
                false,
                currentStatus,
                currentStatus,
                failureReason);
        }
    }
}
