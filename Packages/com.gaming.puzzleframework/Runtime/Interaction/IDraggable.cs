namespace PuzzleFramework.Interaction
{
    /// <summary>
    /// Capability contract for targets that participate in framework-owned drag flow.
    /// The framework forwards pointer intent; movement meaning remains elsewhere.
    /// </summary>
    public interface IDraggable
    {
        /// <summary>
        /// Called when drag interaction begins for this target.
        /// </summary>
        void OnDragStart(InteractionPointerContext context);

        /// <summary>
        /// Called for drag updates while this target remains the active drag selection.
        /// </summary>
        void OnDrag(InteractionPointerContext context);

        /// <summary>
        /// Called when drag interaction ends for this target.
        /// </summary>
        void OnDragEnd(InteractionPointerContext context);
    }
}
