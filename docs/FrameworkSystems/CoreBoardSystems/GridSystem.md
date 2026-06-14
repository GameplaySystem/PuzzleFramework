# Grid System

## Document Metadata

Category:
- Core Board Systems

Status:
- Approved

Parent:
- Overview.md

Related Documents:
- Overview.md
- CellOccupancySystem.md
- ShapeSystem.md
- WallGenerationSystem.md
- PathFindingSystem.md

Depends On:
- None

Used By:
- Drop Away
- Color Block Jam
- Sky Rush
- Hole People
- Bus Jam

## Purpose

The Grid System defines the board coordinate space.

It owns the structural questions of where cells exist, whether a coordinate is valid, how cells are looked up, and which neighboring cells are adjacent.

The Grid System is about board structure.

It is not about cell usage or puzzle rules.

---

# Core Design Idea

The Grid System defines where cells exist.

It does not decide what those cells mean.

Conceptually:

```text
Caller provides coordinate or board query
    ->
Grid System validates coordinate and resolves cell information
    ->
Caller uses structural result
```

The important boundary is:

* Grid owns coordinate structure
* other systems decide occupancy, movement meaning, and puzzle meaning

---

# Responsibilities

The Grid System is responsible for:

* owning board dimensions
* defining the board coordinate model
* supporting cell lookup by coordinate
* validating whether coordinates are inside the board
* exposing neighbor queries
* exposing generic board-structure queries

Typical questions it should answer:

```text
Is this coordinate inside the board?
Which cell is at this coordinate?
Which cells neighbor this coordinate?
How large is the board?
```

---

# Should Not Handle

The Grid System should not handle:

* occupancy
* reservation state
* movement rules
* input
* drag or snap behavior
* puzzle-specific placement rules
* win or lose conditions
* collection logic

The Grid System defines where cells exist.

It does not decide what may happen on them.

---

# Board Dimensions

The Grid System owns board dimensions.

Examples:

* width
* height
* optional board bounds helpers

This allows other systems to reason about board space without duplicating dimension logic.

The board remains cell-based.

That is a shared architectural decision across the framework.

---

# Coordinate Model

The Grid System should use a clear cell-coordinate model.

Typical conceptual shape:

```text
(x, y)
```

Where:

* `x` identifies horizontal position
* `y` identifies vertical position

The exact implementation type can be decided later.

The architecture boundary is simply that the Grid System owns the shared coordinate convention.

---

# Coordinate Validation

The Grid System should validate whether a coordinate is inside board bounds.

Examples of structural checks:

* coordinate inside width and height
* coordinate corresponds to a known cell location

This is structural validation only.

It is not occupancy validation or puzzle-rule validation.

---

# Cell Lookup

The Grid System should support cell lookup by coordinate.

Examples:

```text
Coordinate
    ->
Grid System
    ->
Cell reference or cell data
```

The Grid System may expose cells, cell metadata, or lookup results depending on later implementation shape.

The framework-level rule is that board lookup belongs here.

---

# Neighbor Queries

The Grid System should own neighbor queries.

Examples:

* up
* down
* left
* right

This is useful for:

* pathfinding
* wall derivation
* generic board analysis

Neighbor queries answer structural adjacency.

They do not decide whether movement through a neighbor is allowed.

---

# Framework vs Game Module Ownership

Framework ownership:

* board dimensions
* coordinate conventions
* cell lookup
* coordinate validation
* neighbor queries

Game module ownership:

* what objects occupy cells
* what moving through a cell means
* what puzzle rules apply to a coordinate

Framework owns board structure.

Game modules own gameplay interpretation.

---

# Data Flow

A typical flow may look like:

```text
Caller requests structural board information
    ->
Grid System validates coordinate
    ->
Grid System resolves cell or neighbors
    ->
Caller uses result for occupancy, pathfinding, placement, or rules
```

The Grid System should remain the source of shared board-space truth.

Other systems should read from it rather than recreating their own board coordinate logic.

---

# Example Usage

## Placement Validation

A caller may ask:

```text
Are these coordinates inside the board?
```

The Grid System answers the structural question.

Another system decides whether the placement is allowed.

## Pathfinding

The Pathfinding System may ask for neighbors of a coordinate.

The Grid System answers adjacency.

Pathfinding decides traversability using additional systems.

## Wall Generation

The Wall Generation System may inspect board edges and neighboring cell existence.

The Grid System provides structural board information.

---

# Edge Cases

## Out-of-Bounds Coordinate

If a coordinate is outside the board:

* lookup should fail safely
* validation should report the coordinate as invalid

The Grid System should not silently treat out-of-bounds coordinates as valid board cells.

## Sparse or Inactive Layouts

Some boards may contain inactive cells inside the total board bounds.

The Grid System may still define the board dimensions while cell state determines whether a cell participates in gameplay.

This is why:

* bounds belong to Grid
* active or inactive participation can remain part of cell data consumed by other systems

## Neighbor Query At Board Edge

If a cell is on an edge:

* missing neighbors should be handled cleanly
* the Grid System should not fabricate neighbors outside the board

---

# MVP Scope

The first version of the Grid System should support:

* board dimensions
* coordinate validation
* cell lookup
* neighbor queries
* board-space structural access for other systems

The MVP should stay at the level of shared board structure.

It should not absorb occupancy or rule logic.

---

# Future Extensions

The following can be added later if reuse pressure appears:

* alternative grid layouts if ever required
* richer coordinate helpers
* editor visualization helpers
* debug inspection tools
* cached neighbor maps

These should remain subordinate to the core rule:

the Grid System defines where cells exist.

---

# Final Design Rule

The Grid System defines where cells exist.

It owns board dimensions, coordinates, lookup, validation, and neighbors.

It does not own occupancy, movement meaning, or puzzle rules.
