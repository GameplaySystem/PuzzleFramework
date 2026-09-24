using System;
using System.Collections.Generic;
using System.Text;
using PuzzleFramework.CoreBoard;
using UnityEngine;

namespace PuzzleFramework.Presentation
{
    /// <summary>
    /// Explicit-lifetime cache for generated footprint meshes. Equivalent sorted offsets and exact
    /// settings share one mesh; callers must dispose the cache with their view/scene lifetime.
    /// </summary>
    public sealed class FootprintMeshCache : IDisposable
    {
        private readonly FootprintMeshGenerator _generator;
        private readonly Dictionary<CacheKey, FootprintMeshGenerationResult> _results = new();
        private bool _disposed;

        public FootprintMeshCache(FootprintMeshGenerator generator = null)
        {
            _generator = generator ?? new FootprintMeshGenerator();
        }

        public int CachedMeshCount => _results.Count;

        public FootprintMeshGenerationResult GetOrCreate(
            ShapeFootprint footprint,
            FootprintMeshSettings settings)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(FootprintMeshCache));
            if (footprint == null)
                return FootprintMeshGenerationResult.Failed("A shape footprint is required.");

            CacheKey key = new(BuildSignature(footprint.Offsets), settings);
            if (_results.TryGetValue(key, out FootprintMeshGenerationResult cached) &&
                cached.Mesh != null)
                return cached;

            FootprintMeshGenerationResult generated = _generator.Generate(footprint, settings);
            if (generated.Success) _results[key] = generated;
            return generated;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            foreach (FootprintMeshGenerationResult result in _results.Values)
            {
                Mesh mesh = result.Mesh;
                if (mesh == null) continue;
                if (Application.isPlaying) UnityEngine.Object.Destroy(mesh);
                else UnityEngine.Object.DestroyImmediate(mesh);
            }
            _results.Clear();
        }

        private static string BuildSignature(IReadOnlyList<GridCoordinate> offsets)
        {
            List<GridCoordinate> sorted = new(offsets);
            sorted.Sort((left, right) =>
            {
                int y = left.Y.CompareTo(right.Y);
                return y != 0 ? y : left.X.CompareTo(right.X);
            });
            StringBuilder builder = new(sorted.Count * 8);
            for (int i = 0; i < sorted.Count; i++)
                builder.Append(sorted[i].X).Append(',').Append(sorted[i].Y).Append(';');
            return builder.ToString();
        }

        private readonly struct CacheKey : IEquatable<CacheKey>
        {
            public CacheKey(string signature, FootprintMeshSettings settings)
            {
                Signature = signature;
                Settings = settings;
            }

            private string Signature { get; }
            private FootprintMeshSettings Settings { get; }

            public bool Equals(CacheKey other) =>
                string.Equals(Signature, other.Signature, StringComparison.Ordinal) &&
                Settings.Equals(other.Settings);

            public override bool Equals(object obj) => obj is CacheKey other && Equals(other);
            public override int GetHashCode() => HashCode.Combine(Signature, Settings);
        }
    }
}
