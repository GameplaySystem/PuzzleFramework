using System;

namespace PuzzleFramework.Interaction
{
    /// <summary>
    /// Framework-safe capability bundle resolved by input detection.
    /// This allows the input system to work with interaction capabilities without knowing concrete object types.
    /// </summary>
    public sealed class InteractionTarget
    {
        public InteractionTarget(
            ISelectable selectable = null,
            IClickable clickable = null,
            IDraggable draggable = null)
        {
            if (selectable == null && clickable == null && draggable == null)
            {
                throw new ArgumentException(
                    "Interaction target must expose at least one interaction capability.");
            }

            Selectable = selectable;
            Clickable = clickable;
            Draggable = draggable;
        }

        /// <summary>
        /// Optional selection capability for this interaction target.
        /// </summary>
        public ISelectable Selectable { get; }

        /// <summary>
        /// Optional click capability for this interaction target.
        /// </summary>
        public IClickable Clickable { get; }

        /// <summary>
        /// Optional drag capability for this interaction target.
        /// </summary>
        public IDraggable Draggable { get; }
    }
}
