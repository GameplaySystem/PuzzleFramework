# Drag Movement System

## Document Metadata

Category:
- Interaction Systems

Status:
- Approved

Parent:
- Overview.md

Related Documents:
- Overview.md
- InputSystem.md
- GridSnapSystem.md
- ../CoreBoardSystems/GridSystem.md

Depends On:
- InputSystem.md
- ../CoreBoardSystems/GridSystem.md

Used By:
- Drop Away
- Color Block Jam
- Sky Rush

## Purpose

The Drag Movement System controls how draggable objects move while the player is dragging them.

It receives drag position data from the Input System and moves the selected object according to framework-level movement rules.

The Drag Movement System should preview movement during dragging.

It should not finalize placement.

---

# Core Design Idea

The Input System detects intent.

The Drag Movement System handles movement preview.

The Grid Snap System resolves final placement.

Game-specific systems decide what the movement means.

```text
Input System
    ->
Drag Movement System
    ->
Grid Snap System
    ->
Game Rules
```

---

# Responsibilities

The Drag Movement System is responsible for:

* moving draggable objects during drag
* receiving world position from the Input System
* converting drag position into movement position
* supporting different drag movement modes
* restricting drag movement when needed
* supporting grid-aware movement
* supporting shape-aware movement
* providing valid or invalid movement preview data
* preserving original position if movement fails

---

# Should Not Handle

The Drag Movement System should not handle:

* raw input detection
* raycasting selection
* final snap placement
* win conditions
* lose conditions
* object collection logic
* pathfinding
* game-specific puzzle rules
* permanent occupancy changes

---

# Supported Movement Modes

The framework should support multiple drag movement modes.

## Free Drag

The object follows the pointer freely in world space.

Useful for:

* simple drag prototypes
* UI-like dragging
* non-grid objects

## Grid-Constrained Drag

The object follows the pointer but movement is interpreted through grid cells.

Useful for:

* Drop Away holes
* Color Block Jam bricks
* level editor objects

## Axis-Constrained Drag

The object can only move on a specific axis.

Useful for:

* sliding block puzzles
* Rush Hour-style movement
* objects locked to rows or columns

## Cell-By-Cell Drag

The object moves from cell to cell instead of freely following the pointer.

Useful for:

* grid puzzle objects
* games where movement should feel discrete

## Shape-Based Drag

The object moves while considering all cells occupied by its shape.

Useful for:

* multi-cell bricks
* Tetris-like objects
* large puzzle pieces

---

# Drag Movement Data

The Drag Movement System should work with drag-query data instead of directly depending on a specific object type.

Conceptually, a drag query should include:

* current pointer world position
* current grid position if grid-aware movement is active
* original drag origin position
* shape or footprint information when multi-cell movement matters

A drag result should include:

* whether the requested movement is valid
* the target preview world position
* the target preview grid position where applicable

This keeps the system reusable without tying it to puzzle-specific classes.

---

# Draggable Object Requirements

A draggable object should expose enough framework-safe information for the movement system to work with it.

That usually means access to:

* current position or cell
* original drag origin
* footprint data if shape-aware movement is required

This allows the Drag Movement System to move different object types without knowing whether they are holes, bricks, buses, or editor pieces.

---

# Movement Validation

The Drag Movement System can check framework-level movement restrictions.

Examples:

* is the target cell inside the board
* is the target cell active
* is the target cell blocked
* does the shape fit inside the board
* would the shape overlap blocked cells

The Drag Movement System should not check game-specific rules.

Examples of game-specific rules:

* can this hole collect this stickman
* can this brick exit through this door
* can this bus pick up these passengers
* does this object color match another object

Those rules belong to game modules.

---

# Occupancy Rule

During dragging, occupancy should be treated carefully.

The dragged object may need to temporarily ignore its own occupied cells.

Example:

A 2x2 brick starts on cells:

```text
(2,2), (2,3), (3,2), (3,3)
```

When checking if it can move, the system should not block the brick because of its own current cells.

Design rule:

The dragged object should be ignored during its own movement validation.

---

# Preview vs Commit

Dragging should preview movement only.

The Drag Movement System can move the object visually, but it should not permanently update final board occupancy.

Final occupancy should be updated after release by the Grid Snap System or placement system.

```text
During Drag:
Visual position changes
Temporary movement checks happen

On Release:
Grid Snap System resolves final position
Occupancy is committed or reverted
```

---

# Invalid Movement Behavior

If the player drags toward an invalid position, the framework can support different behaviors.

## Option 1: Block Movement

The object stops at the last valid position.

Best for:

* grid-based puzzle pieces
* physical-feeling movement
* Drop Away-style holes

## Option 2: Allow Preview, Reject On Release

The object visually follows the pointer, but returns if released on an invalid cell.

Best for:

* level editor placement
* casual drag-and-drop systems

## Approved Default

The default movement behavior should be:

**Block movement at the last valid position.**

This feels better for puzzle gameplay because the object immediately communicates that movement is not allowed.

---

# Original Position Tracking

When dragging starts, the system should remember the object's original position.

This allows the object to return if needed.

Typical tracked state includes:

* original world position
* original grid position where applicable
* last valid grid position
* last valid world position

---

# Example Flow

```text
OnDragStart
    ->
Store original position
    ->
Store last valid position
    ->
OnDrag
    ->
Convert pointer world position to grid position
    ->
Check framework movement validity
    ->
If valid:
        Move object to target position
        Update last valid position
    ->
If invalid:
        Keep object at last valid position
    ->
OnDragEnd
    ->
Send final position to Grid Snap System
```

---

# Framework vs Game Module Ownership

Framework ownership:

* movement preview
* reusable drag constraints
* board-boundary checks
* shape-aware movement checks
* last-valid-position behavior

Game module ownership:

* whether the dragged object is allowed to satisfy a puzzle rule
* whether drag should trigger collection, exit, boarding, or completion meaning
* any puzzle-specific consequences after placement

Framework owns preview and generic constraints.

Game modules own meaning.

---

# Example Use Cases

## Drop Away

A hole is dragged across the board.

The Drag Movement System handles:

* moving the hole
* keeping it inside the board
* preventing movement into blocked cells
* preventing movement through invalid framework cells

Game-specific Drop Away logic handles:

* matching stickman collection
* wrong-color blocking
* hole capacity
* win or loss rules

## Color Block Jam

A brick is dragged across grid cells.

The Drag Movement System handles:

* shape-based movement
* board boundary checks
* blocked cell checks
* last valid position tracking

Game-specific Color Block Jam logic handles:

* door matching
* exit behavior
* brick completion
* level success rules

## Level Editor

A shape is dragged around the board.

The Drag Movement System handles:

* grid preview movement
* shape placement preview
* valid or invalid placement feedback

The editor module handles:

* saving level data
* creating objects
* deleting objects
* rotating shapes

---

# Approved Defaults

The current approved defaults are:

* dragging begins immediately after selecting a draggable object
* no drag threshold is required for the first version
* the first implementation should prioritize grid-constrained movement
* invalid movement should block the object at the last valid position
* final placement belongs to the Grid Snap System

These defaults fit the current target games without pushing unnecessary abstraction into the MVP.

---

# MVP Scope

The first version of the Drag Movement System should support:

* single selected draggable object
* grid-constrained dragging
* shape-aware movement
* last valid position tracking
* board boundary checks
* blocked cell checks
* ignoring the dragged object's own occupied cells
* invalid movement blocking
* final position handoff to the Grid Snap System

---

# Future Extensions

The following features can be added later:

* free drag mode
* axis-locked drag mode
* drag threshold
* smooth interpolation
* drag ghost preview
* invalid cell highlight
* custom movement constraints
* physics-based dragging
* multi-touch dragging
* editor-only drag modes
* tutorial-controlled drag locks

---

# Final Design Rule

The Drag Movement System previews where an object can move.

It can block invalid movement.

It does not permanently place the object.

It does not decide game-specific meaning.

Final placement is resolved by the Grid Snap System.
