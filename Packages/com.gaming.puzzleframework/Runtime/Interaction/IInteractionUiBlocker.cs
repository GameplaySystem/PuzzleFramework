namespace PuzzleFramework.Interaction
{
    /// <summary>
    /// Determines whether pointer input should be prevented from starting world interaction.
    /// This keeps UI gating separate from core input intent flow.
    /// </summary>
    public interface IInteractionUiBlocker
    {
        /// <summary>
        /// Returns true when the pointer context should not start world interaction.
        /// </summary>
        bool IsBlockingWorldInteraction(InteractionPointerContext context);
    }
}
