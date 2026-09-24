# Presentation Systems

## Document Metadata

Category:
- Presentation Systems

Status:
- Approved

Parent:
- None

Related Documents:
- ColorSystem.md
- VisualFeedbackSystem.md
- ModularBoardVisualSystem.md
- FootprintMeshGenerationSystem.md
- ../CoreBoardSystems/WallGenerationSystem.md

Depends On:
- Gameplay state producers
- Gameplay event producers
- ../CoreBoardSystems/WallGenerationSystem.md

Used By:
- Drop Away
- Color Block Jam
- Sky Rush
- Hole People
- Bus Jam

## Purpose

Presentation Systems translate gameplay state and gameplay events into player-facing visuals.

They exist to improve readability, feedback, and player clarity without owning gameplay truth.

Gameplay answers:

```text
What happened?
```

Presentation answers:

```text
How should the player perceive what happened?
```

If all visuals are disabled, gameplay should still function correctly.

---

# Responsibilities

Presentation Systems are responsible for:

* reacting to gameplay state
* reacting to gameplay events
* improving readability
* providing visual feedback
* providing player-facing clarity
* playing animations, effects, highlights, and transitions where needed
* applying reusable static board-visual structure where framework-derived topology is available

Presentation Systems do not own gameplay rules, validation, progression, win or lose logic, object ownership, or data persistence.

---

# Systems In This Category

This category currently contains:

1. Color System
2. Visual Feedback System
3. Modular Board Visual System
4. Footprint Mesh Generation System

`ColorSystem` provides framework-level color identity and presentation mapping.

`VisualFeedbackSystem` reacts to gameplay results and makes them visible to the player.

`ModularBoardVisualSystem` converts framework-derived board boundaries into generic modular
cell-prefab visual state without deciding what the board means in a particular puzzle.

`FootprintMeshGenerationSystem` converts a generic connected grid footprint into one unified,
anchor-aware extruded presentation mesh without interpreting the entity that owns the footprint.

These systems cover the current shared presentation needs without mixing gameplay authority with
visual construction.

---

# Presentation Systems Boundary

Presentation Systems may:

* highlight
* animate
* tint
* flash
* shake
* dissolve
* transition
* play particles
* play sounds
* show timer warnings
* construct presentation-only board cell instances through explicit visual inputs
* apply derived wall and corner slot state

Presentation Systems must not:

* decide gameplay rules
* validate gameplay actions
* mutate gameplay truth
* own win or lose decisions
* own level data
* own progression data
* instantiate runtime level objects

Presentation-only cell instances are allowed when they are disposable views of already-defined
board structure. They must not become authoritative runtime gameplay objects.

Logical state remains elsewhere.

Presentation only changes how that state is perceived.

---

# Color System

The Color System is a framework-level presentation and data-mapping system.

Its job is to define reusable color identities and map those identities to visuals such as:

* colors
* materials
* palette entries
* theme values
* visual identifiers

Color identity is framework-level.

Color meaning is game-module-level.

Concrete visual assets remain owned by game modules.

Example:

* Framework can define `Red`.
* A game module decides whether `Red` means a red stickman, red bus, red door, or red brick.

The Color System must not decide:

* color matching rules
* boarding rules
* collection rules
* blocking logic

---

# Visual Feedback System

The Visual Feedback System is a reaction-only presentation system.

It responds to gameplay events or direct calls and shows player-facing feedback such as:

* selection feedback
* drag feedback
* snap success feedback
* snap failure feedback
* invalid move feedback
* resource collection feedback
* boarding feedback
* exiting feedback
* timer warning feedback

It may trigger:

* animations
* particles
* highlights
* sounds
* shake
* scale punch
* trails
* dissolve

It must not:

* decide whether an action is valid
* move gameplay objects logically
* remove gameplay objects
* update occupancy
* update queues, buffers, or capacity
* decide win or lose state

Visual animations may move rendered visuals.

Gameplay systems still own actual logical position and state.

---

# Modular Board Visual System

The Modular Board Visual System is a reusable structural-presentation system.

It consumes boundary facts already derived by the Core Board Wall Generation System and applies
them to a generic modular cell-visual contract.

It may:

* plan per-cell half-wall and corner slot state
* create presentation-only cell visual instances through explicit inputs
* apply and rebuild modular board visuals

It must not:

* decide which game-specific cells count as visible board space
* interpret blocked cells, obstacles, occupancy, or movement rules
* own concrete meshes, materials, or puzzle-specific prefabs
* create authoritative gameplay entities

The game module supplies the participation mapping and concrete art. The framework supplies the
reusable topology-to-visual behavior.

---

# Events vs Direct Calls

Use direct calls when one clear owner exists.

Use events when multiple independent presentation systems need to react.

Good examples:

* `TimerExpired` event -> UI reacts
* `TimerWarning` event -> visual warning reacts
* `ResourceCollected` event -> particles, audio, and UI react

Bad examples:

* Event System decides lose condition
* Visual Feedback System decides collection result
* Color System decides match validity

The event or call communicates that gameplay already decided something.

Presentation reacts afterward.

---

# Framework vs Game Module Ownership

Framework ownership:

* reusable color identities
* shared color-to-visual mapping patterns
* shared visual feedback triggers and presentation hooks
* reusable presentation conventions
* reusable conversion from board-boundary topology into modular cell-visual state
* generic modular board visual application contracts
* generic footprint-to-mesh generation and equivalent-mesh caching

Game module ownership:

* puzzle-specific meaning of colors
* puzzle-specific feedback selection
* puzzle-specific art direction
* puzzle-specific visual interpretation of gameplay results
* concrete board cell prefabs, meshes, and materials
* mapping puzzle-specific board data into visual participation

The framework provides presentation infrastructure.

Game modules decide what specific visual style and semantic meaning sit on top of it.

Concrete visual assets remain owned by game modules.

---

# Data/Event Flow

Presentation should consume gameplay outputs, not define them.

Conceptually:

```text
Gameplay State or Gameplay Event
    ->
Presentation System
    ->
Player-facing visual response
```

For color-driven presentation:

```text
Gameplay object exposes color identity
    ->
Color System maps identity to visuals
    ->
Rendered appearance becomes consistent
```

For feedback-driven presentation:

```text
Gameplay result occurs
    ->
Visual Feedback System reacts
    ->
Player perceives success, failure, warning, or emphasis
```

---

# Example Usage

## Drop Away

Presentation Systems may provide:

* color identity mapping for stickmen and holes
* collection feedback
* invalid move feedback
* selection and drag feedback
* modular floor, half-wall, convex-cap, and concave-elbow board visuals

They do not decide whether a hole may collect a stickman.

## Color Block Jam

Presentation Systems may provide:

* brick and door color mapping
* exit feedback
* blocked feedback
* highlight states

They do not decide whether a brick matches a door.

## Sky Rush

Presentation Systems may provide:

* bus and passenger color presentation
* boarding feedback
* timer warning feedback
* queue-related visual clarity

They do not decide who boards or when the timer causes failure.

---

# Future Extensions

Possible future split candidates include:

* Animation System
* Highlight System
* Effect System

Do not create those systems yet.

Only split them out when multiple games need complex reusable behavior beyond simple feedback triggering.

The current category should stay small until real reuse pressure exists.

---

# Final Design Rule

Presentation Systems react to gameplay truth and translate it into player-facing visuals.

Presentation Systems may also visualize framework-derived static structure without becoming the
authority that defines that structure.

They do not define gameplay truth.

If visuals disappear, gameplay should still work correctly.
