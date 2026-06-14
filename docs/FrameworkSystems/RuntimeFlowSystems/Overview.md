# Runtime Flow Systems

## Document Metadata

Category:
- Runtime Flow Systems

Status:
- Approved

Parent:
- None

Related Documents:
- GameStateSystem.md
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

Runtime Flow Systems coordinate how a level starts, progresses, warns, completes, fails, and announces important runtime facts.

They exist to keep gameplay lifecycle concerns separate from:

* board structure
* interaction mechanics
* resource processing
* authored level content
* presentation reactions

This category defines runtime flow infrastructure.

It does not define puzzle-specific meaning.

---

# Why This Category Exists

All target games need some form of runtime lifecycle control.

Common examples:

* a level becomes active
* a timer starts or expires
* a success or failure state is reached
* independent systems need to react to a completed gameplay fact

If those concerns are handled ad hoc inside unrelated systems, the framework becomes harder to reason about.

Typical failure modes:

* lifecycle state owned by unrelated systems
* timers deciding puzzle rules directly
* UI and audio coupled tightly to gameplay systems
* unrelated systems discovering each other through scene searches
* commands and notifications getting mixed together

Runtime Flow Systems exist to separate:

* lifecycle ownership
* time-based flow
* cross-system notifications

from:

* puzzle-specific rule interpretation

---

# Systems In This Category

This category currently contains three systems:

1. Game State System
2. Timer System
3. Event System

Each system owns a different part of runtime flow.

## Game State System

The Game State System owns lifecycle state.

It answers questions like:

```text
Is the level idle?
Is gameplay active?
Is gameplay paused?
Is the level completed?
Is the level failed?
```

## Timer System

The Timer System owns shared time-based flow where a reusable timer concept exists.

It answers questions like:

```text
How much time remains?
Has the warning threshold been reached?
Has time expired?
```

## Event System

The Event System owns cross-system notification infrastructure.

It answers questions like:

```text
How can completed runtime facts be announced?
How can independent listeners react without tight coupling?
```

---

# Game State vs Timer vs Event

These systems should remain separate by responsibility.

Game State System:

* owns lifecycle state
* represents runtime phase or outcome
* may notify other systems when state changes
* should not become a generic notification bus

Timer System:

* owns time tracking
* reports warnings and expiration
* may notify other systems when time facts occur
* should not become a rule engine

Event System:

* publishes completed facts
* allows independent listeners
* should not become a command router

In short:

```text
Game State
-> owns lifecycle state

Timer
-> owns time-based flow

Event
-> owns cross-system notifications
```

---

# Commands vs Notifications

Runtime Flow Systems should keep commands and notifications clearly separated.

Commands ask another system to perform work.

Examples:

* create runtime object
* validate placement
* reserve occupancy
* save data
* load data

Commands should use explicit contracts, explicit references, or explicit dependencies.

Notifications announce completed facts.

Examples:

* `TimerExpired`
* `TimerWarning`
* `GameStarted`
* `GamePaused`
* `GameResumed`
* `GameWon`
* `GameLost`
* `ResourceCollected`
* `DragStarted`
* `DragEnded`
* `SnapSucceeded`
* `SnapFailed`
* `LevelCompleted`

Notifications should use the Event System.

Important boundary:

* commands request work
* notifications report completed facts

The Event System is for notifications, not commands.

---

# Direct Calls vs Events

Use direct calls when:

* one owner exists
* one system explicitly depends on another system
* work is being requested

Use events when:

* a completed fact is being announced
* multiple independent systems may react
* the publisher should not know listeners

Communication rule:

* internal ownership -> direct calls
* cross-system commands -> explicit references or contracts
* cross-system notifications -> Event System

Do not use:

* `FindObjectOfType`
* runtime scene searches
* unrelated global singletons as communication glue

---

# System Relationships

A common high-level relationship is:

```text
Gameplay owner requests state change
    ->
Game State System updates runtime phase
    ->
Timer may start, stop, pause, resume, warn, or expire
    ->
Completed facts are announced through Event System
    ->
Independent systems react
```

A more concrete flow may look like:

```text
Gameplay system decides fact
    ->
Game State System or Timer System updates owned state if relevant
    ->
Event System publishes completed fact
    ->
UI, audio, presentation, analytics, or game-module listeners react
```

The Event System should sit after the fact was already decided.

It should not be the place where the fact becomes true.

This means:

* Game State owns phase
* Timer owns time
* Event System announces completed cross-system facts

---

# Example Game Usage

## Drop Away

Runtime Flow Systems may provide:

* level state transitions
* timer countdown where applicable
* game started or game won notifications
* notifications such as `ResourceCollected` or `LevelCompleted`

The framework provides runtime flow mechanics.

The game module decides what collection or completion means.

## Sky Rush

Runtime Flow Systems may provide:

* timer warnings
* timer expiration notification
* level state transitions
* event-based reactions for UI, feedback, and audio

The Timer System does not decide boarding rules.

The Event System does not decide failure logic.

The Game State System does not decide whether timer expiration should mean failure.

## Bus Jam

Runtime Flow Systems may provide:

* drag-related notifications
* snap result notifications
* pause and resume state transitions
* state transitions for success or failure

The framework provides lifecycle and notification mechanics.

The game module decides puzzle-specific outcomes.

---

# What Does Not Belong Here

The following do not belong inside Runtime Flow Systems:

* puzzle-specific rules
* pathfinding logic
* board occupancy mutation rules
* save/load persistence
* runtime object construction
* service locator behavior
* object discovery through scene search

More specifically:

* Game State should not absorb unrelated gameplay rules.
* Timer should not decide win or lose meaning by itself.
* Event System should not execute commands.

Do not use Runtime Flow Systems as a replacement for explicit service ownership.

---

# Final Design Rule

Runtime Flow Systems manage lifecycle state, shared timer behavior, and cross-system notifications.

Game State owns lifecycle state.

Timer owns time-based flow.

Event System announces completed facts.

Game modules decide what those facts mean.
