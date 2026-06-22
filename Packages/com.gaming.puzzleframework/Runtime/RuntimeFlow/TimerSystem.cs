using System;
using UnityEngine;

namespace PuzzleFramework.RuntimeFlow
{
    /// <summary>
    /// Minimal countdown timer runtime foundation for the MVP.
    /// It owns generic time tracking and raises warning or expiration facts through
    /// explicit result data instead of a broad event layer.
    /// </summary>
    public sealed class TimerSystem
    {
        public TimerSystem(float durationSeconds, float? warningThresholdSeconds = null)
        {
            if (durationSeconds <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(durationSeconds),
                    "Timer duration must be positive.");
            }

            if (warningThresholdSeconds.HasValue &&
                (warningThresholdSeconds.Value < 0f ||
                 warningThresholdSeconds.Value > durationSeconds))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(warningThresholdSeconds),
                    "Timer warning threshold must be between zero and the full duration.");
            }

            DurationSeconds = durationSeconds;
            WarningThresholdSeconds = warningThresholdSeconds;
            Status = TimerStatus.Stopped;
        }

        /// <summary>
        /// Fixed countdown duration configured for the timer instance.
        /// </summary>
        public float DurationSeconds { get; }

        /// <summary>
        /// Optional remaining-time threshold that raises one warning fact when crossed.
        /// </summary>
        public float? WarningThresholdSeconds { get; }

        /// <summary>
        /// Authoritative timer runtime status.
        /// </summary>
        public TimerStatus Status { get; private set; }

        /// <summary>
        /// Elapsed seconds accumulated since the latest start.
        /// </summary>
        public float ElapsedSeconds { get; private set; }

        /// <summary>
        /// Remaining countdown seconds clamped to zero.
        /// </summary>
        public float RemainingSeconds => Mathf.Max(0f, DurationSeconds - ElapsedSeconds);

        /// <summary>
        /// True after the timer has raised its one-shot warning fact.
        /// </summary>
        public bool HasRaisedWarning { get; private set; }

        /// <summary>
        /// True after the timer has reached expiration.
        /// </summary>
        public bool HasExpired => Status == TimerStatus.Expired;

        /// <summary>
        /// Starts the timer from a clean countdown state.
        /// </summary>
        public TimerOperationResult Start()
        {
            if (Status != TimerStatus.Stopped)
            {
                return TimerOperationResult.Failed(
                    Status,
                    $"Timer cannot start while status is {Status}.");
            }

            TimerStatus previousStatus = Status;
            ElapsedSeconds = 0f;
            HasRaisedWarning = false;
            Status = TimerStatus.Running;

            return TimerOperationResult.Successful(previousStatus, Status, true);
        }

        /// <summary>
        /// Stops the timer without resetting its current elapsed state.
        /// </summary>
        public TimerOperationResult Stop()
        {
            if (Status == TimerStatus.Stopped)
            {
                return TimerOperationResult.Successful(Status, Status, false);
            }

            TimerStatus previousStatus = Status;
            Status = TimerStatus.Stopped;
            return TimerOperationResult.Successful(previousStatus, Status, true);
        }

        /// <summary>
        /// Pauses the timer so advancement stops until resumed.
        /// </summary>
        public TimerOperationResult Pause()
        {
            if (Status != TimerStatus.Running)
            {
                return TimerOperationResult.Failed(
                    Status,
                    $"Timer cannot pause while status is {Status}.");
            }

            TimerStatus previousStatus = Status;
            Status = TimerStatus.Paused;
            return TimerOperationResult.Successful(previousStatus, Status, true);
        }

        /// <summary>
        /// Resumes a previously paused timer without resetting elapsed progress.
        /// </summary>
        public TimerOperationResult Resume()
        {
            if (Status != TimerStatus.Paused)
            {
                return TimerOperationResult.Failed(
                    Status,
                    $"Timer cannot resume while status is {Status}.");
            }

            TimerStatus previousStatus = Status;
            Status = TimerStatus.Running;
            return TimerOperationResult.Successful(previousStatus, Status, true);
        }

        /// <summary>
        /// Clears elapsed progress and returns the timer to a stopped pre-start state.
        /// </summary>
        public TimerOperationResult Reset()
        {
            TimerStatus previousStatus = Status;
            ElapsedSeconds = 0f;
            HasRaisedWarning = false;
            Status = TimerStatus.Stopped;
            return TimerOperationResult.Successful(previousStatus, Status, previousStatus != Status);
        }

        /// <summary>
        /// Advances the countdown while running and returns one-shot warning or expiration facts.
        /// </summary>
        public TimerAdvanceResult Advance(float deltaTimeSeconds)
        {
            if (deltaTimeSeconds < 0f)
            {
                return TimerAdvanceResult.Failed(
                    Status,
                    ElapsedSeconds,
                    RemainingSeconds,
                    "Timer delta time must be zero or greater.");
            }

            if (Status != TimerStatus.Running)
            {
                return TimerAdvanceResult.Failed(
                    Status,
                    ElapsedSeconds,
                    RemainingSeconds,
                    $"Timer cannot advance while status is {Status}.");
            }

            TimerStatus previousStatus = Status;
            float previousRemainingSeconds = RemainingSeconds;

            ElapsedSeconds = Mathf.Min(DurationSeconds, ElapsedSeconds + deltaTimeSeconds);

            bool warningRaised = false;
            if (WarningThresholdSeconds.HasValue &&
                !HasRaisedWarning &&
                previousRemainingSeconds >= WarningThresholdSeconds.Value &&
                previousRemainingSeconds != RemainingSeconds &&
                RemainingSeconds <= WarningThresholdSeconds.Value)
            {
                HasRaisedWarning = true;
                warningRaised = true;
            }

            bool expiredRaised = false;
            if (ElapsedSeconds >= DurationSeconds)
            {
                Status = TimerStatus.Expired;
                expiredRaised = true;
            }

            return TimerAdvanceResult.Successful(
                previousStatus,
                Status,
                ElapsedSeconds,
                RemainingSeconds,
                warningRaised,
                expiredRaised);
        }
    }
}
