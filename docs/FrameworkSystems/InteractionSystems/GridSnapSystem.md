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

```
Drag Movement System
    ↓
Grid Snap System
    ↓
Occupancy System
    ↓
Game Rules
```

---

# Responsibilities

The Grid Snap System is responsible for:

- Converting world position to grid position
- Converting grid position to world position
- Finding the nearest valid cell
- Snapping objects to board-aligned positions
- Supporting single-cell objects
- Supporting multi-cell shapes
- Checking board boundaries
- Checking active/blocked cells
- Returning snap results
- Supporting runtime and level editor placement

---

# Should Not Handle

The Grid Snap System should not handle:

- Raw input detection
- Drag movement preview
- Pathfinding
- Win conditions
- Lose conditions
- Object collection logic
- Door matching logic
- Color matching logic
- Game-specific puzzle rules
- Visual animation logic

---

# Snap Request

The Grid Snap System should receive a request describing what needs to be snapped.

Example:

```csharp
public struct SnapRequest
{
    public Vector3 WorldPosition;
    public ShapeData Shape;
    public Vector2Int CurrentOriginCell;
}
```

---

# Snap Result

The Grid Snap System should return a result instead of directly deciding final behavior.

Example:

```csharp
public struct SnapResult
{
    public bool IsValid;
    public Vector2Int OriginCell;
    public Vector3 WorldPosition;
}
```

This allows the caller to decide what to do next.

Example:

- Place object
- Reject placement
- Return object to previous cell
- Play feedback
- Trigger game-specific logic

---

# World To Grid Conversion

The system should convert a world position into the nearest grid cell.

Example:

```
World Position
    ↓
Board Origin Offset
    ↓
Cell Size Conversion
    ↓
Rounded Grid Position
```

The board should own grid layout values such as:

- Board origin
- Cell size
- Grid width
- Grid height

The Grid Snap System should use this board data rather than hardcoded values.

---

# Grid To World Conversion

The system should convert a grid cell into the correct world position.

Example:

```
Grid Cell
    ↓
Cell Size Conversion
    ↓
Board Origin Offset
    ↓
World Position
```

This ensures snapped objects always align perfectly with the board.

---

# Shape-Aware Snapping

For multi-cell objects, the snap check should be based on the object's shape.

The origin cell represents the main anchor cell of the object.

The shape defines which additional cells are occupied relative to the origin.

Example 2x2 shape:

```
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

- Is the origin cell inside the board?
- Are all shape cells inside the board?
- Are all required cells active?
- Are any required cells blocked?
- Are any required cells occupied by another object?

The Grid Snap System should not check game-specific rules.

Examples of game-specific rules:

- Does the brick match the door color?
- Can this hole collect this stickman?
- Can this bus pick up this passenger?
- Is the selected object allowed to complete the level?

---

# Occupancy Rule

The Grid Snap System should validate against the Occupancy System.

However, it should not directly commit occupancy by default.

The result should tell the caller whether snapping is valid.

Then another system can decide whether to commit the placement.

```
Snap Check
    ↓
Snap Result
    ↓
Caller Decides
    ↓
Occupancy Commit
```

---

# Runtime Usage

During gameplay, the Grid Snap System is commonly used after dragging ends.

Example:

```
Player releases object
    ↓
Input System calls OnDragEnd
    ↓
Drag Movement System provides final position
    ↓
Grid Snap System calculates nearest valid snap
    ↓
Valid:
        Object moves to snapped position
        Occupancy updates
    ↓
Invalid:
        Object returns to previous position
```

---

# Level Editor Usage

In the level editor, the Grid Snap System is used while placing and moving objects.

Example:

```
Designer drags shape
    ↓
Grid Snap System previews target cell
    ↓
Valid placement:
        Show valid preview
    ↓
Invalid placement:
        Show invalid preview
    ↓
Designer releases
    ↓
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

# Design Decisions

## Grid-Based Snapping

The framework will prioritize grid-based snapping.

Status:

Approved

---

## Shape-Aware Validation

The Grid Snap System must support multi-cell shapes.

Status:

Approved

---

## Board Data Source

The Grid Snap System should use board/grid data instead of hardcoded cell sizes or positions.

Status:

Approved

---

## Occupancy Commit

The Grid Snap System should not directly commit occupancy by default.

It returns snap data.

The caller or placement system commits occupancy afterward.

Status:

Approved

---

## Runtime And Editor Support

The same Grid Snap System should be usable by both runtime gameplay and the level editor.

Status:

Approved

---

# MVP Scope

The first version of the Grid Snap System should support:

- World position to grid cell conversion
- Grid cell to world position conversion
- Single-cell snapping
- Multi-cell shape snapping
- Board boundary checks
- Active cell checks
- Blocked cell checks
- Occupancy checks
- Snap result return data
- Runtime usage
- Level editor usage

---

# Future Extensions

The following features can be added later:

- Snap preview ghost
- Invalid cell highlighting
- Custom snap anchors
- Partial shape snapping
- Rotation-aware snapping
- Door-adjacent snapping
- Path-based snapping
- Magnetic snap assist
- Editor-only snap modes
- Custom validation rules
- Smooth snap animation

---

# Final Design Rule

The Grid Snap System aligns objects to the board.

It validates framework-level placement.

It returns a result.

It does not decide game-specific meaning.

It does not permanently commit occupancy by default.
