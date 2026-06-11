# Pathfinding System

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
- CellOccupancySystem.md

Depends On:
- GridSystem.md
- CellOccupancySystem.md

Used By:
- Hole People
- Bus Jam
- Sky Rush

## Purpose

The Pathfinding System finds valid paths across the board.

It allows framework and game-specific systems to ask whether one cell can reach another cell.

The system should calculate paths only.

It should not move objects directly.

---

# Core Design Idea

The Pathfinding System receives a path request.

It checks the board.

It returns a path result.

The caller decides what to do with that result.

```text
Game System
    ->
Pathfinding System
    ->
Path Result
    ->
Movement / Animation / Game Rules
```

---

# Algorithm Decision

A* will be used as the primary pathfinding algorithm.

A* is suitable because the framework uses cell-based boards and usually needs shortest valid paths.

Status:

Approved

---

# Responsibilities

The Pathfinding System is responsible for:

- Finding paths between cells
- Checking whether cells are traversable
- Supporting blocked cells
- Supporting occupied cells
- Supporting inactive cells
- Returning path results
- Returning failure when no path exists
- Supporting 4-directional movement by default
- Supporting reusable path queries for multiple games

---

# Should Not Handle

The Pathfinding System should not handle:

- Raw input
- Object selection
- Drag movement
- Grid snapping
- Animation
- Object spawning
- Win conditions
- Lose conditions
- Sound effects
- Visual effects
- Game-specific success/failure rules

---

# Default Movement Rule

The framework default is 4-directional movement.

Allowed directions:

- Up
- Down
- Left
- Right

Diagonal movement is not supported by default.

Diagonal movement may be added later if a game requires it.

Status:

Approved

---

# Path Request

The Pathfinding System should receive a request describing the path query.

Example:

```csharp
public struct PathRequest
{
    public Vector2Int StartCell;
    public Vector2Int TargetCell;
}
```

Future versions may include additional options.

Example:

```csharp
public struct PathRequest
{
    public Vector2Int StartCell;
    public Vector2Int TargetCell;
    public bool CanPassThroughOccupiedCells;
    public bool CanPassThroughBlockedCells;
}
```

---

# Path Result

The Pathfinding System should return path data instead of moving an object.

Example:

```csharp
public struct PathResult
{
    public bool PathFound;
    public List<Vector2Int> Cells;
}
```

The result allows the caller to decide what happens next.

Examples:

- Move object along path
- Animate stickman walking
- Reject interaction
- Trigger collection
- Show feedback
- Retry another target

---

# Traversability Rules

The Pathfinding System can check framework-level traversability.

A cell is traversable if:

- It is inside the board
- It is active
- It is not blocked
- It is not occupied by an obstacle, unless the request allows it

The system should not check game-specific rules.

Examples of game-specific rules:

- Does this stickman match the bus color?
- Does this hole have capacity?
- Is this brick allowed to exit?
- Should this unit be collected?
- Should this door accept this object?

---

# Board Integration

The Pathfinding System should read board and cell data from the framework board systems.

It should not own board data itself.

Required board information:

- Grid width
- Grid height
- Cell active/inactive state
- Cell blocked state
- Cell occupancy state

```text
Board System
    ->
Cell Data
    ->
Pathfinding System
```

---

# Occupancy Integration

The Pathfinding System should be able to consider occupancy.

Some objects block paths.

Some objects may be ignored depending on the request.

Example:

A stickman path should not pass through blocked cells.

A path check may optionally ignore the moving object's own current cell.

Future requests may support custom ignored objects.

---

# Heuristic

For 4-directional grid movement, the default heuristic should be Manhattan Distance.

```text
Distance = Abs(current.x - target.x) + Abs(current.y - target.y)
```

This works well because diagonal movement is not allowed.

Status:

Approved

---

# Path Output Rule

The returned path should include the ordered cell list from start to target.

Example:

```text
Start
    ->
Cell A
    ->
Cell B
    ->
Target
```

The caller can decide whether to include or skip the start cell during movement animation.

---

# Runtime Use Cases

## Hole People

The system can check whether:

- A person can reach a hole
- A hole can reach a board position
- A path exists before triggering movement

Game-specific systems still decide:

- Hole capacity
- Queue order
- Collection rules
- Win/loss state

---

## Bus Jam

The system can check whether:

- A stickman can reach a bus
- A bus can leave the board
- A path is blocked by other objects

Game-specific systems still decide:

- Passenger color matching
- Bus capacity
- Boarding order
- Completion rules

---

## Sky Rush

The system can check whether:

- Stickmen can move from door queues to buses
- A bus route is clear
- A target pickup cell is reachable

Game-specific systems still decide:

- Which stickman should move first
- Which bus accepts which passenger
- Queue behavior
- Level completion

---

# Failure Cases

The Pathfinding System should return failure when:

- Start cell is invalid
- Target cell is invalid
- Target cell is unreachable
- No traversable route exists
- Board data is missing
- Start or target is outside the board

Failure should not automatically trigger gameplay behavior.

The caller decides what failure means.

---

# Performance Notes

Most target puzzle games use small or medium-sized boards.

For MVP, a straightforward A* implementation is acceptable.

Optimization can be added later if needed.

Possible future optimizations:

- Reusing node data
- Priority queue optimization
- Path cache
- Early exit conditions
- Limiting search area
- Burst/ECS version for large boards

---

# Design Decisions

## Primary Algorithm

A* is the primary pathfinding algorithm.

Status:

Approved

---

## Default Movement

The default pathfinding movement is 4-directional.

Status:

Approved

---

## Default Heuristic

The default heuristic is Manhattan Distance.

Status:

Approved

---

## Output Style

The system returns path data.

It does not move objects.

Status:

Approved

---

## Board Ownership

The Pathfinding System reads board data.

It does not own board data.

Status:

Approved

---

# MVP Scope

The first version of the Pathfinding System should support:

- A* pathfinding
- 4-directional movement
- Manhattan Distance heuristic
- Active cell checks
- Blocked cell checks
- Occupied cell checks
- Start cell validation
- Target cell validation
- Path success result
- Path failure result
- Ordered path cell list

---

# Future Extensions

The following features can be added later:

- Diagonal movement
- Weighted cells
- Custom traversal costs
- Ignored object lists
- Path caching
- Multiple target search
- Nearest reachable cell search
- Flow field pathfinding
- Jump point search
- Editor path preview
- Debug path visualization
- Async pathfinding
- Burst/ECS optimization

---

# Final Design Rule

The Pathfinding System answers one question:

Can this cell reach that cell, and through which cells?

It returns the answer as data.

It does not decide what the answer means.
