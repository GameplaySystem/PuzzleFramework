namespace PuzzleFramework.Interaction
{
    /// <summary>
    /// Resolves a framework-safe interaction target from pointer context.
    /// Concrete raycast or hit-test behavior lives behind this boundary rather than inside the core interaction flow.
    /// </summary>
    public interface IInteractionTargetResolver
    {
        /// <summary>
        /// Resolves the current interaction target for the supplied pointer context.
        /// Returns null when no framework-interactable target is available.
        /// </summary>
        InteractionTarget ResolveTarget(InteractionPointerContext context);
    }
}
