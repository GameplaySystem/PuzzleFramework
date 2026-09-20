# Cell Occupancy System

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
- PathFindingSystem.md

Depends On:
- GridSystem.md
- ShapeSystem.md

Used By:
- Drop Away
- Color Block Jam
- Sky Rush
- Hole People
- Bus Jam

## Approved 2026-09-20 implementation extension

Drop The Man already transfers a committed footprint by releasing old cells, occupying new cells,
and rolling back on failure. Color Block Escape needs the same rule-free operation. Add one
all-or-nothing transfer contract taking explicit old/new coordinate sets. Validate nonempty,
unique structural coordinates; verify old cells are occupied and new cells are free except where
they overlap old cells; reject reservations. Mutate only after complete validation. Return a
result rather than partially changing occupancy. The caller supplies the correct source cells
and remains responsible for Blocked-cell placement legality, entity ownership, snap/reachability,
and gameplay meaning. Do not add game identities or exit rules to occupancy. Both games consume
this operation; individual `Occupy`/`Release` remain available for progressive CBE exit release.

## Purpose

The Cell Occupancy System tracks cell usage on the board.

It exists to answer which cells are occupied, which cells are reserved, whether a placement overlaps existing usage, and whether a requested footprint can be accepted structurally.

The Cell Occupancy System is about cell usage.

It is not about movement meaning or puzzle rules.

---

# Core Design Idea

Occupancy tracks cell usage.

It does not decide what that usage means.

Conceptually:

```text
Caller provides placement or occupancy query
    ->
Cell Occupancy System checks current cell usage
    ->
System returns occupancy result
    ->
Caller decides what to do next
```

The important boundary is:

* Grid defines where cells exist
* Shape defines footprint
* Occupancy tracks whether those cells are already in use

---

# Responsibilities

The Cell Occupancy System is responsible for:

* tracking occupied cells
* tracking reserved cells
* validating whether a placement overlaps occupied or reserved cells
* supporting occupancy queries
* supporting placement-validation queries based on cell usage
* supporting add, remove, reserve, and release style occupancy operations in architecture terms

Typical questions it should answer:

```text
Is this cell occupied?
Is this cell reserved?
Can this footprint fit here without overlap?
Which cells are currently in use?
```

---

# Should Not Handle

The Cell Occupancy System should not handle:

* movement execution
* input
* drag behavior
* snapping behavior
* puzzle-specific placement rules
* collection logic
* win or lose conditions
* why a cell is occupied in puzzle terms

Occupancy tracks cell usage.

It does not decide whether a move is desirable or meaningful.

---

# Occupied vs Reserved

The Cell Occupancy System should distinguish between:

* occupied cells
* reserved cells

Occupied cells are already in use.

Reserved cells are structurally claimed for an upcoming or temporary board operation.

This distinction is useful because many systems need to ask slightly different questions:

```text
Is the cell already taken?
Is the cell currently blocked for placement?
```

The architecture does not require puzzle-specific semantics for reservation.

It only requires that the framework can represent temporary structural claims.

---

# Placement Validation

The Cell Occupancy System should validate placement against current cell usage.

Typical conceptual flow:

```text
Shape footprint at candidate coordinate
    ->
Grid System confirms cells exist
    ->
Cell Occupancy System checks whether cells are free
    ->
Caller receives structural placement result
```

Placement validation here is occupancy validation.

It is not puzzle-rule validation.

Examples of what does not belong here:

* color matching
* movement timing
* interaction ownership
* puzzle-specific legality

---

# Occupancy Queries

The Cell Occupancy System should support direct occupancy queries.

Examples:

* single-cell occupancy
* single-cell reservation
* multi-cell footprint occupancy
* aggregated usage checks

These queries are shared mechanics that other systems can build on:

* pathfinding
* grid snap validation
* editor placement checks
* gameplay placement checks

---

# Framework vs Game Module Ownership

Framework ownership:

* occupied cell tracking
* reserved cell tracking
* structural placement validation
* occupancy queries

Game module ownership:

* what occupies a cell in gameplay meaning
* why a reservation exists
* what an overlap means for the puzzle
* puzzle-specific consequences of blocked placement

Framework owns usage mechanics.

Game modules own meaning.

---

# Data Flow

A typical flow may look like:

```text
Caller requests placement or occupancy check
    ->
Grid System resolves coordinates
    ->
Shape System provides footprint if needed
    ->
Cell Occupancy System checks occupied or reserved state
    ->
Caller receives occupancy result
```

The Cell Occupancy System should sit between board structure and higher-level placement meaning.

It answers whether cells are in use.

It does not answer whether that usage is acceptable in puzzle terms.

---

# Example Usage

## Grid Snap Validation

An interaction system may ask whether a footprint can occupy candidate cells.

The Cell Occupancy System answers overlap and reservation questions.

The interaction or game-rule system decides what to do with the result.

## Pathfinding

The Pathfinding System may ask whether cells should be treated as occupied.

The Cell Occupancy System provides occupancy information.

Pathfinding decides traversability using that information plus grid and cell state.

## Editor Placement

An editor tool may ask whether a multi-cell authored shape overlaps existing usage.

The Cell Occupancy System answers the structural overlap question.

---

# Edge Cases

## Reserve Without Occupy

A cell may be reserved but not yet occupied.

The system should represent that state clearly rather than collapsing it into generic occupancy.

## Release Missing Reservation

If a caller tries to release a reservation that does not exist:

* the system should fail safely
* authoritative occupancy state should remain coherent

## Multi-Cell Partial Overlap

If a footprint spans multiple cells and only one conflicts:

* placement validation should fail cleanly
* the system should not treat partial fit as valid

## Out-of-Bounds Queries

If a caller asks about cells outside the board:

* Grid should reject the structural query first
* Occupancy should not silently accept impossible cells

---

# MVP Scope

The first version of the Cell Occupancy System should support:

* occupied cell tracking
* reserved cell tracking
* occupancy queries
* structural placement validation
* footprint overlap checks

The MVP should stay at the level of usage tracking.

It should not absorb movement or puzzle-rule meaning.

---

# Future Extensions

The following can be added later if reuse pressure appears:

* richer occupancy result data
* debug occupancy visualization
* batched reservation helpers
* editor inspection tools
* layered occupancy categories if a real shared use case appears

These should remain subordinate to the core rule:

occupancy tracks cell usage.

---

# Final Design Rule

The Cell Occupancy System tracks cell usage.

It owns occupied cells, reserved cells, placement validation, and occupancy queries.

It does not own movement, input, or puzzle-specific rules.
