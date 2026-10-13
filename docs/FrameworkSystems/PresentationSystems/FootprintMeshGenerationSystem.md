# Footprint Mesh Generation System

## Document Metadata

Category:
- Presentation Systems

Status:
- Implemented and verified for the first slice

Parent:
- Overview.md

Related Documents:
- Overview.md
- ModularBoardVisualSystem.md
- ../CoreBoardSystems/ShapeSystem.md
- ../CoreBoardSystems/GridSystem.md
- ../../ColorBlockEscapeRuntimeTechnicalDesign.md

Depends On:
- ../CoreBoardSystems/ShapeSystem.md
- ../CoreBoardSystems/GridSystem.md

Used By:
- Color Block Escape
- Future grid-footprint views that require unified presentation geometry

## Purpose

The Footprint Mesh Generation System converts one generic two-dimensional `ShapeFootprint` into
one presentation-only extruded Unity mesh.

It exists so games can render connected grid footprints as continuous solids without internal
cell faces, gaps, or separately beveled cell seams. The logical footprint remains authoritative;
the generated mesh never participates in movement, collision, occupancy, save data, or outcomes.

## Ownership

PuzzleFramework owns:

- exposed perimeter extraction from footprint cells;
- one-contour validation;
- anchor-aware local coordinate generation;
- extrusion and configurable outer bevel rings;
- optional plan-view corner chamfers for real vertical edge surfaces;
- optional main/edge surface submesh partitioning without assigning materials;
- cap triangulation, normals, tangents, bounds, and diagnostics;
- equivalent-footprint mesh caching with explicit cache lifetime.

Game modules own:

- concrete depth, bevel, and smoothness values;
- materials, colors, shaders, and art direction;
- block/entity view composition;
- when a cache is created and disposed;
- mapping generated presentation to game-specific entities.

PuzzleFramework must not interpret a footprint as a block, hole, door, exit, or other puzzle noun.

## First-Slice Contract

The generator accepts:

- `ShapeFootprint`;
- two-dimensional cell size;
- extrusion depth;
- bevel width;
- positive bevel segment count;
- `GridCellAnchor.Center` or `GridCellAnchor.Corner`.
- optional non-negative plan-view corner bevel width;
- unified surfaces or separated main/edge submeshes.

It returns a `FootprintMeshGenerationResult` containing either one mesh plus diagnostic counts or a
failure reason. Unified mode preserves the original one-submesh contract. Separated mode places
caps and flat wall spans in submesh 0, and top/bottom bevel strips plus vertical corner chamfers in
submesh 1. The framework does not assign meaning or materials to either region. Invalid input does
not publish a partial mesh.

The generated mesh uses local XY for the footprint plane and local Z for extrusion. Its local
origin follows the same convention as `GridWorldLayout.GridToWorldPosition(origin)`:

- `Corner`: offset `(0,0)` occupies `[0, cellSize.x] × [0, cellSize.y]`;
- `Center`: offset `(0,0)` is centered on local `(0,0)`.

The full mesh depth is centered around local Z zero.

## Geometry Approach

1. Derive exposed cell edges using the shared wall-generation topology.
2. Orient those edges counter-clockwise with occupied space on the left.
3. Stitch them into one simple exterior contour and remove collinear points.
4. Reject ambiguous diagonal-touch topology or multiple contours.
5. When requested, replace each plan-view turn with a bounded chamfer segment and retain whether
   each resulting contour span is a flat wall or corner surface.
6. Build bottom-bevel, side, and top-bevel contour rings.
7. Offset each bevel ring inward from the resulting outer contour.
8. Connect neighboring rings with outward-facing quads and route triangles to their requested
   surface region.
9. Ear-clip the inset top contour and mirror it for the bottom cap.
10. Recalculate normals, tangents, and bounds on the completed mesh.

Internal cell edges never enter the exterior contour, so they create no side faces or bevels.
Cap triangles tessellate one polygonal surface; triangle edges are not separate surfaces.

## Deliberate First-Slice Limits

The initial generator accepts one edge-connected, hole-free contour. It rejects:

- disconnected footprint islands;
- internal holes, which require polygon triangulation with holes;
- diagonal-touch/non-manifold boundary vertices;
- bevel values that collapse a one-cell corridor or the extrusion depth;
- an inward offset that becomes degenerate or self-intersecting.

Plan-view corner beveling uses one chamfer face per turn. Multi-segment rounded plan-view corners
remain deferred; they would require a more general corner-arc contract and are not needed for the
current consumers.

These limits cover the current Color Block Escape presets and ordinary hole-free custom
polyominoes. They avoid introducing a general-purpose polygon modeling library before a concrete
game needs it. A consumer must handle a failed result without changing authoritative gameplay
data; CBE logs the limitation and uses a simple presentation fallback for such content.

## Caching

`FootprintMeshCache` keys meshes by sorted footprint offsets and exact mesh settings. Equivalent
offset orderings reuse the same `Mesh`. The cache owns generated mesh lifetime and must be disposed
by the scene or view owner. Cache lookup/generation happens during view construction, never during
ordinary dragging.

## Performance Boundary

Mesh generation may allocate temporary topology, contour, ring, vertex, and index collections.
This is acceptable at view-construction or editor-refresh boundaries. It is not intended for
per-frame regeneration.

Bevel segment count increases ring count linearly. For the final render contour vertex count `n`
and bevel segment count `s`, the ring strip contains `2s + 2` rings, and its triangle count is:

```text
2(n - 2) + 2n(2s + 1)
```

The game should choose the smallest segment count that produces the intended silhouette on target
hardware. A non-zero plan-view corner bevel replaces each original turn with two contour vertices,
so callers should account for that predictable increase during construction-time generation.

## Final Design Rule

The system converts already-authoritative footprint data into disposable presentation geometry.
If the generated mesh or its material is removed, gameplay behavior must remain unchanged.
