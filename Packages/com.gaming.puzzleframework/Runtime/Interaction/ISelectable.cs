namespace PuzzleFramework.Interaction
{
    /// <summary>
    /// Capability contract for targets that participate in framework-owned selection flow.
    /// Implementers decide what selected or deselected means in their own layer.
    /// </summary>
    public interface ISelectable
    {
        /// <summary>
        /// Called when the input system selects this target.
        /// </summary>
        void OnSelected(InteractionPointerContext context);

        /// <summary>
        /// Called when the input system clears selection for this target.
        /// </summary>
        void OnDeselected(InteractionPointerContext context);
    }
}
