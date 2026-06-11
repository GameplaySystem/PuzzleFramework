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

Interaction Systems define how the player communicates with puzzle objects during gameplay.

These systems should be reusable across multiple puzzle games and should not contain game-specific rules.

---

# Design Decision: Interface-Based Interaction

Framework interaction objects will communicate through interfaces instead of concrete game classes.

The Input System should not know what a Hole, Brick, Bus, Stickman, or Door is.

Instead, interactive objects expose capabilities through interfaces such as:

- `ISelectable`
- `IDraggable`
- `IClickable`
- `IHoverable`

This keeps the framework decoupled, reusable, and easier to extend.

Status:

Approved

Input System

Drag Movement System

Grid Snap System

# Interaction Systems Summary

The Interaction Systems category contains:

1. Input System
2. Drag Movement System
3. Grid Snap System

The diagram for the connections

Player Input
    ->
Input System
    ->
Drag Movement System
    ->
Grid Snap System
    ->
Game Rules

Together, these systems allow the framework to support reusable player interaction without tying the framework to a specific puzzle game.

The key architecture rule is:

Framework systems detect, move, snap, and calculate.

Game modules decide what those actions mean.
