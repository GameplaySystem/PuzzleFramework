# Event System

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
- TimerSystem.md
- ../PresentationSystems/VisualFeedbackSystem.md

Depends On:
- None

Used By:
- Drop Away
- Color Block Jam
- Sky Rush
- Hole People
- Bus Jam

## Purpose

The Event System is a framework-level cross-system notification system.

It exists to announce facts that already happened so independent systems can react without tight coupling.

The Event System is not:

* a command system
* a decision system
* a service locator

---

# Core Design Idea

The Event System publishes completed facts.

It does not request work.

Conceptually:

```text
Gameplay fact already happened
    ->
Event System publishes notification
    ->
Independent listeners react
```

This keeps publishers unaware of which listeners exist, while preserving the rule that gameplay meaning was already decided elsewhere.

---

# Responsibilities

The Event System is responsible for:

* publishing completed facts
* allowing independent listeners to subscribe
* reducing direct cross-system coupling for notifications
* supporting game-agnostic event usage patterns
* helping presentation, audio, UI, analytics, and other passive listeners react after gameplay decisions

---

# Should Not Handle

The Event System should not handle:

* deciding gameplay rules
* validating actions
* owning game state
* deciding win conditions
* deciding lose conditions
* executing commands
* locating arbitrary services
* replacing explicit ownership or direct dependencies

Important boundary:

The Event System announces facts.

It does not make those facts true.

---

# Commands vs Notifications

The framework should separate commands from notifications clearly.

Commands ask another system to perform work.

Examples:

* create runtime object
* validate placement
* reserve occupancy
* save data
* load data

Commands should use explicit contracts, explicit references, or direct dependencies.

Notifications announce completed facts.

Examples:

* `TimerExpired`
* `TimerWarning`
* `ResourceCollected`
* `DragStarted`
* `DragEnded`
* `SnapSucceeded`
* `SnapFailed`
* `LevelCompleted`

Notifications should use the Event System.

Rule:

* commands request work
* notifications report completed facts

If a message is asking another system to do something, it should not go through the Event System.

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

Examples of good direct calls:

* a placement system asks occupancy to reserve cells
* a save button calls the save/load service directly
* a runtime builder calls a factory through an explicit contract

Examples of good event usage:

* `TimerExpired` -> UI reacts
* `TimerWarning` -> visual warning reacts
* `ResourceCollected` -> particles, audio, and UI react

Communication rule:

* internal ownership -> direct calls
* cross-system commands -> explicit references or contracts
* cross-system notifications -> Event System

Do not use:

* `FindObjectOfType`
* runtime scene searches
* unrelated global singletons as communication glue

---

# Architecture Boundaries

The Event System:

* publishes facts
* allows independent listeners
* reduces cross-system coupling

The Event System must not:

* decide gameplay rules
* validate actions
* own game state
* decide win or lose conditions
* execute commands

Good:

```text
TimerExpired
    ->
UI reacts
Audio reacts
Visual Feedback reacts
```

Bad:

```text
TimerExpired
    ->
Event System decides lose state
```

The lose condition may be announced.

It should not be computed by the Event System itself.

---

# Framework vs Game Module Ownership

Framework ownership:

* event infrastructure
* event usage rules
* game-agnostic event patterns

Game module ownership:

* puzzle-specific events
* puzzle-specific reactions
* puzzle-specific rule interpretation

The framework must remain unaware of game-specific nouns and puzzle entities.

It must not become aware of:

* Hole
* Stickman
* Bus
* Door
* Brick

Those belong to game modules.

---

# Data/Event Flow

The Event System should sit after a fact was already decided.

Typical flow:

```text
Gameplay system decides fact
    ->
Optional owned state changes happen
    ->
Event System publishes notification
    ->
Independent listeners react
```

For a timer-related flow:

```text
Timer reaches warning threshold
    ->
Timer System decides warning fact
    ->
Event System publishes TimerWarning
    ->
UI, audio, and feedback systems react
```

For a gameplay result flow:

```text
Snap logic decides result
    ->
Event System publishes SnapSucceeded or SnapFailed
    ->
Presentation and audio react
```

The publisher should know the fact.

It should not need to know every listener.

---

# Examples

## Timer Expiration

A shared timer reaches zero.

Good event use:

```text
Timer System decides TimerExpired
    ->
Event System publishes TimerExpired
    ->
UI reacts
Audio reacts
Visual Feedback reacts
Game module controller may also react
```

Bad event use:

```text
Timer System publishes command asking someone to decide failure
```

Failure meaning belongs to the owning gameplay logic, not to the Event System.

## Drag Lifecycle

During interaction:

* drag starts
* drag ends
* snap succeeds or fails

Those are useful notifications because multiple independent systems may respond:

* presentation
* audio
* analytics
* tutorial hints

The drag system should not need hard references to every one of those listeners.

## Level Completion

A gameplay owner decides the level is complete.

Then:

```text
LevelCompleted
    ->
UI reacts
Audio reacts
Visual Feedback reacts
Progression or summary logic may react through explicit ownership rules
```

The Event System announces completion.

It does not decide completion.

---

# Edge Cases

## Event Overuse

If every internal interaction becomes an event, ownership becomes harder to follow.

Rule:

Prefer direct calls when one owner clearly depends on another.

Use events only when the publisher should not know listeners.

## Commands Disguised As Events

A common failure mode is naming commands like notifications.

Examples of misuse:

* `RequestSaveData`
* `CreateRuntimeObject`
* `ValidatePlacement`

Those are requests for work, not completed facts.

They should use explicit contracts or direct dependencies.

## Event System As Service Locator

Another failure mode is using the Event System as general communication glue because direct architecture is unclear.

Rule:

If one system needs another system to do work, define the dependency explicitly.

Do not hide command routing behind event publication.

## Shared Mutable State In Payloads

If event publication starts passing around ownership-heavy mutable state that listeners can change arbitrarily, boundaries become unclear.

Rule:

Events should communicate facts cleanly.

Ownership of authoritative state should remain elsewhere.

---

# MVP Scope

The first version of the Event System should support:

* framework-level notification infrastructure
* publish and subscribe behavior
* clear usage rules for facts versus commands
* game-agnostic notification patterns
* support for independent listeners across systems

The MVP should stay small.

It does not need to become a generalized application bus or service-discovery mechanism.

---

# Future Extensions

The following can be added later if reuse pressure appears:

* scoped event channels
* debug event tracing
* event recording for tooling
* editor inspection helpers
* stronger event payload conventions

These should remain subordinate to the core rule:

the Event System is for notifications, not commands.

---

# Final Design Rule

The Event System announces completed facts across system boundaries.

It does not request work.

It does not decide gameplay meaning.

Use direct calls for owned dependencies and explicit commands.

Use the Event System for independent cross-system notifications.
