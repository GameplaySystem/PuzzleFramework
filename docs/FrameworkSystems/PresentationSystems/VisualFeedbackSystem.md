# Visual Feedback System

## Document Metadata

Category:
- Presentation Systems

Status:
- Approved

Parent:
- Overview.md

Related Documents:
- Overview.md
- ColorSystem.md

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

The Visual Feedback System is a reaction-only presentation system.

It exists to make gameplay results readable to the player through visual and audiovisual feedback.

It reacts after gameplay systems decide what happened.

It does not decide what happened.

---

# Core Design Idea

The Visual Feedback System responds to gameplay events or direct calls and turns them into player-facing feedback.

Conceptually:

```text
Gameplay result occurs
    ->
Visual Feedback System reacts
    ->
Player perceives outcome clearly
```

This keeps gameplay truth and player-facing feedback separate.

One gameplay result may trigger multiple presentation channels at the same time.

Example:

```text
ResourceCollected
    ->
Particle
Sound
Scale Punch
UI Counter Update
```

If all feedback is disabled, gameplay should still work correctly.

---

# Responsibilities

The Visual Feedback System is responsible for:

* responding to gameplay events or direct calls
* showing selection feedback
* showing drag feedback
* showing snap success and failure feedback
* showing invalid move feedback
* showing resource collection feedback
* showing boarding and exiting feedback
* showing timer warning feedback
* triggering animations, particles, highlights, sounds, shake, scale punch, trails, dissolve, and similar responses

It exists to improve clarity, emphasis, and perceived responsiveness.

---

# Should Not Handle

The Visual Feedback System should not handle:

* deciding whether an action is valid
* moving gameplay objects logically
* removing gameplay objects
* updating occupancy
* updating queues, buffers, or capacity
* deciding win state
* deciding lose state
* defining puzzle rules

Important boundary:

Visual animations may move rendered visuals.

Gameplay systems still own actual logical position and state.

---

# Events vs Direct Calls

Use direct calls when one clear owner exists.

Use events when multiple independent presentation systems need to react.

Good:

* `TimerExpired` event -> UI reacts
* `TimerWarning` event -> visual warning reacts
* `ResourceCollected` event -> particles, audio, and UI react

Bad:

* Event System decides lose condition
* Visual Feedback System decides collection result
* Color System decides match validity

Events and calls should communicate already-decided gameplay outcomes.

Presentation responds afterward.

---

# Framework vs Game Module Ownership

Framework ownership:

* generic feedback hooks
* reusable feedback trigger patterns
* shared reaction conventions

Game module ownership:

* which feedback should play for specific puzzle results
* puzzle-specific sequencing
* puzzle-specific art direction
* concrete mapping from gameplay outcome to feedback style

The framework can provide shared reaction infrastructure.

Game modules decide the meaning and style of specific reactions.

---

# Data/Event Flow

Typical direct-call flow:

```text
Gameplay system decides result
    ->
VisualFeedbackSystem.ShowFeedback(...)
    ->
Player-facing response plays
```

Typical event-based flow:

```text
Gameplay system emits event
    ->
Visual Feedback System reacts
    ->
Other presentation systems may also react
```

In both cases:

* gameplay decides truth
* presentation decides perception

---

# Example Usage

## Drag And Snap

During interaction, gameplay systems decide whether a placement is valid.

The Visual Feedback System may then show:

* drag emphasis
* snap success feedback
* snap failure feedback
* invalid move feedback

It does not decide whether the move is actually valid.

## Resource Collection

When gameplay confirms a resource was collected, the system may show:

* particles
* scale punch
* dissolve
* collection sound

It does not decide whether collection happened.

## Timer Warning

When gameplay or timer systems report a warning threshold, the system may show:

* flash
* urgency animation
* warning UI pulse
* sound cue

It does not decide when the timer rule becomes dangerous or terminal.

---

# Edge Cases

## Feedback Tries To Become Logic

If a feedback trigger starts deciding validity, ownership has drifted.

That logic belongs in gameplay systems.

## Visual Movement Confused With Logical Movement

A rendered object may shake, bounce, or slide visually.

That must not be mistaken for ownership of logical gameplay position.

## Event Overuse

If every simple feedback path becomes a global event, the system becomes harder to follow.

Use direct calls when ownership is obvious.

Use events only when multiple independent listeners genuinely need them.

---

# MVP Scope

The first version of the Visual Feedback System should support:

* direct-call and event-driven reactions
* selection feedback
* drag feedback
* snap success and failure feedback
* invalid move feedback
* resource collection feedback
* boarding and exiting feedback
* timer warning feedback

---

# Future Extensions

Possible future split candidates include:

* Animation System
* Highlight System
* Effect System

Do not create those systems yet.

Only split them into dedicated systems when multiple games need complex reusable behavior beyond simple feedback triggering.

---

# Final Design Rule

The Visual Feedback System reacts to gameplay truth.

It does not define gameplay truth.

Gameplay owns logical state.

Presentation owns how that state is perceived.
