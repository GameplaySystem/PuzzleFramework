# Modular Board Visual System

## Document Metadata

Category:
- Presentation Systems

Status:
- Approved

Parent:
- Overview.md

Related Documents:
- Overview.md
- ../CoreBoardSystems/WallGenerationSystem.md
- ../CoreBoardSystems/GridSystem.md

Depends On:
- ../CoreBoardSystems/WallGenerationSystem.md
- ../CoreBoardSystems/GridSystem.md

Used By:
- Drop Away
- Color Block Jam
- Future grid-based puzzle modules using the same modular boundary profile

## Purpose

The Modular Board Visual System converts framework-derived board boundaries into reusable
cell-prefab visual state.

It exists so grid-based puzzle games can use the same board-generation behavior while supplying
their own cell prefab, meshes, materials, layout mapping, and visual-presence rules.

The system owns generic visual planning and application.

It does not own authored level meaning, gameplay blocking, occupancy, or concrete art assets.

---

# Core Design Idea

The reusable pipeline is:

```text
Game module supplies boundary-participating coordinates
    ->
Wall Generation System derives exposed edges and vertex topology
    ->
Modular Board Visual System converts topology into per-cell visual slot state
    ->
Framework visual builder applies that state to game-supplied cell prefabs
```

The framework owns the reusable algorithm and generic prefab-slot contract.

The game module owns what counts as visible board space and which assets fill the slots.

---

# Architecture Review

## Restated Problem

Multiple puzzle games require a board assembled from repeated floor cells and modular boundary
pieces. The exact art differs, but the structural decisions repeat:

* where floor cells exist
* which cell sides touch absent space
* whether a boundary vertex is straight, convex, or concave
* which wall halves and corner pieces should be enabled

That behavior should not be rebuilt independently inside every game module.

## Assumptions

* the board is a square orthogonal grid
* the caller supplies an explicit set of boundary-participating coordinates
* one visual cell prefab represents one participating coordinate
* cell-prefab children are positioned and rotated before runtime
* the initial modular profile uses half-edge walls, convex corner caps, and concave corner elbows
* board visuals are presentation only

## Risks

* interpreting generic blocked metadata as absent geometry would leak game meaning into framework
* one unordered Inspector list would make directional slot configuration fragile
* allowing individual cell views to derive topology independently could create duplicate corners
  and mismatched neighboring wall halves
* concave elbows replace adjacent half-wall visuals and therefore require board-level planning
* diagonal-only cell contact would place two convex caps at one vertex and is unsupported by the
  initial piece profile
* concrete dimensions, pivots, and materials must not become framework constants

## Simpler Solutions Considered

Keep the complete system in the first game module.

Rejected because the topology and slot-activation rules are intentionally reusable across
multiple grid-based puzzle games.

Store authored wall and corner entries in every level.

Rejected because walls are derived from board shape and would drift when cells change.

Let each cell component inspect neighbors and configure itself.

Rejected because concave corners alter half-wall state across neighboring cells and require one
coherent board-level result.

## Recommended Approach

Keep three responsibilities separate:

```text
WallGenerationSystem
-> pure boundary topology

ModularBoardVisualPlanner
-> pure per-cell visual slot state

ModularBoardVisualBuilder / ModularBoardCellView
-> Unity visual instance creation and state application
```

Exact public type names remain subject to implementation review, but these ownership boundaries
are approved.

---

# Input Contract

The system consumes:

* boundary-participating coordinates
* the corresponding Wall Generation result
* a board-to-world layout supplied by the caller
* a game-supplied modular cell visual prefab or visual factory
* an optional visual root for generated instances

The supplied board-to-world layout may be centered from logical board dimensions before visual
construction. The visual builder must consume that layout as-is; it must not independently center
only the generated cells because gameplay views and interaction systems need the same conversion.

The system must not infer participation from:

* `IsBlocked`
* occupancy
* colliders
* renderer state
* scene object presence

The game module translates its content or runtime data into the participation mask before calling
the framework.

---

# Modular Cell Visual Contract

The initial reusable cell profile exposes these logical slots:

```text
CellBase

NorthWestHalfWall
NorthEastHalfWall
EastNorthHalfWall
EastSouthHalfWall
SouthEastHalfWall
SouthWestHalfWall
WestSouthHalfWall
WestNorthHalfWall

ConvexNorthEast
ConvexSouthEast
ConvexSouthWest
ConvexNorthWest

ConcaveNorthEast
ConcaveSouthEast
ConcaveSouthWest
ConcaveNorthWest
```

Equivalent enum-indexed arrays are allowed if their direction mapping is validated. One generic
unordered list is not sufficient.

All optional wall and corner slots should default inactive in the production prefab. Applying a
new result must first reset optional slots so rebuilding cannot preserve stale visuals.

The component owns references and activation only. It must not:

* query neighboring cells
* classify corners
* inspect occupancy
* mutate level data
* decide gameplay blocking
* add wall or corner data to persistence

---

# Activation Rules

## Participating And Absent Cells

For each participating coordinate:

* create or obtain one cell visual
* enable its cell base
* apply its derived modular slot state

For an absent coordinate:

* do not create a visual cell, or keep its complete visual disabled
* do not make the absent coordinate own walls or corners

All boundary visuals are owned by participating-cell views or by framework visual instances
associated with those participating cells.

## Exposed Edges

For every exposed full edge, initially enable both half-wall slots on that edge.

```text
one exposed cell edge
-> first half-wall enabled
-> second half-wall enabled
```

The two logical halves allow vertex-specific corner pieces to replace or cover only the affected
half of an edge.

## Straight Vertex

At a straight boundary vertex:

* keep the relevant half-walls enabled
* enable no convex or concave corner slot
* adjacent half-walls meet at the shared vertex or edge midpoint according to the authored prefab
  profile

## Convex Vertex

At a convex vertex:

* the sole participating cell owns the convex corner slot at that local corner
* keep the two exposed half-walls that terminate at the vertex enabled
* enable the convex cap over their intersection

The initial profile assumes the convex piece acts as a visual cap. The prefab owns the exact
overlap and must hide the wall end faces without z-fighting.

## Concave Vertex

At a concave vertex:

* the participating cell diagonally opposite the missing quadrant owns the concave corner slot
* enable that cell's concave elbow at the corresponding local corner
* disable the two exposed half-wall slots that terminate at the vertex and are visually replaced
  by the elbow arms

Those replaced half-walls may belong to the two participating cells orthogonally adjacent to the
missing quadrant. This is why one board-level planner must calculate the complete result.

## Diagonal-Touch Vertex

At a vertex with two diagonally opposite participating cells:

* preserve the Wall Generation diagnostic
* do not silently choose one owner
* the initial modular profile should reject or clearly report the unsupported visual topology

Two convex caps cannot occupy the same vertex reliably with the current profile.

---

# Visual Construction Ownership

Framework may own reusable presentation construction for this profile:

* converting topology into per-cell slot state
* creating presentation-only cell visual instances through an explicit prefab or factory input
* positioning visual cells through an explicit board-to-world layout
* applying and rebuilding visual state
* reporting unsupported topology or missing required slots

Framework must not create or own authoritative gameplay entities through this system.

Game modules own:

* the concrete prefab asset
* meshes and materials
* which authored/runtime cells participate visually
* the scene composition root that requests a board rebuild
* puzzle-specific gameplay meaning of absent space or displayed walls

---

# Runtime And Editor Reuse

The same planner and cell-view contract should serve gameplay runtime and level-authoring previews.

Gameplay scene:

```text
loaded board data
-> game module participation mapping
-> framework board visual build
```

Level editor:

```text
edited board data
-> game module participation mapping
-> framework board visual rebuild
```

The scenes may use separate composition owners. They must not duplicate topology or slot-activation
algorithms.

Walls and corners remain derived output and must not be stored in level JSON.

---

# Validation

The system should support pure tests for:

* exposed-edge to two-half-wall conversion
* four convex orientations
* four concave orientations
* concave replacement of the correct neighboring half-walls
* reset behavior across repeated rebuilds
* full rectangles
* internal one-cell and multi-cell holes
* edge notches
* L-shaped silhouettes
* disconnected regions
* all-absent input
* diagonal-touch diagnostics

Unity integration checks should verify:

* all directional prefab slots are assigned
* generated cells use the requested layout and root
* no optional pieces flash before configuration
* convex caps hide intersecting wall ends
* concave elbows meet neighboring halves without gaps
* repeated rebuilding leaves no stale or duplicate visual instances

---

# MVP Scope

The first implementation should include:

* pure conversion from Wall Generation results to per-cell modular visual state
* the half-wall / convex-cap / concave-elbow profile
* a generic Unity cell-view component with explicit directional slots
* a narrow reusable visual builder using explicit inputs
* rebuild support
* unsupported diagonal-touch reporting
* automated pure tests for the activation rules

The first implementation should not include:

* gameplay colliders or movement blocking
* mesh combining
* runtime batching optimization
* arbitrary hexagonal or non-orthogonal grids
* authored wall persistence
* puzzle-specific obstacle interpretation
* automatic pivot correction
* support for every possible modular wall art style

---

# Final Design Rule

Wall Generation derives reusable boundary topology.

The Modular Board Visual System converts that topology into reusable modular prefab state.

Game modules decide which cells participate and supply the concrete visual assets.

Neither system owns gameplay meaning.
