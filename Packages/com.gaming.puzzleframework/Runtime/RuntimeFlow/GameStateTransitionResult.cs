namespace PuzzleFramework.RuntimeFlow
{
    /// <summary>
    /// Reports whether a requested lifecycle transition was accepted,
    /// whether state changed, and what state remains authoritative afterward.
    /// </summary>
    public readonly struct GameStateTransitionResult
    {
        public GameStateTransitionResult(
            bool isSuccess,
            bool stateChanged,
            GameState previousState,
            GameState currentState,
            GameState requestedState,
            string failureReason)
        {
            IsSuccess = isSuccess;
            StateChanged = stateChanged;
            PreviousState = previousState;
            CurrentState = currentState;
            RequestedState = requestedState;
            FailureReason = failureReason ?? string.Empty;
        }

        /// <summary>
        /// True when the requested transition was structurally accepted.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// True when the accepted transition changed the authoritative state.
        /// </summary>
        public bool StateChanged { get; }

        /// <summary>
        /// State before the transition attempt.
        /// </summary>
        public GameState PreviousState { get; }

        /// <summary>
        /// Authoritative state after the transition attempt.
        /// </summary>
        public GameState CurrentState { get; }

        /// <summary>
        /// Requested target state for the transition attempt.
        /// </summary>
        public GameState RequestedState { get; }

        /// <summary>
        /// Failure explanation when <see cref="IsSuccess"/> is false.
        /// </summary>
        public string FailureReason { get; }

        public static GameStateTransitionResult Successful(
            GameState previousState,
            GameState currentState,
            GameState requestedState,
            bool stateChanged)
        {
            return new GameStateTransitionResult(
                true,
                stateChanged,
                previousState,
                currentState,
                requestedState,
                string.Empty);
        }

        public static GameStateTransitionResult Failed(
            GameState currentState,
            GameState requestedState,
            string failureReason)
        {
            return new GameStateTransitionResult(
                false,
                false,
                currentState,
                currentState,
                requestedState,
                failureReason);
        }
    }
}
