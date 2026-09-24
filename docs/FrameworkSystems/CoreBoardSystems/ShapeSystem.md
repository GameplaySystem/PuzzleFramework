# Shape System

## Document Metadata

Category:
- Core Board Systems

Status:
- Approved

Parent:
- Overview.md

Related Documents:
- Overview.md
- ../PresentationSystems/FootprintMeshGenerationSystem.md
- GridSystem.md
- CellOccupancySystem.md
- WallGenerationSystem.md

Depends On:
- GridSystem.md

Used By:
- Drop Away
- Color Block Jam
- Sky Rush
- Hole People
- Bus Jam

## Purpose

The Shape System defines multi-cell footprints.

It exists to describe how an item, footprint, or board-space definition occupies more than one cell without embedding gameplay meaning inside the footprint itself.

The Shape System is about footprint structure.

It is not about puzzle rules or runtime rotation behavior.

---

# Core Design Idea

Shape defines footprint.

It does not decide meaning.

Conceptually:

```text
Caller requests shape footprint
    ->
Shape System returns cell offsets or occupied footprint
    ->
Caller uses footprint for placement, occupancy, walls, or authored content
```

The important boundary is:

* Shape owns multi-cell structure
* other systems decide placement validity, occupancy, and puzzle meaning

---

# Responsibilities

The Shape System is responsible for:

* defining shape footprints
* defining multi-cell structures
* exposing shape offsets
* supporting footprint queries for placement and validation systems
* preserving authored orientation at runtime

Typical questions it should answer:

```text
Which cells belong to this shape?
What offsets define this footprint?
How many cells does this shape cover?
```

---

# Should Not Handle

The Shape System should not handle:

* occupancy ownership
* movement rules
* input
* drag or snap behavior
* puzzle-specific legality
* collection logic
* win or lose conditions
* runtime decision-making about rotation

The Shape System defines footprint structure.

It does not decide how that footprint should be used.

---

# Multi-Cell Definitions

The Shape System should support reusable multi-cell definitions.

Examples:

* single-cell shapes
* line shapes
* block footprints
* authored irregular footprints where shared board logic needs them

The framework-level requirement is not a specific catalog of shapes.

The requirement is that footprint definition remains generic and reusable.

---

# Offsets

The Shape System should define footprints using offsets relative to an origin.

Conceptually:

```text
Origin cell
    +
Offset list
    ->
Resolved footprint cells
```

This keeps the system generic and compatible with:

* occupancy checks
* authored placement
* boundary analysis

Offsets are structural data.

They do not encode puzzle-specific meaning.

---

# Rotation Rule

Approved decision:

* rotation is editor-only
* runtime uses authored orientation

This means:

* level authoring may rotate a shape while creating content
* gameplay runtime should use the orientation already authored into the level

The Shape System should not become a runtime rotation controller for gameplay.

That keeps shape logic simpler and prevents unnecessary runtime ambiguity.

---

# Framework vs Game Module Ownership

Framework ownership:

* footprint definitions
* multi-cell structure
* offset-based shape data
* editor-compatible orientation support

Game module ownership:

* what a footprint represents in puzzle terms
* whether a specific object should use a given footprint
* puzzle-specific consequences of footprint overlap or orientation

Framework owns structure.

Game modules own meaning.

---

# Data Flow

A typical flow may look like:

```text
Caller requests shape data
    ->
Shape System returns footprint offsets
    ->
Grid System resolves coordinates
    ->
Cell Occupancy System checks usage if needed
    ->
Caller uses result for placement, validation, or board derivation
```

The Shape System should provide reusable footprint data that other systems can consume.

It should not own the downstream puzzle interpretation.

---

# Example Usage

## Occupancy Validation

An occupancy or placement system may ask for a shape footprint at a target coordinate.

The Shape System provides the footprint offsets.

Another system checks whether those cells are free.

## Level Authoring

An editor workflow may rotate a shape during authoring.

The authored orientation becomes part of level content.

Runtime then uses that authored orientation directly.

## Wall Derivation

If a board feature is represented with a multi-cell footprint, the Shape System may help derive which cells belong to that structure.

The Shape System still only defines footprint.

---

# Edge Cases

## Single-Cell Shapes

A shape may cover only one cell.

That is still a valid footprint and should not require separate special-case architecture.

## Irregular Footprints

Some shapes may not be simple rectangles or lines.

Offset-based footprint definition should still support them cleanly.

## Runtime Rotation Drift

A common failure mode is letting gameplay runtime start rotating shapes dynamically because editor rotation already exists.

Rule:

Editor rotation support does not imply runtime rotation support.

Runtime uses authored orientation.

## Footprint Without Meaning

A shape definition by itself should remain generic.

If the shape system starts deciding what a footprint represents in puzzle rules, it has crossed the framework boundary.

---

# MVP Scope

The first version of the Shape System should support:

* shape footprints
* multi-cell definitions
* offset queries
* editor-authored orientation support
* runtime use of authored orientation

The MVP should not include runtime gameplay rotation behavior.

---

# Future Extensions

The following can be added later if reuse pressure appears:

* richer authored shape libraries
* debug footprint visualization
* editor footprint tools
* additional serialization helpers

These should remain subordinate to the core rule:

shape defines footprint.

---

# Final Design Rule

The Shape System defines footprint.

It owns multi-cell definitions and offsets.

Rotation is editor-only.

Runtime uses authored orientation.

The Shape System does not decide gameplay meaning.
