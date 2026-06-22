namespace PuzzleFramework.Interaction
{
    /// <summary>
    /// Capability contract for targets that respond to framework-owned click intent.
    /// This communicates intent only and does not define gameplay consequences.
    /// </summary>
    public interface IClickable
    {
        /// <summary>
        /// Called when the input system resolves a click interaction on this target.
        /// </summary>
        void OnClicked(InteractionPointerContext context);
    }
}
