# Runtime Construction Systems

## Document Metadata

Category:
- Runtime Construction Systems

Status:
- Approved

Parent:
- None

Related Documents:
- LevelRuntimeBuilderSystem.md
- RuntimeObjectFactorySystem.md
- RuntimeConstructionValidationSystem.md
- ../ContentSystems/LevelDataSystem.md
- ../ContentSystems/LevelSaveLoadSystem.md
- ../ProgressionSystems/Overview.md

Depends On:
- Level Data System
- Core Board Systems

Used By:
- Drop Away
- Color Block Jam
- Sky Rush
- Hole People
- Bus Jam

## Purpose

Runtime Construction Systems convert loaded authored level data into a playable runtime level.

They sit between authored content and live gameplay state.

This category exists to keep runtime build flow separate from:

* authored content definition
* authored content persistence
* player progression
* puzzle-specific gameplay rules

---

# Why This Category Exists

Loading a `LevelDefinition` is not the same thing as having a playable level.

Authored data still needs to become:

* initialized board systems
* runtime objects
* registered runtime references
* a usable runtime context for gameplay systems

If that work is spread across random scene scripts, content loading, and game rules, the project will accumulate hidden coupling and unclear ownership.

Runtime Construction Systems exist to separate:

* build-safety validation
* construction coordination
* object creation

from:

* content authoring
* file persistence
* player progression
* gameplay outcome logic

---

# Systems In This Category

This category contains three systems:

1. Runtime Construction Validation System
2. Level Runtime Builder System
3. Runtime Object Factory System

Each system owns a different part of runtime construction.

## Runtime Construction Validation System

The Runtime Construction Validation System validates whether loaded authored level data can be safely built into runtime state.

It owns:

* build-safety validation
* placement validity checks
* shape fit checks
* overlap checks
* required construction data checks

It does not own puzzle quality or puzzle-specific rule meaning.

## Level Runtime Builder System

The Level Runtime Builder System coordinates runtime construction flow.

It owns:

* construction sequencing
* build order
* system initialization order
* factory coordination
* production of runtime level context or equivalent runtime state handoff

It does not own low-level object creation details.

## Runtime Object Factory System

The Runtime Object Factory System creates runtime objects and initializes them with construction-ready data.

It owns:

* object creation
* object initialization
* returning runtime references

It does not own overall construction order or validation policy.

---

# Validation vs Builder vs Factory

These systems should remain separate by responsibility.

Validation:

* answers whether the level can be built safely
* checks structural and construction readiness

Builder:

* coordinates the build flow
* decides when construction steps happen

Factory:

* creates runtime objects
* returns created runtime references

In short:

```text
Validation
-> checks build safety

Builder
-> coordinates construction

Factory
-> creates runtime objects
```

The category will become brittle if Builder and Factory are merged, or if Validation starts deciding gameplay quality.

---

# Core Runtime Construction Flow

The approved high-level flow is:

```text
LevelDefinition
    ->
Runtime Construction Validation
    ->
Level Runtime Builder
    ->
Runtime Object Factory
    ->
Playable Runtime Level
```

A more detailed typical flow is:

```text
LevelDefinition
    ->
Validate
    ->
Initialize Board Systems
    ->
Create Runtime Objects
    ->
Register Runtime State
    ->
Hand Off To Gameplay
```

This makes the boundary explicit:

* Content Systems produce and load the data
* Runtime Construction Systems build live runtime state from that data

---

# Runtime Context Concept

A future `RuntimeLevelContext` or equivalent concept is reasonable.

`RuntimeLevelContext` is a runtime handoff structure.

It is not a gameplay ownership system.

It may later contain:

* board systems
* occupancy state
* runtime objects
* runtime references
* shared runtime lookup data

That concept is useful because construction usually needs to hand off more than one runtime object.

However, implementation should not be locked yet.

This document approves the idea of a coordinated runtime context, not a final concrete type shape.

---

# Validation Timing

Validation should happen before construction.

The default rule is:

* validate first
* build second

That avoids partial construction and harder-to-debug failure states.

Post-build validation may exist later as debug support, but it should not replace pre-build validation.

The key distinction is:

* pre-build validation protects construction flow
* post-build validation can help inspect debug issues

---

# Runtime Construction vs Content Systems

Content Systems own authored level content.

Runtime Construction Systems own conversion of authored level content into runtime state.

Boundary:

* Content Systems define, edit, save, and load `LevelDefinition`
* Runtime Construction Systems consume loaded `LevelDefinition` data and build playable runtime state

Runtime Construction Systems must not take ownership of:

* level file persistence
* authored level schema definition
* editor authoring workflows

---

# Runtime Construction vs Progression Systems

Runtime Construction Systems build levels.

Progression Systems track player-owned state.

Runtime Construction Systems must not own:

* unlocked levels
* stars
* coins
* profile state
* settings
* progression save or load

These are different lifecycle concerns.

Level construction is runtime setup.

Progression is player state management.

---

# Framework vs Game Module Ownership

Framework ownership:

* construction flow
* construction validation patterns
* factory patterns
* framework-safe runtime build coordination

Game module ownership:

* puzzle-specific runtime objects
* puzzle-specific validation extensions
* puzzle-specific construction adapters
* puzzle-specific meaning of constructed objects

The framework must remain unaware of:

* Hole
* Stickman
* Bus
* Door
* Brick

Those belong to game modules.

Framework can provide the socket or contract.

Game modules provide puzzle-specific adapters and concrete content interpretation.

---

# What Does Not Belong Here

The following do not belong inside Runtime Construction Systems:

* level file save or load
* authored level structure definition
* player progression
* puzzle-specific win conditions
* puzzle-specific lose conditions
* puzzle-specific solvability judgments
* presentation ownership
* audio ownership
* UI ownership

More specifically:

* Validation should not judge whether a level is a good puzzle.
* Builder should not become an object factory.
* Factory should not decide build order.

---

# Future Extensions

Possible future extensions include:

* stronger runtime context abstractions
* debug-time post-build validation
* factory registries
* adapter discovery patterns
* pooled runtime object creation where needed
* construction diagnostics and reports

These should remain subordinate to the core category boundary:

Runtime Construction Systems build runtime levels from authored data.

They do not own content authoring, progression, or puzzle-specific rules.

---

# Final Design Rule

Runtime Construction Systems convert loaded authored level data into playable runtime state.

Validation checks whether the level can be built safely.

Builder coordinates construction.

Factory creates runtime objects.

Content Systems own authored data.

Progression Systems own player progress.
