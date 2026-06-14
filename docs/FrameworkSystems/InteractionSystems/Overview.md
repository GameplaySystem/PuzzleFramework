# Interaction Systems

## Document Metadata

Category:
- Interaction Systems

Status:
- Approved

Parent:
- None

Related Documents:
- InputSystem.md
- DragMovementSystem.md
- GridSnapSystem.md

Depends On:
- None

Used By:
- Drop Away
- Color Block Jam
- Sky Rush
- Hole People
- Bus Jam

## Purpose

Interaction Systems define how the player communicates with puzzle objects during gameplay.

They translate player intent into reusable framework-level interaction behavior.

These systems should remain reusable across multiple puzzle games and should not contain game-specific rules.

---

# Why This Category Exists

All target games need some form of player-driven interaction.

Common needs include:

* selecting an object
* dragging an object
* snapping an object onto valid board positions
* preventing invalid framework-level placement

If these concerns are embedded directly inside puzzle-specific objects, the framework becomes tightly coupled to individual games.

Interaction Systems separate:

* input detection
* movement preview
* board-aligned placement

from:

* puzzle-specific rules
* object meaning
* completion logic

---

# Systems In This Category

This category contains three systems:

1. Input System
2. Drag Movement System
3. Grid Snap System

Each system owns a different part of runtime interaction.

## Input System

The Input System detects player intent.

It owns:

* pointer press and release handling
* world interaction raycasts
* selection detection
* interaction capability calls

It does not own movement rules or puzzle meaning.

## Drag Movement System

The Drag Movement System previews movement during drag.

It owns:

* movement preview during drag
* framework-level drag constraints
* last-valid-position behavior

It does not own final placement or puzzle meaning.

## Grid Snap System

The Grid Snap System resolves board-aligned placement.

It owns:

* world-to-grid and grid-to-world conversion
* final board-aligned snap evaluation
* framework-level placement validation

It does not own puzzle-specific placement meaning.

---

# Interface-Based Interaction

Framework interaction should rely on capabilities rather than concrete game object types.

The Input System should not know what a Hole, Brick, Bus, Stickman, or Door is.

Instead, interactive objects should expose framework-safe capabilities such as:

* selectable
* draggable
* clickable
* hoverable if needed later

This keeps the framework decoupled, reusable, and easier to extend across multiple games.

---

# Input vs Drag vs Snap

These systems should remain separate by responsibility.

Input:

* detects player intent
* identifies which object is being interacted with

Drag:

* previews object movement during interaction
* applies reusable framework-level movement constraints

Snap:

* resolves board-aligned placement
* validates framework-level final placement

In short:

```text
Input
-> detects intent

Drag
-> previews movement

Snap
-> resolves board alignment
```

---

# Framework vs Game Module Ownership

Framework ownership:

* input detection
* interaction capability contracts
* movement preview
* board-aligned snapping
* framework-level placement validation

Game module ownership:

* puzzle-specific object meaning
* whether an interaction is desirable in puzzle terms
* collection rules
* win or lose meaning
* puzzle-specific consequences after interaction

The framework remains game-agnostic.

It must not know:

* Hole
* Stickman
* Bus
* Door
* Brick

Those belong to game modules.

---

# System Relationships

A typical gameplay interaction may look like:

```text
Player Input
    ->
Input System
    ->
Drag Movement System
    ->
Grid Snap System
    ->
Game Module Rules
```

This is a common interaction chain, not a rule that every interaction must pass through every system.

Some interactions may stop after input detection.

Some may use snapping without dragging.

The shared rule is:

* framework systems detect, preview, and align interaction
* game modules decide what those interaction results mean

---

# What Does Not Belong Here

The following do not belong inside Interaction Systems:

* puzzle-specific rules
* win conditions
* lose conditions
* pathfinding decisions
* resource processing rules
* authored content persistence
* progression data
* UI ownership
* presentation ownership

More specifically:

* Input should not decide whether a move is good for the puzzle.
* Drag should not decide whether an object completes an objective.
* Snap should not decide whether a placement satisfies a game rule.

---

# Final Design Rule

Interaction Systems translate player intent into reusable framework interaction behavior.

Input detects intent.

Drag previews movement.

Snap resolves board alignment.

Game modules decide what those interaction results mean.
