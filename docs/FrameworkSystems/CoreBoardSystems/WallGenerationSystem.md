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

Depends On:
- GridSystem.md
- ShapeSystem.md

Used By:
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
Caller uses result for setup or presentation
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

Game module ownership:

* whether derived boundaries participate in gameplay restrictions
* whether walls have thematic or puzzle-specific meaning
* how derived walls are rendered or interpreted by a particular game

Framework owns structural derivation.

Game modules own gameplay meaning and presentation meaning.

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
Caller uses result for board setup, rendering, or downstream queries
```

The Wall Generation System should consume shared board structure and produce derived structural output.

It should not own puzzle-specific consequences of those walls.

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
