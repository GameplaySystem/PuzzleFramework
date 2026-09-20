# Input System

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
- GridSnapSystem.md
- ../CoreBoardSystems/GridSystem.md

Depends On:
- ../CoreBoardSystems/GridSystem.md (pointer-to-board projection extension)

Used By:
- Drop Away
- Color Block Jam
- Sky Rush
- Hole People
- Bus Jam

## Approved 2026-09-20 implementation extension

Both Drop The Man and Color Block Escape need board-plane pointer projection and preservation
of the selected object's pointer offset. Provide a narrow reusable geometry primitive for
screen ray/board-plane intersection and offset application, using the caller's camera/layout.
Game scene adapters keep platform polling, UI gating, hit targets, layers and puzzle meaning.
Use the existing generic `InputSystem` press/drag lifecycle where its capability contracts fit;
do not introduce a second generic lifecycle or make framework code depend on a game view.

## Purpose

The Input System detects player input and translates it into high-level interaction calls for gameplay objects.

It acts as the bridge between raw player input and framework interaction capabilities.

The Input System should detect player intent only.

It should not contain puzzle-specific rules.

---

# Core Design Idea

The Input System owns intent detection.

It determines what the player is trying to interact with and forwards that intent through framework-safe interaction contracts.

Conceptually:

```text
Pointer Input
    ->
Input System detects target and interaction type
    ->
Framework interaction capability is called
    ->
Other systems decide movement, placement, or gameplay meaning
```

The important boundary is:

* Input detects intent
* other systems interpret what that intent means

---

# Responsibilities

The Input System is responsible for:

* reading pointer press
* reading pointer release
* reading pointer position
* raycasting into the world
* identifying interactable targets
* calling framework interaction capabilities
* tracking the current selected interaction target
* ignoring world interaction when the pointer is over UI

---

# Should Not Handle

The Input System should not handle:

* object movement rules
* grid validation
* snap logic
* pathfinding
* win conditions
* lose conditions
* game-specific rules
* object-specific gameplay behavior
* visual animation logic
* sound effects
* haptic feedback

The Input System should not decide whether an interaction is good, valid, or useful in puzzle terms.

It should only detect and forward intent.

---

# Interaction Contract Model

The Input System should communicate through framework-safe interaction capabilities rather than concrete object types.

Typical capabilities include:

* selectable
* draggable
* clickable

Additional capabilities such as hoverable behavior can be added later if reuse pressure appears.

This keeps the framework decoupled from puzzle-specific nouns.

The Input System must not know whether the selected object is a hole, bus, brick, door, or any other game-specific entity.

---

# Approved Interaction Defaults

The current approved defaults are:

* selection happens through pointer input
* desktop uses mouse input
* mobile uses touch input
* only one object can be selected at a time
* dragging can begin immediately after selecting a draggable object
* release ends the default interaction flow
* explicit cancellation is not required for the MVP

These defaults keep the first framework version simple and consistent across target games.

---

# Platform Input Direction

The framework should support both:

* mouse input for editor and testing
* touch input for mobile builds

The intended design direction is a shared action-based input flow rather than separate gameplay logic per platform.

This keeps platform support aligned while preserving one interaction model.

---

# Raycasting And UI Blocking

The Input System should use camera-based raycasting to detect interactable world objects.

The raycast should only check valid interaction layers.

This prevents accidental selection of:

* background objects
* visual-only objects
* non-interactable gameplay objects

Before raycasting into the world, the Input System should also check whether the pointer is currently over UI.

This prevents problems such as:

* pressing a UI button and accidentally selecting a board object
* dragging gameplay objects while interacting with menus
* touching popup UI and triggering world interaction behind it

---

# Direct Calls vs Event Reactions

The Input System should directly call interaction capabilities on the active target.

That keeps the ownership chain explicit and easier to debug.

Secondary presentation or analytics reactions can listen through the Event System later if needed.

The Input System itself should not become an event-heavy ownership layer.

---

# Interaction Flow

## Click-Oriented Flow

A simple click-oriented interaction may look like:

```text
Pointer Press
    ->
Check UI blocking
    ->
Raycast world
    ->
Find selectable or clickable capability
    ->
Notify selected state
    ->
Pointer Release
    ->
Notify click if appropriate
    ->
Notify deselection if appropriate
```

## Drag-Oriented Flow

A drag-oriented interaction may look like:

```text
Pointer Press
    ->
Check UI blocking
    ->
Raycast world
    ->
Find draggable capability
    ->
Notify selected state
    ->
Notify drag start
    ->
Pointer Move
    ->
Notify drag update with world position
    ->
Pointer Release
    ->
Notify drag end
    ->
Notify deselection
```

---

# Data The Input System Tracks

The Input System will likely need to track interaction state such as:

* the current selected target
* whether the pointer is currently pressed
* whether a drag is currently active
* the current pointer position

That tracked state belongs to the Input System because it owns interaction intent flow.

---

# Framework vs Game Module Ownership

Framework ownership:

* intent detection
* raycasting rules
* UI blocking behavior
* interaction capability calling
* single-selection default behavior

Game module ownership:

* what selection means
* what dragging means
* what clicking means
* whether an interacted object can satisfy puzzle rules
* what consequences happen after interaction

Framework owns detection.

Game modules own meaning.

---

# Example Usage

## Drop Away

The Input System can detect:

* which hole the player touched
* whether dragging began
* when drag ended

Drop Away game logic still decides:

* whether a movement is useful
* whether stickmen are collected
* whether the level is won or lost

## Color Block Jam

The Input System can detect:

* which brick is selected
* whether the player is dragging
* when to hand off to movement and snapping systems

Color Block Jam logic still decides exit and completion meaning.

## Bus Jam And Hole People

The Input System can detect selection or click intent for movers, targets, or interactive board objects.

Game modules still decide what those interactions mean.

---

# Edge Cases

## Pointer Over UI

If the pointer is over UI, the Input System should not start world interaction.

## Temporary Interaction Interruptions

The MVP does not require a full cancellation system.

If interruption handling becomes necessary later, it should be added explicitly rather than assumed.

## Multiple Pointer Support

The MVP assumes single-pointer interaction.

Multi-touch can be added later if a game truly requires it.

---

# MVP Scope

The first version of the Input System will support:

* single pointer input
* mouse testing
* mobile touch input
* single object selection
* immediate drag start
* release-based deselection
* camera raycasting
* UI blocking
* direct interaction capability calls

---

# Future Extensions

The following features are not required for the first version but may be added later:

* drag threshold
* multi-touch support
* multi-selection support
* input cancellation
* input lock or unlock states
* tutorial-controlled input
* event-based secondary broadcasting
* custom interaction priority
* long press detection
* hover support for desktop
* gesture support

---

# Final Design Rule

The Input System detects intent.

It does not decide meaning.

Game objects expose what they can do through framework-safe interaction capabilities.

Game-specific systems decide what should happen after interaction.
