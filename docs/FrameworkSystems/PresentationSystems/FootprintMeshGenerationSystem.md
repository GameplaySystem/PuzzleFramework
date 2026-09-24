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

It returns a `FootprintMeshGenerationResult` containing either one mesh plus diagnostic counts or a
failure reason. Invalid input does not publish a partial mesh.

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
5. Build bottom-bevel, side, and top-bevel contour rings.
6. Offset each bevel ring inward from the original contour.
7. Connect neighboring rings with outward-facing quads.
8. Ear-clip the inset top contour and mirror it for the bottom cap.
9. Recalculate normals, tangents, and bounds on the completed mesh.

Internal cell edges never enter the exterior contour, so they create no side faces or bevels.
Cap triangles tessellate one polygonal surface; triangle edges are not separate surfaces.

## Deliberate First-Slice Limits

The initial generator accepts one edge-connected, hole-free contour. It rejects:

- disconnected footprint islands;
- internal holes, which require polygon triangulation with holes;
- diagonal-touch/non-manifold boundary vertices;
- bevel values that collapse a one-cell corridor or the extrusion depth;
- an inward offset that becomes degenerate or self-intersecting.

The current bevel rounds the top/bottom perimeter profile. Plan-view polygon corners remain
mitered; fully rounded XY corners would require corner-join geometry with changing contour vertex
counts and are not part of this first slice.

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

Bevel segment count increases ring count linearly. For a contour with `n` vertices and `s` bevel
segments, the current ring strip contains `2s + 2` rings, and its triangle count is:

```text
2(n - 2) + 2n(2s + 1)
```

The game should choose the smallest segment count that produces the intended silhouette on target
hardware.

## Final Design Rule

The system converts already-authoritative footprint data into disposable presentation geometry.
If the generated mesh or its material is removed, gameplay behavior must remain unchanged.
