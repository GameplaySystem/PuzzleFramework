# Core Board Systems

## Document Metadata

Category:
- Core Board Systems

Status:
- Approved

Parent:
- None

Related Documents:
- GridSystem.md
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

Core Board Systems define the shared structural model of a board-based puzzle space.

They exist to answer reusable framework questions such as:

* where cells exist
* which cells are occupied
* how multi-cell footprints are represented
* how board boundaries are derived
* whether one cell can reach another

This category owns generic board structure.

It does not own puzzle-specific rules or puzzle-specific object meaning.

---

# Why This Category Exists

All target games rely on some form of board structure.

Common needs include:

* a grid with known dimensions
* valid coordinate lookup
* occupancy checks
* multi-cell object footprints
* board boundary generation
* path queries across traversable cells

If those concerns are embedded directly inside puzzle-specific systems, the framework becomes tightly coupled and difficult to reuse.

Core Board Systems exist to separate:

* board structure
* occupancy state
* footprint definition
* derived boundaries
* path queries

from:

* puzzle-specific rules
* input behavior
* movement meaning
* collection logic

---

# Systems In This Category

This category contains five systems:

1. Grid System
2. Cell Occupancy System
3. Shape System
4. Wall Generation System
5. Pathfinding System

Each system owns a different part of board logic.

## Grid System

The Grid System defines where cells exist.

It owns:

* board dimensions
* coordinates
* cell lookup
* coordinate validation
* neighbor queries

It does not own occupancy or puzzle rules.

## Cell Occupancy System

The Cell Occupancy System tracks cell usage.

It owns:

* occupied cells
* reserved cells
* placement validation
* occupancy queries

It does not own movement meaning or puzzle rules.

## Shape System

The Shape System defines multi-cell footprints.

It owns:

* shape footprints
* multi-cell definitions
* shape offsets

Rotation is editor-only.

Runtime uses authored orientation.

## Wall Generation System

The Wall Generation System derives boundary structure from board layout.

It owns:

* boundary wall generation
* wall corners
* board boundary structure

It does not own gameplay rules or movement rules.

## Pathfinding System

The Pathfinding System answers path queries across the board.

It owns:

* path calculation
* route calculation using framework-safe board data and traversal inputs
* A* path queries
* Manhattan heuristic for 4-direction movement

It does not move objects or decide what paths mean.

---

# Grid vs Occupancy vs Shape vs Walls vs Pathfinding

These systems should remain separate by responsibility.

Grid:

* defines where cells exist
* answers coordinate and neighbor questions

Occupancy:

* tracks which cells are in use
* answers placement and usage questions

Shape:

* defines how many cells an item covers
* answers footprint questions

Walls:

* derive boundary structure from board layout
* answer board-edge structural questions

Pathfinding:

* performs route calculations using board structure and traversal information provided through framework-safe inputs

In short:

```text
Grid
-> defines where cells exist

Occupancy
-> tracks cell usage

Shape
-> defines footprint

Walls
-> derive board boundary structure

Pathfinding
-> calculates traversable routes
```

---

# Framework vs Game Module Ownership

Framework ownership:

* grids
* cells
* occupancy
* shapes
* walls
* pathfinding

Game module ownership:

* game objects
* blockers as puzzle meaning
* movement meaning
* collection rules
* puzzle rules

The framework remains game-agnostic.

It must not know:

* Hole
* Stickman
* Bus
* Door
* Brick

Those belong to game modules.

---

# System Collaboration

Core Board Systems are peers that collaborate through shared board data.

They should not be interpreted as a strict sequential pipeline.

A more accurate relationship is:

```text
Grid provides board structure
Shape provides footprint data
Occupancy provides usage state
Pathfinding consumes board and traversal inputs for route calculation
Walls derive boundary structure from board layout
```

Different callers may use these systems in different combinations.

The shared rule is:

* Core Board Systems return board data or board-query results
* game modules decide what those results mean

---

# Example Usage

## Drop Away

Core Board Systems may provide:

* board dimensions
* active and blocked cells
* occupancy checks
* shape footprints where shared structure is useful
* board boundary walls

The game module decides matching, collection, and level-completion meaning.

## Color Block Jam

Core Board Systems may provide:

* grid lookup
* occupancy validation
* multi-cell footprints
* path queries where needed
* derived wall boundaries

The game module decides exit rules and puzzle constraints.

## Hole People and Bus Jam

Core Board Systems may provide:

* pathfinding over traversable cells
* occupancy-aware reachability
* board-space structure

The game module decides what movement, collection, or routing means.

---

# What Does Not Belong Here

The following do not belong inside Core Board Systems:

* input handling
* drag behavior
* snapping behavior
* puzzle-specific movement rules
* collection rules
* save/load persistence
* progression data
* UI, audio, or visual feedback ownership

More specifically:

* Grid should not decide whether a move is allowed by puzzle rules.
* Occupancy should not decide why a placement matters.
* Shape should not decide gameplay meaning.
* Walls should not decide traversal rules beyond derived structure.
* Pathfinding should not decide what reaching a target means.

---

# Final Design Rule

Core Board Systems own board structure and board-query mechanics.

Grid defines where cells exist.

Occupancy tracks cell usage.

Shape defines footprint.

Walls are derived from board layout.

Pathfinding answers route questions.

Game modules decide what those structures and results mean.
