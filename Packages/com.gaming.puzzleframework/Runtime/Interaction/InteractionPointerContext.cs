using UnityEngine;

namespace PuzzleFramework.Interaction
{
    /// <summary>
    /// Shared pointer interaction data forwarded by framework-owned interaction systems.
    /// This carries input context only and does not imply gameplay meaning.
    /// </summary>
    public readonly struct InteractionPointerContext
    {
        public InteractionPointerContext(int pointerId, Vector2 screenPosition, Vector3 worldPosition)
        {
            PointerId = pointerId;
            ScreenPosition = screenPosition;
            WorldPosition = worldPosition;
        }

        /// <summary>
        /// Pointer identifier supplied by the input source.
        /// For the MVP this supports a single active pointer, but the data shape stays extensible.
        /// </summary>
        public int PointerId { get; }

        /// <summary>
        /// Current pointer position in screen space.
        /// </summary>
        public Vector2 ScreenPosition { get; }

        /// <summary>
        /// Current pointer position projected into world space by the input system.
        /// </summary>
        public Vector3 WorldPosition { get; }
    }
}
