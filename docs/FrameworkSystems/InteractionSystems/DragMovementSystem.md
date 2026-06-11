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

```
Input System
    ↓
Drag Movement System
    ↓
Grid Snap System
    ↓
Game Rules
```

---

# Responsibilities

The Drag Movement System is responsible for:

- Moving draggable objects during drag
- Receiving world position from the Input System
- Converting drag position into movement position
- Supporting different drag movement modes
- Restricting drag movement when needed
- Supporting grid-aware movement
- Supporting shape-aware movement
- Providing valid/invalid movement preview data
- Preserving original position if movement fails

---

# Should Not Handle

The Drag Movement System should not handle:

- Raw input detection
- Raycasting selection
- Final snap placement
- Win conditions
- Lose conditions
- Object collection logic
- Pathfinding
- Game-specific puzzle rules
- Permanent occupancy changes

---

# Supported Movement Modes

The framework should support multiple drag movement modes.

## Free Drag

The object follows the pointer freely in world space.

Useful for:

- Simple drag prototypes
- UI-like dragging
- Non-grid objects

## Grid-Constrained Drag

The object follows the pointer but movement is interpreted through grid cells.

Useful for:

- Drop Away holes
- Color Block Jam bricks
- Level editor objects

## Axis-Constrained Drag

The object can only move on a specific axis.

Useful for:

- Sliding block puzzles
- Rush Hour-style movement
- Objects locked to rows or columns

## Cell-By-Cell Drag

The object moves from cell to cell instead of freely following the pointer.

Useful for:

- Grid puzzle objects
- Games where movement should feel discrete

## Shape-Based Drag

The object moves while considering all cells occupied by its shape.

Useful for:

- Multi-cell bricks
- Tetris-like objects
- Large puzzle pieces

---

# Drag Movement Data

The Drag Movement System should work with drag data instead of directly depending on a specific object type.

Example:

```csharp
public struct DragMoveRequest
{
    public Vector3 PointerWorldPosition;
    public Vector2Int CurrentGridPosition;
    public Vector2Int OriginalGridPosition;
}
```

Example result:

```csharp
public struct DragMoveResult
{
    public bool CanMove;
    public Vector3 TargetWorldPosition;
    public Vector2Int TargetGridPosition;
}
```

---

# Draggable Object Requirements

A draggable object should expose enough data for the movement system to work with it.

Possible interface:

```csharp
public interface IGridDraggable : IDraggable
{
    Vector2Int CurrentCell { get; }
    Vector2Int OriginalCell { get; }
    ShapeData Shape { get; }
}
```

This allows the Drag Movement System to move different object types without knowing whether they are holes, bricks, buses, or editor pieces.

---

# Movement Validation

The Drag Movement System can check framework-level movement restrictions.

Examples:

- Is the target cell inside the board?
- Is the target cell active?
- Is the target cell blocked?
- Does the shape fit inside the board?
- Would the shape overlap blocked cells?

The Drag Movement System should not check game-specific rules.

Examples of game-specific rules:

- Can this hole collect this stickman?
- Can this brick exit through this door?
- Can this bus pick up these passengers?
- Does this object color match another object?

Those rules belong to game modules.

---

# Occupancy Rule

During dragging, occupancy should be treated carefully.

The dragged object may need to temporarily ignore its own occupied cells.

Example:

A 2x2 brick starts on cells:

```
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

```
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

- Grid-based puzzle pieces
- Physical-feeling movement
- Drop Away-style holes

## Option 2: Allow Preview, Reject On Release

The object visually follows the pointer, but returns if released on an invalid cell.

Best for:

- Level editor placement
- Casual drag-and-drop systems

## Approved Default

The default movement behavior should be:

**Block movement at the last valid position.**

This feels better for puzzle gameplay because the object immediately communicates that movement is not allowed.

Status:

Approved for gameplay MVP

---

# Original Position Tracking

When dragging starts, the system should remember the object's original position.

This allows the object to return if needed.

Tracked values:

```csharp
private Vector3 originalWorldPosition;
private Vector2Int originalGridPosition;
private Vector2Int lastValidGridPosition;
private Vector3 lastValidWorldPosition;
```

---

# Example Flow

```
OnDragStart
    ↓
Store original position
    ↓
Store last valid position
    ↓
OnDrag
    ↓
Convert pointer world position to grid position
    ↓
Check framework movement validity
    ↓
If valid:
        Move object to target position
        Update last valid position
    ↓
If invalid:
        Keep object at last valid position
    ↓
OnDragEnd
    ↓
Send final position to Grid Snap System
```

---

# Example Use Cases

## Drop Away

A hole is dragged across the board.

The Drag Movement System handles:

- Moving the hole
- Keeping it inside the board
- Preventing movement into blocked cells
- Preventing movement through invalid framework cells

Game-specific Drop Away logic handles:

- Matching stickman collection
- Wrong-color blocking
- Hole capacity
- Win/loss rules

---

## Color Block Jam

A brick is dragged across grid cells.

The Drag Movement System handles:

- Shape-based movement
- Board boundary checks
- Blocked cell checks
- Last valid position tracking

Game-specific Color Block Jam logic handles:

- Door matching
- Exit behavior
- Brick completion
- Level success rules

---

## Level Editor

A shape is dragged around the board.

The Drag Movement System handles:

- Grid preview movement
- Shape placement preview
- Valid/invalid placement feedback

The editor module handles:

- Saving level data
- Creating objects
- Deleting objects
- Rotating shapes

---

# Design Decisions

## Drag Starts Immediately

Dragging begins immediately after selecting a draggable object.

No drag threshold is required for the first version.

Status:

Approved

---

## Default Movement Style

The first implementation should prioritize grid-constrained movement.

Free drag can exist, but most target puzzle games are grid-based.

Status:

Approved

---

## Invalid Movement Default

Invalid movement should block the object at the last valid position.

Status:

Approved

---

## Final Placement

The Drag Movement System does not finalize placement.

Final placement belongs to the Grid Snap System.

Status:

Approved

---

# MVP Scope

The first version of the Drag Movement System should support:

- Single selected draggable object
- Grid-constrained dragging
- Shape-aware movement
- Last valid position tracking
- Board boundary checks
- Blocked cell checks
- Ignoring the dragged object's own occupied cells
- Invalid movement blocking
- Final position handoff to Grid Snap System

---

# Future Extensions

The following features can be added later:

- Free drag mode
- Axis-locked drag mode
- Drag threshold
- Smooth interpolation
- Drag ghost preview
- Invalid cell highlight
- Custom movement constraints
- Physics-based dragging
- Multi-touch dragging
- Editor-only drag modes
- Tutorial-controlled drag locks

---

# Final Design Rule

The Drag Movement System previews where an object can move.

It can block invalid movement.

It does not permanently place the object.

It does not decide game-specific meaning.

Final placement is resolved by the Grid Snap System.
