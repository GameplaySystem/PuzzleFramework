namespace PuzzleFramework.RuntimeFlow
{
    /// <summary>
    /// Reports the outcome of a timer advancement step, including whether the
    /// warning or expiration facts were raised during this update.
    /// </summary>
    public readonly struct TimerAdvanceResult
    {
        public TimerAdvanceResult(
            bool isSuccess,
            TimerStatus previousStatus,
            TimerStatus currentStatus,
            float elapsedSeconds,
            float remainingSeconds,
            bool warningRaised,
            bool expiredRaised,
            string failureReason)
        {
            IsSuccess = isSuccess;
            PreviousStatus = previousStatus;
            CurrentStatus = currentStatus;
            ElapsedSeconds = elapsedSeconds;
            RemainingSeconds = remainingSeconds;
            WarningRaised = warningRaised;
            ExpiredRaised = expiredRaised;
            FailureReason = failureReason ?? string.Empty;
        }

        /// <summary>
        /// True when the timer accepted the advancement step.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Status before the advancement step.
        /// </summary>
        public TimerStatus PreviousStatus { get; }

        /// <summary>
        /// Status after the advancement step.
        /// </summary>
        public TimerStatus CurrentStatus { get; }

        /// <summary>
        /// Total elapsed seconds after the advancement step.
        /// </summary>
        public float ElapsedSeconds { get; }

        /// <summary>
        /// Remaining seconds after the advancement step.
        /// </summary>
        public float RemainingSeconds { get; }

        /// <summary>
        /// True only on the update where the warning threshold is crossed.
        /// </summary>
        public bool WarningRaised { get; }

        /// <summary>
        /// True only on the update where expiration first occurs.
        /// </summary>
        public bool ExpiredRaised { get; }

        /// <summary>
        /// Failure explanation when <see cref="IsSuccess"/> is false.
        /// </summary>
        public string FailureReason { get; }

        public static TimerAdvanceResult Successful(
            TimerStatus previousStatus,
            TimerStatus currentStatus,
            float elapsedSeconds,
            float remainingSeconds,
            bool warningRaised,
            bool expiredRaised)
        {
            return new TimerAdvanceResult(
                true,
                previousStatus,
                currentStatus,
                elapsedSeconds,
                remainingSeconds,
                warningRaised,
                expiredRaised,
                string.Empty);
        }

        public static TimerAdvanceResult Failed(
            TimerStatus currentStatus,
            float elapsedSeconds,
            float remainingSeconds,
            string failureReason)
        {
            return new TimerAdvanceResult(
                false,
                currentStatus,
                currentStatus,
                elapsedSeconds,
                remainingSeconds,
                false,
                false,
                failureReason);
        }
    }
}
