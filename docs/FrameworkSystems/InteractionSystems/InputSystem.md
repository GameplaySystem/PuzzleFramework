## Purpose

The Input System detects player input and translates it into high-level interaction calls for gameplay objects.

It acts as the bridge between raw player input and framework interaction interfaces.

The Input System should detect player intent only.

It should not contain puzzle-specific rules.

---

# Design Decisions

## Selection Method

Objects are selected through pointer input.

On desktop, this means mouse click.

On mobile, this means screen touch.

Status:

Approved

---

## Raycasting

The Input System will use camera-based raycasting to detect interactable world objects.

The raycast should only check valid interaction layers.

This prevents the Input System from accidentally selecting background objects, visual-only objects, UI elements, or non-interactable gameplay objects.

Status:

Approved

---

## Platform Support

The Input System must support both:

- Mouse input for editor/testing
- Touch input for mobile builds

The same interaction flow should work for both input types.

Status:

Approved

---

## Unity Input System

The framework will use Unity's New Input System.

The old input approach uses methods such as:

```csharp
Input.GetKeyDown(KeyCode.A)
Input.GetMouseButtonDown(0)
```

The New Input System uses Input Actions instead.

Example actions:

- Pointer Press
- Pointer Position
- Pointer Delta
- Pointer Release

This allows mouse and touch input to be handled through the same action-based flow.

Status:

Approved

---

## Selection Limit

Only one object can be selected at a time.

The framework does not support multi-selection by default.

Status:

Approved

---

## Drag Start

Dragging can begin immediately after pressing/selecting a draggable object.

No drag threshold is required for the first version.

A drag threshold may be added later if accidental drags become a problem.

Status:

Approved

---

## Deselection

For draggable objects, deselection happens when the player releases input.

Example:

- Player presses object.
- Object becomes selected.
- Player drags object.
- Player releases input.
- Object is deselected.

For click-only objects, selection may be temporary and does not require a separate deselection step.

Status:

Approved

---

## Cancellation

Explicit cancellation is not required for the first version.

The main interaction flow is:

- Press
- Optional drag
- Release

Cancellation can be added later if needed for special cases such as pause menus, invalid states, or tutorial interruptions.

Status:

Approved for MVP

---

## Communication Style

The Input System will directly call interaction interfaces on selected objects.

Example:

```csharp
selectable.OnSelected();
draggable.OnDragStart();
draggable.OnDrag(worldPosition);
draggable.OnDragEnd();
```

This is simpler and easier to debug than an event-heavy architecture.

Events can be added later for secondary systems such as:

- Sound effects
- Haptics
- Analytics
- Tutorial steps
- Visual feedback

Status:

Approved

---

## UI Blocking

The Input System should not select or drag world objects when the pointer is over UI.

Before raycasting into the game world, the Input System should check whether the pointer is currently over a UI element.

This prevents problems such as:

- Pressing a UI button and accidentally selecting a board object
- Dragging gameplay objects while interacting with menus
- Touching popup UI and triggering world interaction behind it

Status:

Approved

---

# Responsibilities

The Input System is responsible for:

- Reading pointer press
- Reading pointer release
- Reading pointer position
- Detecting selected object
- Raycasting from the camera into the world
- Checking interaction interfaces
- Calling selection interfaces
- Calling drag interfaces
- Ignoring world input when UI is being touched

---

# Should Not Handle

The Input System should not handle:

- Object movement rules
- Grid validation
- Snap logic
- Pathfinding
- Win conditions
- Lose conditions
- Game-specific rules
- Object-specific gameplay behavior
- Visual animation logic
- Sound effects
- Haptic feedback

---

# Required Interfaces

## ISelectable

Used by objects that can be selected.

```csharp
public interface ISelectable
{
    void OnSelected();
    void OnDeselected();
}
```

---

## IDraggable

Used by objects that can be dragged.

```csharp
public interface IDraggable
{
    void OnDragStart();
    void OnDrag(Vector3 worldPosition);
    void OnDragEnd();
}
```

---

## IClickable

Used by objects that react to a simple click/tap.

```csharp
public interface IClickable
{
    void OnClicked();
}
```

---

# Input Flow

## Click Flow

```
Pointer Press
    ↓
Check UI Blocking
    ↓
Raycast World
    ↓
Find IClickable / ISelectable
    ↓
Call OnSelected
    ↓
Pointer Release
    ↓
Call OnClicked if no drag happened
    ↓
Call OnDeselected
```

---

## Drag Flow

```
Pointer Press
    ↓
Check UI Blocking
    ↓
Raycast World
    ↓
Find IDraggable
    ↓
Call OnSelected
    ↓
Call OnDragStart
    ↓
Pointer Move
    ↓
Call OnDrag(worldPosition)
    ↓
Pointer Release
    ↓
Call OnDragEnd
    ↓
Call OnDeselected
```

---

# Data Tracked By Input System

The Input System should track:

```csharp
private ISelectable currentSelectable;
private IDraggable currentDraggable;
private IClickable currentClickable;

private bool isPointerDown;
private bool isDragging;
private Vector2 pointerScreenPosition;
```

---

# Raycasting Rule

The Input System should raycast from the main gameplay camera using the current pointer screen position.

Only objects on valid interaction layers should be checked.

Example interaction layer:

```
Interactable
```

This keeps interaction detection clean and prevents accidental selection.

---

# UI Blocking Rule

If the pointer is over UI, the Input System should ignore world selection.

Example:

```
Pointer Press
    ↓
Is pointer over UI?
    ↓
Yes → Stop
No  → Continue world raycast
```

---

# MVP Scope

The first version of the Input System will support:

- Single pointer input
- Mouse testing
- Mobile touch input
- Single object selection
- Immediate drag start
- Release-based deselection
- Camera raycasting
- UI blocking
- Direct interface calls

---

# Future Extensions

The following features are not required for the first version but may be added later:

- Drag threshold
- Multi-touch support
- Multi-selection support
- Input cancellation
- Input lock/unlock states
- Tutorial-controlled input
- Event-based interaction broadcasting
- Custom interaction priority
- Long press detection
- Hover support for desktop
- Gesture support

---

# Final Design Rule

The Input System detects intent.

It does not decide meaning.

Game objects expose what they can do through interfaces.

Game-specific systems decide what should happen after interaction.