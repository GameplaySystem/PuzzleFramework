namespace PuzzleFramework.Interaction
{
    /// <summary>
    /// Minimal input intent coordinator for the framework interaction model.
    /// This first slice owns single-selection state and direct capability calls only.
    /// Concrete raycasting, concrete UI queries, and platform-specific input polling remain separate next steps.
    /// </summary>
    public sealed class InputSystem
    {
        private readonly IInteractionTargetResolver _targetResolver;
        private readonly IInteractionUiBlocker _uiBlocker;

        public InputSystem()
        {
        }

        public InputSystem(
            IInteractionTargetResolver targetResolver,
            IInteractionUiBlocker uiBlocker)
        {
            _targetResolver = targetResolver;
            _uiBlocker = uiBlocker;
        }

        /// <summary>
        /// Target currently selected by the active pointer interaction, if any.
        /// </summary>
        public InteractionTarget CurrentTarget { get; private set; }

        /// <summary>
        /// Most recent pointer context observed by the input system.
        /// </summary>
        public InteractionPointerContext CurrentPointerContext { get; private set; }

        /// <summary>
        /// True while the input system considers the pointer pressed.
        /// </summary>
        public bool IsPointerPressed { get; private set; }

        /// <summary>
        /// True while the current interaction target is in drag flow.
        /// </summary>
        public bool IsDragActive { get; private set; }

        /// <summary>
        /// Resolves and begins a pointer press through the configured target resolver and UI blocker.
        /// Returns true when world interaction actually starts.
        /// </summary>
        public bool TryBeginPointerPress(InteractionPointerContext context)
        {
            if (_targetResolver == null)
            {
                throw new System.InvalidOperationException(
                    "InputSystem requires an interaction target resolver for resolved pointer press flow.");
            }

            CurrentPointerContext = context;

            if (_uiBlocker != null && _uiBlocker.IsBlockingWorldInteraction(context))
            {
                return false;
            }

            InteractionTarget target = _targetResolver.ResolveTarget(context);
            BeginPointerPress(context, target);
            return target != null;
        }

        /// <summary>
        /// Begins a new pointer interaction against the resolved framework-safe target.
        /// Passing a null target results in no selection.
        /// </summary>
        public void BeginPointerPress(
            InteractionPointerContext context,
            InteractionTarget target)
        {
            if (CurrentTarget != null)
            {
                ClearCurrentInteraction(context, completeInteraction: false);
            }

            CurrentPointerContext = context;
            IsPointerPressed = true;
            CurrentTarget = target;

            if (CurrentTarget == null)
            {
                return;
            }

            CurrentTarget.Selectable?.OnSelected(context);

            if (CurrentTarget.Draggable == null)
            {
                return;
            }

            IsDragActive = true;
            CurrentTarget.Draggable.OnDragStart(context);
        }

        /// <summary>
        /// Updates pointer context during an active interaction.
        /// Drag capability receives updates only while drag flow is active.
        /// </summary>
        public void UpdatePointer(InteractionPointerContext context)
        {
            CurrentPointerContext = context;

            if (!IsPointerPressed || !IsDragActive || CurrentTarget?.Draggable == null)
            {
                return;
            }

            CurrentTarget.Draggable.OnDrag(context);
        }

        /// <summary>
        /// Ends the current pointer interaction and forwards the appropriate capability callbacks.
        /// </summary>
        public void EndPointerPress(InteractionPointerContext context)
        {
            CurrentPointerContext = context;
            ClearCurrentInteraction(context, completeInteraction: true);
        }

        private void ClearCurrentInteraction(
            InteractionPointerContext context,
            bool completeInteraction)
        {
            InteractionTarget target = CurrentTarget;
            bool dragWasActive = IsDragActive;

            if (target == null)
            {
                IsPointerPressed = false;
                IsDragActive = false;
                return;
            }

            if (completeInteraction)
            {
                if (dragWasActive && target.Draggable != null)
                {
                    target.Draggable.OnDragEnd(context);
                }
                else
                {
                    target.Clickable?.OnClicked(context);
                }
            }

            target.Selectable?.OnDeselected(context);

            CurrentTarget = null;
            IsPointerPressed = false;
            IsDragActive = false;
        }
    }
}
