# Timer System

## Document Metadata

Category:
- Runtime Flow Systems

Status:
- Approved

Parent:
- Overview.md

Related Documents:
- Overview.md
- GameStateSystem.md
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

The Timer System is a reusable runtime countdown or count-up time tracker.

It exists to track time, expose elapsed or remaining values, and announce time-related facts without absorbing game state ownership or puzzle-specific rule meaning.

The Timer System is about time.

It is not about what time means for a specific puzzle.

---

# Core Design Idea

The Timer System owns time tracking.

It publishes time-related facts.

Conceptually:

```text
Runtime flow starts or resumes timer
    ->
Timer System tracks elapsed or remaining time
    ->
Timer System publishes warning or expiration facts
    ->
Other systems react
```

The key boundary is:

* Timer System announces timer facts
* gameplay owners decide what those facts mean

---

# Responsibilities

The Timer System is responsible for:

* starting the timer
* stopping the timer
* pausing the timer
* resuming the timer
* resetting the timer
* exposing remaining time or elapsed time
* publishing timer warning events
* publishing timer expired events

The Timer System may support either:

* countdown behavior
* count-up behavior

depending on the later implementation shape.

The architecture rule is that time ownership remains centralized and generic.

---

# Should Not Handle

The Timer System should not handle:

* deciding lose state
* owning game state
* owning UI display
* owning audio or visual feedback
* knowing puzzle-specific rules
* saving or loading progression
* building levels
* deciding puzzle-specific failure meaning

The Timer System should not know:

* Hole
* Stickman
* Bus
* Door
* Brick

Those belong to game modules.

---

# Countdown vs Count-Up

The Timer System should be documented as a reusable timer concept rather than a single puzzle-specific countdown.

Possible shared uses:

* countdown until a limit
* elapsed-time tracking
* warning threshold detection

Questions it should answer:

```text
How much time remains?
How much time has elapsed?
Has the warning threshold been reached?
Has time expired?
```

The later implementation can choose the exact API shape.

The documentation boundary is that this is a shared time tracker, not a puzzle rule owner.

---

# Expiration Boundary

Timer expiration is a fact.

Lose condition is a rule.

Good:

```text
TimerSystem publishes TimerExpired
    ->
Game module or GameStateSystem rule owner decides whether this causes Lost
```

Bad:

```text
TimerSystem directly sets game to Lost
```

A timer may contribute to failure.

It must not own puzzle-specific failure meaning by itself.

---

# Event System Relationship

The Timer System should use the Event System for cross-system notifications.

Useful notifications may include:

* `TimerStarted`
* `TimerPaused`
* `TimerResumed`
* `TimerStopped`
* `TimerReset`
* `TimerWarning`
* `TimerExpired`

Internal ownership may use direct calls.

Cross-system notifications should use the Event System.

Do not use:

* `FindObjectOfType`
* runtime scene searches
* unrelated global singletons as communication glue

---

# Framework vs Game Module Ownership

Framework ownership:

* reusable timer infrastructure
* generic time tracking
* warning and expiration notification patterns
* start, stop, pause, resume, and reset boundaries

Game module ownership:

* whether a specific game uses a timer at all
* what warning thresholds mean
* whether expiration matters
* whether expiration causes failure, scoring changes, or some other puzzle-specific result

Framework owns time mechanics.

Game modules own gameplay interpretation.

---

# Data/Event Flow

A typical timer flow may look like:

```text
Owning runtime flow starts timer
    ->
Timer System tracks time
    ->
Timer reaches warning threshold or expiration
    ->
Event System publishes timer fact
    ->
UI, audio, presentation, or gameplay owners react
```

For a warning flow:

```text
Timer reaches warning threshold
    ->
Timer System publishes TimerWarning
    ->
UI and visual feedback react
```

For an expiration flow:

```text
Timer reaches expiration
    ->
Timer System publishes TimerExpired
    ->
Game module or flow owner decides whether to request Lost
```

The Timer System owns the fact.

It does not own the puzzle-specific consequence.

---

# Example Usage

## Sky Rush

The Timer System may provide:

* countdown tracking
* warning threshold notification
* expiration notification

The game module decides whether expiration should request a `Lost` transition.

## Drop Away

If a timed mode exists, the Timer System may provide:

* elapsed or remaining time
* warning notifications
* expiration fact publication

It does not decide collection rules or success logic.

## Bus Jam

The Timer System may provide:

* reusable shared timing mechanics
* pause and resume aware tracking
* event publication for warning and expiration

The game module decides how those facts affect the puzzle.

---

# Edge Cases

## Pause While Running

If the timer is paused:

* tracked time should stop advancing
* warning or expiration should not continue progressing until resumed

This preserves clear ownership of paused runtime flow.

## Resume After Pause

If the timer resumes:

* tracking should continue from preserved state
* the timer should not reset unless explicitly requested

## Expiration Repeatedly Firing

If the timer reaches expiration:

* duplicate expiration notifications should be avoided unless explicitly designed later
* the timer should not keep re-announcing expiration every update tick

The fact should be published cleanly.

## Warning Threshold Repeatedly Firing

If the timer crosses a warning threshold:

* the warning should behave like a meaningful fact
* duplicate spam should be avoided unless multiple thresholds are intentionally supported later

## Timer Drift Into Rule Logic

A common failure mode is adding direct failure logic into Timer because expiration feels close to losing.

Rule:

Timer owns time facts.

Gameplay owners own consequences.

---

# MVP Scope

The first version of the Timer System should support:

* start
* stop
* pause
* resume
* reset
* elapsed or remaining time queries
* warning fact publication
* expiration fact publication

The MVP should stay at the level of shared time tracking.

It should not absorb UI behavior, failure logic, or game state ownership.

---

# Future Extensions

The following can be added later if reuse pressure appears:

* multiple warning thresholds
* scaled or modified time flow
* timer result data
* editor or debug visualization
* analytics hooks
* multiple coordinated timers where a real shared use case exists

These should remain subordinate to the core boundary:

the Timer System owns time facts, not puzzle-specific consequences.

---

# Final Design Rule

The Timer System tracks time and publishes time-related facts.

It may publish warning and expiration notifications.

It does not decide lose state.

It does not own game state.

Game modules decide what timer facts mean.
