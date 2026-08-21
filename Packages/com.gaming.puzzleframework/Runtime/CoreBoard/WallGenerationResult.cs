using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PuzzleFramework.CoreBoard
{
    /// <summary>
    /// Deterministic structural boundary derived from an explicit participation mask.
    /// </summary>
    public sealed class WallGenerationResult
    {
        internal WallGenerationResult(
            IList<GridCoordinate> participatingCoordinates,
            IList<BoardBoundaryEdge> exposedEdges,
            IList<BoardBoundaryVertex> boundaryVertices)
        {
            ParticipatingCoordinates = Copy(participatingCoordinates);
            ExposedEdges = Copy(exposedEdges);
            BoundaryVertices = Copy(boundaryVertices);

            for (int i = 0; i < BoundaryVertices.Count; i++)
            {
                if (BoundaryVertices[i].Kind == BoardBoundaryVertexKind.DiagonalTouch)
                {
                    HasDiagonalTouch = true;
                    break;
                }
            }
        }

        public IReadOnlyList<GridCoordinate> ParticipatingCoordinates { get; }
        public IReadOnlyList<BoardBoundaryEdge> ExposedEdges { get; }
        public IReadOnlyList<BoardBoundaryVertex> BoundaryVertices { get; }
        public bool HasDiagonalTouch { get; }

        private static IReadOnlyList<T> Copy<T>(IList<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return new ReadOnlyCollection<T>(new List<T>(source));
        }
    }
}
