# Game State System

## Document Metadata

Category:
- Runtime Flow Systems

Status:
- Approved

Parent:
- Overview.md

Related Documents:
- Overview.md
- TimerSystem.md
- EventSystem.md

Depends On:
- None

Used By:
- Drop Away
- Color Block Jam
- Sky Rush
- Hole People
- Bus Jam

## Purpose

The Game State System owns high-level runtime gameplay state.

It exists to track the current gameplay phase, allow valid transitions, reject invalid transitions, and coordinate top-level flow without absorbing puzzle-specific rules.

The Game State System should provide a shared framework-level lifecycle model, not a puzzle-specific rule engine.

---

# Core Design Idea

The Game State System owns runtime phase.

It does not own puzzle meaning.

Conceptually:

```text
Owning gameplay rule system decides a phase change is needed
    ->
Game State System validates transition
    ->
Game State System changes state
    ->
Event System notifies listeners
```

The important boundary is:

* rule systems decide when a transition should happen
* Game State System decides whether the transition is structurally valid
* Event System announces that the transition happened

---

# Responsibilities

The Game State System is responsible for:

* tracking current runtime game state
* defining the shared high-level phase model
* allowing valid state transitions
* preventing invalid state transitions
* notifying other systems when state changes
* coordinating high-level flow such as start, pause, resume, win, lose, and restart

Typical shared states may include:

* `NotStarted`
* `Playing`
* `Paused`
* `Won`
* `Lost`

The exact implementation shape can vary later.

The architectural rule is that one owner should hold the authoritative runtime phase.

---

# Should Not Handle

The Game State System should not handle:

* deciding puzzle-specific rules
* validating board movement
* collecting resources
* building levels
* saving or loading level data
* saving or loading player progression
* owning UI, audio, or visual feedback
* knowing puzzle-specific nouns or entities

The Game State System must not know:

* Hole
* Stickman
* Bus
* Door
* Brick

Those belong to game modules.

---

# State Model

The framework-level lifecycle model should stay small and generic.

A reasonable shared state model is:

```text
NotStarted
Playing
Paused
Won
Lost
```

These states answer broad runtime questions such as:

```text
Has play begun?
Is gameplay currently active?
Is gameplay paused?
Has the level ended in success?
Has the level ended in failure?
```

The Game State System should not expand into fine-grained puzzle-specific substate tracking unless reuse pressure proves that a shared abstraction is necessary.

---

# Valid Transition Rules

The Game State System should own structural transition validity.

Examples of likely valid transitions:

```text
NotStarted -> Playing
Playing -> Paused
Paused -> Playing
Playing -> Won
Playing -> Lost
Won -> NotStarted
Lost -> NotStarted
Paused -> NotStarted
```

Examples of likely invalid transitions:

```text
Won -> Playing
Lost -> Playing
NotStarted -> Won
NotStarted -> Lost
Paused -> Won without resumed flow owner deciding it
```

These are not puzzle rules.

They are lifecycle integrity rules.

This prevents unrelated systems from pushing the runtime state into incoherent combinations.

---

# Win/Lose Boundary

The Game State System may enter `Won` or `Lost` state.

It must not decide puzzle-specific win or lose conditions by itself.

Good:

```text
DropAwayRules determines all stickmen collected
    ->
requests GameStateSystem transition to Won
    ->
GameStateSystem changes state
    ->
Event System notifies listeners
```

Bad:

```text
GameStateSystem checks stickmen itself
```

The same rule applies across all games:

* gameplay rules decide when success or failure is true
* Game State System owns the resulting high-level phase

---

# Event System Relationship

The Game State System should use the Event System for cross-system notifications.

Useful notifications may include:

* `GameStateChanged`
* `GameStarted`
* `GamePaused`
* `GameResumed`
* `GameWon`
* `GameLost`

Internal ownership may use direct calls.

Cross-system notifications should use the Event System.

Do not use:

* `FindObjectOfType`
* runtime scene searches
* unrelated global singletons as communication glue

---

# Framework vs Game Module Ownership

Framework ownership:

* shared runtime phase model
* lifecycle transition rules
* generic start, pause, resume, win, lose, and restart flow boundaries
* notification patterns for state changes

Game module ownership:

* puzzle-specific win and lose conditions
* puzzle-specific reasons for pausing or resuming
* puzzle-specific interpretation of what a state change means
* puzzle-specific reactions layered on top of state changes

Framework owns lifecycle structure.

Game modules own gameplay meaning.

---

# Data/Event Flow

A typical flow may look like:

```text
Gameplay rule owner decides a state change is needed
    ->
Game State System validates requested transition
    ->
Game State System updates authoritative state
    ->
Event System publishes state-change notifications
    ->
UI, audio, feedback, and other listeners react
```

For a pause flow:

```text
Pause owner requests pause
    ->
Game State System transitions Playing -> Paused
    ->
Event System publishes GamePaused
    ->
UI and presentation react
```

For a win flow:

```text
Gameplay rule owner decides puzzle is complete
    ->
Game State System transitions Playing -> Won
    ->
Event System publishes GameWon
    ->
UI, audio, and progression listeners react
```

The Game State System owns the phase transition.

It does not own the puzzle-specific condition that triggered it.

---

# Example Usage

## Drop Away

The Game State System may provide:

* start of gameplay
* pause and resume support
* transition to `Won` after the game module decides all required collection is complete
* transition to `Lost` if a game-module failure rule decides failure occurred

It does not decide collection logic itself.

## Sky Rush

The Game State System may provide:

* active play state
* paused state
* won or lost state

The game module decides whether timer expiration, boarding outcomes, or puzzle state should request a transition.

## Bus Jam

The Game State System may provide:

* top-level lifecycle control
* restart flow
* shared notification of success or failure

The game module decides what counts as success or failure.

---

# Edge Cases

## Repeated Transition Requests

If a caller requests the current state again:

* the system should avoid redundant transition side effects
* duplicate notifications should be avoided unless explicitly required later

The authoritative owner should treat no-op transitions carefully.

## Invalid Transition Requests

If a caller requests an invalid lifecycle jump:

* the transition should be rejected safely
* the current state should remain unchanged

The system should preserve lifecycle integrity rather than trusting caller discipline.

## Multiple Competing Owners

A common failure mode is multiple unrelated systems trying to force state changes independently.

Rule:

One clear higher-level flow owner or coordinated set of owners should request state transitions.

The Game State System should not become a conflict-resolution layer for poorly owned gameplay logic.

## Game State Drift Into Rule Logic

Another failure mode is slowly moving puzzle-specific checks into Game State because it already owns `Won` and `Lost`.

Rule:

State ownership is not rule ownership.

The Game State System should remain generic.

---

# MVP Scope

The first version of the Game State System should support:

* one authoritative current runtime state
* a small shared lifecycle model
* valid and invalid transition handling
* start, pause, resume, win, lose, and restart flow support
* event publication for state changes

The MVP should stay at the level of runtime phase ownership.

It should not expand into puzzle-specific substate modeling.

---

# Future Extensions

The following can be added later if reuse pressure appears:

* richer state transition result data
* transition guards with clearer failure reasons
* editor or debug visualization
* replay or analytics hooks
* optional substate models for shared advanced use cases

These should remain subordinate to the main boundary:

the Game State System owns phase, not puzzle logic.

---

# Final Design Rule

The Game State System owns high-level runtime phase.

It allows valid transitions and rejects invalid ones.

It may enter `Won` or `Lost`.

It does not decide why a puzzle was won or lost.

Game modules decide puzzle-specific conditions.
