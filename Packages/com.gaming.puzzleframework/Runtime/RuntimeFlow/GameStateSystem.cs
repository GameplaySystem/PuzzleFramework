namespace PuzzleFramework.RuntimeFlow
{
    /// <summary>
    /// Minimal framework-level owner of high-level gameplay phase.
    /// This slice preserves lifecycle integrity only; event publication is deferred
    /// until a real multi-listener notification need appears during MVP.
    /// </summary>
    public sealed class GameStateSystem
    {
        public GameStateSystem()
        {
            CurrentState = GameState.NotStarted;
        }

        /// <summary>
        /// Authoritative runtime lifecycle state.
        /// </summary>
        public GameState CurrentState { get; private set; }

        /// <summary>
        /// Returns true when the requested lifecycle transition is structurally valid.
        /// </summary>
        public bool CanTransitionTo(GameState targetState)
        {
            if (targetState == CurrentState)
            {
                return true;
            }

            return CurrentState switch
            {
                GameState.NotStarted => targetState == GameState.Playing,
                GameState.Playing => targetState == GameState.NotStarted ||
                                     targetState == GameState.Paused ||
                                     targetState == GameState.Won ||
                                     targetState == GameState.Lost,
                GameState.Paused => targetState == GameState.NotStarted ||
                                    targetState == GameState.Playing,
                GameState.Won => targetState == GameState.NotStarted,
                GameState.Lost => targetState == GameState.NotStarted,
                _ => false
            };
        }

        /// <summary>
        /// Attempts a lifecycle transition while preserving the current state on failure.
        /// Duplicate requests succeed as no-op transitions to avoid redundant side effects.
        /// </summary>
        public GameStateTransitionResult TryTransitionTo(GameState targetState)
        {
            if (targetState == CurrentState)
            {
                return GameStateTransitionResult.Successful(
                    CurrentState,
                    CurrentState,
                    targetState,
                    false);
            }

            if (!CanTransitionTo(targetState))
            {
                return GameStateTransitionResult.Failed(
                    CurrentState,
                    targetState,
                    $"Game state cannot transition from {CurrentState} to {targetState}.");
            }

            GameState previousState = CurrentState;
            CurrentState = targetState;

            return GameStateTransitionResult.Successful(
                previousState,
                CurrentState,
                targetState,
                true);
        }
    }
}
