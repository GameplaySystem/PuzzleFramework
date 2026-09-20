# Wall Generation System

## Document Metadata

Category:
- Core Board Systems

Status:
- Approved

Parent:
- Overview.md

Related Documents:
- Overview.md
- GridSystem.md
- ShapeSystem.md
- ../PresentationSystems/ModularBoardVisualSystem.md
- ../ContentSystems/LevelEditorFoundation.md

Depends On:
- GridSystem.md
- ShapeSystem.md

Used By:
- Modular Board Visual System
- Level Editor Foundation (authoring boundary view composition)
- Drop Away
- Color Block Jam
- Sky Rush
- Hole People
- Bus Jam

## Purpose

The Wall Generation System derives boundary wall structure from board layout.

It exists so boundary walls and corners can be generated from shared board data rather than hand-authored separately for every board.

The Wall Generation System is about derived boundary structure.

It is not about gameplay rules or movement meaning.

---

# Core Design Idea

Walls are derived from board layout.

They are not a primary puzzle-rule system.

Conceptually:

```text
Grid and board layout data
    ->
Wall Generation System analyzes boundaries
    ->
System returns wall segments and corners
    ->
Modular Board Visual System or another caller consumes the structural result
```

The important boundary is:

* board layout defines structure
* Wall Generation derives boundary walls from that structure
* game modules decide whether walls have puzzle-specific meaning

---

# Responsibilities

The Wall Generation System is responsible for:

* boundary wall generation
* wall corner generation
* board boundary structure derivation
* exposing derived wall-layout results based on shared board data

Typical questions it should answer:

```text
Where are the outer board boundaries?
Where should wall segments exist?
Where should wall corners exist?
```

---

# Should Not Handle

The Wall Generation System should not handle:

* gameplay rules
* movement rules
* pathfinding decisions
* occupancy ownership
* input
* drag or snap behavior
* puzzle-specific collision meaning
* win or lose conditions

Wall derivation is structural.

It does not decide what a wall means in puzzle logic.

---

# Boundary Derivation

The Wall Generation System should derive walls from board layout.

This may include:

* board outer edges
* exposed transitions between active and inactive space
* boundary corners where needed

The system should avoid treating walls as an unrelated manually owned gameplay abstraction when they are just a structural consequence of layout.

This reduces duplication and keeps boundaries consistent with the board definition.

---

# Boundary Input Contract

Wall generation should consume an explicit set of coordinates that participate in the
derived boundary.

Conceptually:

```text
Boundary-participating cell coordinates
    ->
Wall Generation System
    ->
Derived edge and corner facts
```

The caller owns the mapping from its board data into that boundary-participation mask.

This distinction is required because generic cell metadata does not always describe visual
presence. In particular, an authored `Blocked` cell remains an existing structural cell in
the Grid System. One game may render it as a hole while another may render it as a present
but non-enterable cell.

Rules:

* missing, inactive, and out-of-bounds coordinates do not participate by default
* existing structural coordinates may participate
* callers may deliberately exclude existing coordinates based on game-module meaning
* Wall Generation must not assign generic visual meaning to `Blocked` or occupancy state

The system derives topology from the supplied mask. It does not decide how that mask was
created.

---

# Exact Edge Derivation

For every participating cell, inspect its four orthogonal neighbors.

An exposed edge exists on a side when the coordinate on the other side does not participate
in the boundary mask.

```text
participating cell + participating neighbor     -> no exposed edge
participating cell + non-participating neighbor -> one exposed edge
```

Each exposed edge belongs to the participating cell and carries an orientation:

* North
* East
* South
* West

This rule handles rectangular outer bounds, internal holes, irregular silhouettes, and
disconnected board regions without special cases.

---

# Exact Corner Derivation

Corner classification should be vertex-based rather than inferred only from pairs of wall
objects. At each grid vertex, inspect the four cells that touch that vertex.

The structural names should describe geometry:

* `Convex` means the participating silhouette turns outward
* `Concave` means the participating silhouette turns inward around missing space

Presentation code may map those facts to art assets named `Outer Corner` and `Inner Corner`.
The framework should not rely on potentially ambiguous asset naming.

Vertex classification:

| Participating quadrants | Derived result |
| --- | --- |
| 0 | No corner |
| 1 | One convex corner owned by the participating cell |
| 2, orthogonally adjacent | No corner; the boundary continues straight through the vertex |
| 2, diagonally opposite | Diagonal-touch topology; preserve this as an explicit diagnostic/result case |
| 3 | One concave corner oriented toward the missing quadrant |
| 4 | No corner |

For cell-local presentation, ownership can be mapped deterministically:

* a convex corner belongs to the sole participating cell
* a concave corner belongs to the participating cell diagonally opposite the missing quadrant
* a diagonal-touch vertex represents two coincident convex turns and must not be silently
  collapsed into one corner

Diagonal-touch input is geometrically ambiguous for some art sets. The structural system
should expose that fact rather than guessing whether presentation should overlap two pieces,
separate the regions, or reject the layout. The caller decides the supported policy.

---

# Corner Generation

Wall corners belong here as part of derived boundary structure.

If wall segments are derived from board edges, corner generation should remain part of the same structural responsibility rather than being split into a separate early system.

This keeps the boundary derivation model coherent:

```text
Board layout
    ->
Wall segments
    +
Wall corners
    ->
Derived boundary structure
```

---

# Framework vs Game Module Ownership

Framework ownership:

* derived board boundary structure
* wall segment generation
* corner generation

Framework Presentation ownership:

* translating the derived structure into generic modular cell-visual slot state
* applying that state through framework-safe visual contracts

Game module ownership:

* whether derived boundaries participate in gameplay restrictions
* whether walls have thematic or puzzle-specific meaning
* mapping game-specific level data into the boundary-participation mask
* concrete cell prefabs, meshes, materials, and art configuration

Framework owns structural derivation.

Framework Presentation may own reusable visual application patterns.

Game modules own puzzle-specific meaning and concrete art.

---

# Data Flow

A typical flow may look like:

```text
Grid or board layout data
    ->
Wall Generation System analyzes exposed boundaries
    ->
System returns wall and corner structure
    ->
Modular Board Visual System converts structure into visual slot state
    ->
Game-supplied visual instances display the result
```

The Wall Generation System should consume shared board structure and produce derived structural output.

It should not own puzzle-specific consequences of those walls.

Derived output should describe logical coordinates, orientations, corner geometry, and any
topology diagnostics. It should not contain Unity prefab references, transforms, materials,
or game-specific presentation objects.

The Wall Generation System should not know whether presentation uses full wall segments,
half-wall pieces, corner caps, inner elbows, mesh generation, or another rendering strategy.
Those decisions belong to a presentation consumer such as the Modular Board Visual System.

---

# Example Usage

## Board Setup

A board-definition workflow may derive outer wall structure automatically from the board layout.

The Wall Generation System provides the structural result.

Another system decides how those walls are instantiated or rendered later.

## Presentation Support

Presentation-facing systems may use derived wall and corner results to place visuals consistently.

The Wall Generation System still only owns structural derivation.

## Puzzle Interpretation

A game module may decide that a derived wall blocks movement, defines a visual frame, or affects puzzle behavior.

That interpretation belongs outside the Wall Generation System.

---

# Edge Cases

## Inactive Cell Boundaries

If a board contains inactive cells within the overall board bounds:

* exposed edges between active and inactive space may need derived boundaries

That remains a structural derivation problem, not a puzzle-rule problem.

## Disconnected Board Regions

If the board contains multiple disconnected active regions:

* boundary generation should still derive walls consistently around each region

## Diagonal Cell Contact

If two participating regions touch only at one vertex:

* edge derivation remains deterministic
* the shared vertex should be reported as diagonal-touch topology
* presentation policy should not be guessed by the framework

## Derived Wall Drift Into Rules

A common failure mode is slowly adding gameplay blocking logic directly into wall derivation because walls often imply blocked movement.

Rule:

Wall Generation derives structure.

Other systems decide gameplay meaning.

---

# MVP Scope

The first version of the Wall Generation System should support:

* outer boundary wall generation
* corner generation
* board boundary structure derivation from shared layout
* explicit boundary-participation input
* convex and concave geometric corner classification
* diagonal-touch topology reporting

The MVP should stay at the level of structural derivation.

It should not absorb gameplay-rule ownership.

---

# Future Extensions

The following can be added later if reuse pressure appears:

* richer boundary result data
* debug wall-preview tools
* editor visualization helpers
* specialized handling for additional reusable board-boundary patterns

These should remain subordinate to the core rule:

walls are derived from board layout.

---

# Final Design Rule

The Wall Generation System derives board boundary walls and corners from layout.

It owns structural wall generation.

It does not own gameplay rules or movement meaning.
