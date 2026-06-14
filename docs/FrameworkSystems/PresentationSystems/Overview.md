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

Depends On:
- Gameplay state producers
- Gameplay event producers

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

Presentation Systems do not own gameplay rules, validation, progression, win or lose logic, object ownership, or data persistence.

---

# Systems In This Category

This category currently contains:

1. Color System
2. Visual Feedback System

`ColorSystem` provides framework-level color identity and presentation mapping.

`VisualFeedbackSystem` reacts to gameplay results and makes them visible to the player.

These two systems cover the current shared presentation needs without prematurely splitting into more specialized subsystems.

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

Presentation Systems must not:

* decide gameplay rules
* validate gameplay actions
* mutate gameplay truth
* own win or lose decisions
* own level data
* own progression data
* instantiate runtime level objects

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

Game module ownership:

* puzzle-specific meaning of colors
* puzzle-specific feedback selection
* puzzle-specific art direction
* puzzle-specific visual interpretation of gameplay results

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

They do not define gameplay truth.

If visuals disappear, gameplay should still work correctly.
