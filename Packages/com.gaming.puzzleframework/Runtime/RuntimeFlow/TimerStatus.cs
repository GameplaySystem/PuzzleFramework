namespace PuzzleFramework.RuntimeFlow
{
    /// <summary>
    /// High-level timer runtime status owned by <see cref="TimerSystem"/>.
    /// This remains timer-specific and separate from overall game state.
    /// </summary>
    public enum TimerStatus
    {
        Stopped = 0,
        Running = 1,
        Paused = 2,
        Expired = 3
    }
}
