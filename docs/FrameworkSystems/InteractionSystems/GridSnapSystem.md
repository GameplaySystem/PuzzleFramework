# Grid Snap System

## Document Metadata

Category:
- Interaction Systems

Status:
- Approved

Parent:
- Overview.md

Related Documents:
- Overview.md
- DragMovementSystem.md
- ../CoreBoardSystems/GridSystem.md
- ../CoreBoardSystems/ShapeSystem.md
- ../CoreBoardSystems/CellOccupancySystem.md

Depends On:
- ../CoreBoardSystems/GridSystem.md
- ../CoreBoardSystems/ShapeSystem.md
- ../CoreBoardSystems/CellOccupancySystem.md

Used By:
- Drop Away
- Color Block Jam
- Level Editor

## Purpose

The Grid Snap System converts world positions into clean grid positions.

It is responsible for placing objects onto valid board cells after dragging, spawning, editor placement, or other movement operations.

The Grid Snap System resolves final grid alignment.

---

# Core Design Idea

The Drag Movement System previews movement.

The Grid Snap System resolves final placement.

The Occupancy System commits the final occupied cells.

```text
Drag Movement System
    ->
Grid Snap System
    ->
Occupancy System
    ->
Game Rules
```

---

# Responsibilities

The Grid Snap System is responsible for:

* converting world position to grid position
* converting grid position to world position
* finding the nearest valid cell
* snapping objects to board-aligned positions
* supporting single-cell objects
* supporting multi-cell shapes
* checking board boundaries
* checking active and blocked cells
* returning snap results
* supporting runtime and level editor placement

---

# Should Not Handle

The Grid Snap System should not handle:

* raw input detection
* drag movement preview
* pathfinding
* win conditions
* lose conditions
* object collection logic
* door matching logic
* color matching logic
* game-specific puzzle rules
* visual animation logic

---

# Snap Request

The Grid Snap System should receive a request describing what needs to be snapped.

A snap request should conceptually contain:

* world position to evaluate
* shape or footprint data if multi-cell snapping is required
* current origin cell where relevant
* any framework-safe validation context needed for placement checks

---

# Snap Result

The Grid Snap System should return a result instead of directly deciding final behavior.

A snap result should conceptually contain:

* whether the snap is valid
* the resolved origin cell
* the resolved snapped world position

This allows the caller to decide what to do next.

Examples:

* place object
* reject placement
* return object to previous cell
* play feedback
* trigger game-specific logic

---

# World To Grid Conversion

The system should convert a world position into the nearest grid cell.

Conceptually:

```text
World Position
    ->
Board Origin Offset
    ->
Cell Size Conversion
    ->
Rounded Grid Position
```

The board should own grid layout values such as:

* board origin
* cell size
* grid width
* grid height

The Grid Snap System should use this board data rather than hardcoded values.

---

# Grid To World Conversion

The system should convert a grid cell into the correct world position.

Conceptually:

```text
Grid Cell
    ->
Cell Size Conversion
    ->
Board Origin Offset
    ->
World Position
```

This ensures snapped objects always align perfectly with the board.

---

# Shape-Aware Snapping

For multi-cell objects, the snap check should be based on the object's shape.

The origin cell represents the main anchor cell of the object.

The shape defines which additional cells are occupied relative to the origin.

Example 2x2 shape:

```text
Origin + (0,0)
Origin + (1,0)
Origin + (0,1)
Origin + (1,1)
```

The Grid Snap System should check all shape cells before returning a valid result.

---

# Validation Rules

The Grid Snap System can check framework-level placement rules.

Examples:

* is the origin cell inside the board
* are all shape cells inside the board
* are all required cells active
* are any required cells blocked
* are any required cells occupied by another object

The Grid Snap System should not check game-specific rules.

Examples of game-specific rules:

* does the brick match the door color
* can this hole collect this stickman
* can this bus pick up this passenger
* is the selected object allowed to complete the level

---

# Occupancy Rule

The Grid Snap System should validate against the Occupancy System.

However, it should not directly commit occupancy by default.

The result should tell the caller whether snapping is valid.

Then another system can decide whether to commit the placement.

```text
Snap Check
    ->
Snap Result
    ->
Caller Decides
    ->
Occupancy Commit
```

---

# Runtime Usage

During gameplay, the Grid Snap System is commonly used after dragging ends.

Example:

```text
Player releases object
    ->
Input System calls drag end
    ->
Drag Movement System provides final position
    ->
Grid Snap System calculates nearest valid snap
    ->
Valid:
        Object moves to snapped position
        Occupancy updates
    ->
Invalid:
        Object returns to previous position
```

---

# Level Editor Usage

In the level editor, the Grid Snap System is used while placing and moving objects.

Example:

```text
Designer drags shape
    ->
Grid Snap System previews target cell
    ->
Valid placement:
        Show valid preview
    ->
Invalid placement:
        Show invalid preview
    ->
Designer releases
    ->
Object is placed or rejected
```

---

# Invalid Snap Behavior

If snapping fails, the caller should decide what happens.

Common options:

## Return To Original Position

Best for gameplay objects.

## Stay At Last Valid Position

Best for grid-based dragging.

## Reject Placement

Best for level editor creation.

## Show Invalid Feedback

Best for both runtime and editor tooling.

Approved default for gameplay:

**Return or remain at the last valid position.**

Approved default for editor:

**Reject placement and show invalid feedback.**

---

# Approved Defaults

The current approved defaults are:

* the framework prioritizes grid-based snapping
* the system must support multi-cell shapes
* board or grid data should be the source of layout truth
* the system should not directly commit occupancy by default
* the same system should support both runtime gameplay and level editor placement

These defaults preserve reuse while keeping placement ownership explicit.

---

# Framework vs Game Module Ownership

Framework ownership:

* grid alignment
* placement conversion between world and board space
* framework-level placement validation
* shape-aware snap checks
* occupancy-aware validation queries

Game module ownership:

* whether a valid snap satisfies puzzle rules
* whether a snapped object exits, collects, boards, or completes something
* any puzzle-specific consequences after snap resolution

Framework owns alignment and validation.

Game modules own meaning.

---

# MVP Scope

The first version of the Grid Snap System should support:

* world position to grid cell conversion
* grid cell to world position conversion
* single-cell snapping
* multi-cell shape snapping
* board boundary checks
* active cell checks
* blocked cell checks
* occupancy checks
* snap result return data
* runtime usage
* level editor usage

---

# Future Extensions

The following features can be added later:

* snap preview ghost
* invalid cell highlighting
* custom snap anchors
* partial shape snapping
* rotation-aware snapping
* door-adjacent snapping
* path-based snapping
* magnetic snap assist
* editor-only snap modes
* custom validation rules
* smooth snap animation

---

# Final Design Rule

The Grid Snap System aligns objects to the board.

It validates framework-level placement.

It returns a result.

It does not decide game-specific meaning.

It does not permanently commit occupancy by default.
