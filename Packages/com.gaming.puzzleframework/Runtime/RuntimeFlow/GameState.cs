namespace PuzzleFramework.RuntimeFlow
{
    /// <summary>
    /// Shared framework-level lifecycle states for a single gameplay session.
    /// These states remain generic and do not encode puzzle-specific meaning.
    /// </summary>
    public enum GameState
    {
        NotStarted = 0,
        Playing = 1,
        Paused = 2,
        Won = 3,
        Lost = 4
    }
}
