using System;
using UnityEngine;

namespace PuzzleFramework.Presentation
{
    /// <summary>
    /// Presentation-only mapping from a shared color identity to a concrete visual color.
    /// This carries no gameplay rule meaning.
    /// </summary>
    [Serializable]
    public readonly struct ColorVisualMapping
    {
        public ColorVisualMapping(ColorIdentity identity, Color color)
        {
            if (identity == ColorIdentity.None)
            {
                throw new ArgumentException(
                    "Color mapping requires a non-default color identity.",
                    nameof(identity));
            }

            Identity = identity;
            Color = color;
        }

        /// <summary>
        /// Shared color identity being mapped.
        /// </summary>
        public ColorIdentity Identity { get; }

        /// <summary>
        /// Concrete visual color used by presentation.
        /// </summary>
        public Color Color { get; }
    }
}
