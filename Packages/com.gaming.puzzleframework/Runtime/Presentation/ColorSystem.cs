using System;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleFramework.Presentation
{
    /// <summary>
    /// Minimal shared color mapping system for the MVP.
    /// It resolves framework color identities to concrete visual colors and does not
    /// decide what those identities mean in puzzle logic.
    /// </summary>
    public sealed class ColorSystem
    {
        private readonly Dictionary<ColorIdentity, Color> _colorsByIdentity = new();

        public ColorSystem(IEnumerable<ColorVisualMapping> mappings)
        {
            if (mappings == null)
            {
                throw new ArgumentNullException(nameof(mappings));
            }

            foreach (ColorVisualMapping mapping in mappings)
            {
                if (mapping.Identity == ColorIdentity.None)
                {
                    throw new ArgumentException(
                        "Color system contains a mapping with the default None identity.",
                        nameof(mappings));
                }

                // Duplicate identity validation belongs here because mapping ownership is presentation data.
                if (!_colorsByIdentity.TryAdd(mapping.Identity, mapping.Color))
                {
                    throw new ArgumentException(
                        $"Color system contains duplicate mapping for identity {mapping.Identity}.",
                        nameof(mappings));
                }
            }
        }

        /// <summary>
        /// Returns true when the identity has a registered visual color mapping.
        /// </summary>
        public bool ContainsIdentity(ColorIdentity identity)
        {
            return _colorsByIdentity.ContainsKey(identity);
        }

        /// <summary>
        /// Attempts to resolve a concrete visual color for the shared identity.
        /// </summary>
        public bool TryResolveColor(ColorIdentity identity, out Color color)
        {
            return _colorsByIdentity.TryGetValue(identity, out color);
        }
    }
}
