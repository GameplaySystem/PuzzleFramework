namespace PuzzleFramework.CoreBoard
{
    /// <summary>
    /// Result for occupancy operations that mutate structural cell usage state.
    /// </summary>
    public readonly struct CellOccupancyOperationResult
    {
        public CellOccupancyOperationResult(bool success, string failureReason)
        {
            Success = success;
            FailureReason = failureReason ?? string.Empty;
        }

        /// <summary>
        /// True when the requested occupancy operation completed successfully.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Failure explanation for callers when <see cref="Success"/> is false.
        /// </summary>
        public string FailureReason { get; }

        public static CellOccupancyOperationResult Successful()
        {
            return new CellOccupancyOperationResult(true, string.Empty);
        }

        public static CellOccupancyOperationResult Failed(string failureReason)
        {
            return new CellOccupancyOperationResult(false, failureReason);
        }
    }

    /// <summary>
    /// Result for structural footprint overlap checks.
    /// </summary>
    public readonly struct FootprintAvailabilityResult
    {
        public FootprintAvailabilityResult(
            bool canAccept,
            GridCoordinate? blockingCoordinate,
            string failureReason)
        {
            CanAccept = canAccept;
            BlockingCoordinate = blockingCoordinate;
            FailureReason = failureReason ?? string.Empty;
        }

        /// <summary>
        /// True when the full footprint can be accepted structurally.
        /// </summary>
        public bool CanAccept { get; }

        /// <summary>
        /// Coordinate that caused the first structural rejection, when available.
        /// </summary>
        public GridCoordinate? BlockingCoordinate { get; }

        /// <summary>
        /// Failure explanation for callers when <see cref="CanAccept"/> is false.
        /// </summary>
        public string FailureReason { get; }

        public static FootprintAvailabilityResult Accepted()
        {
            return new FootprintAvailabilityResult(true, null, string.Empty);
        }

        public static FootprintAvailabilityResult Rejected(
            GridCoordinate? blockingCoordinate,
            string failureReason)
        {
            return new FootprintAvailabilityResult(false, blockingCoordinate, failureReason);
        }
    }
}
