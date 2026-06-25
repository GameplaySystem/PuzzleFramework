# Framework Architecture

## Purpose

This document explains how the framework system categories relate to each other at a high level.

It does not redefine each system.

It summarizes:

* where data starts
* how runtime objects are built
* where gameplay truth lives
* how presentation reacts
* where player progress is stored
* how game modules extend the framework

The repository source of truth remains:

* `docs/PROJECT_STATE.md`
* `docs/FrameworkSystems/**`

---

# Core Architecture Rule

Framework systems define reusable mechanics, data flow, and boundaries.

Game modules define puzzle-specific meaning, rules, factories, adapters, and interpretation.

Dependency rule:

```text
Game Modules
    ->
Framework

Framework must not depend on Game Modules
```

The framework must remain unaware of:

* Hole
* Stickman
* Bus
* Door
* Brick

Those belong to game modules.

---

# Category Responsibilities

## Content Systems

Own:

* authored level data
* level definitions
* level authoring
* authored level persistence

Rule:

Content Systems own what the level is.

## Runtime Construction Systems

Own:

* converting loaded authored level data into runtime state
* build-safety validation
* construction coordination
* runtime object creation patterns

Rule:

Runtime Construction Systems build the playable runtime level from loaded authored data.

## Core Board Systems

Own:

* board structure
* board queries
* occupancy
* shapes
* derived boundaries
* path queries

Rule:

Core Board Systems own shared board structure and board-query mechanics.

## Interaction Systems

Own:

* input detection
* movement preview
* board-aligned snapping

Rule:

Interaction Systems translate player input into interaction requests and framework-level interaction behavior.

## Resource Processing Systems

Own:

* queueing
* temporary storage
* count limits

Rule:

Resource Processing Systems manage queues, buffers, and capacity.

## Runtime Flow Systems

Own:

* lifecycle state
* timer behavior
* cross-system notifications

Rule:

Runtime Flow Systems coordinate game state, timer, and event notifications.

## Presentation Systems

Own:

* player-facing visual response
* color identity mapping
* visual feedback reactions

Rule:

Presentation Systems react to gameplay truth and show feedback.

## Progression Systems

Own:

* player-owned progression state
* progression persistence

Rule:

Progression Systems store what the player has done.

---

# Full Category Relationship Map

The framework lifecycle is better understood as a category relationship map than as one strict chain.

Conceptually:

```text
Content Systems
    ->
Runtime Construction Systems
    ->
Runtime Level State

Runtime Level State uses:
    - Core Board Systems
    - Runtime Flow Systems
    - Resource Processing Systems
    - Interaction Systems

Presentation Systems react to runtime facts.
Progression Systems store player-owned outcomes.
```

Important clarification:

* Content Systems happen before runtime play begins
* Runtime Construction Systems prepare runtime state from loaded authored data
* Core Board Systems act as shared runtime board foundation
* Runtime Flow, Resource Processing, and Interaction Systems collaborate with active runtime state during gameplay
* Presentation reacts during gameplay
* Progression stores player-owned outcomes after gameplay facts are known

At runtime, many of these systems operate in parallel collaboration rather than one linear sequence.

---

# Data Lifecycle Diagram

The main data lifecycle is:

```text
Level Editor Foundation
    ->
LevelDefinition
    ->
Level Save Load System
    ->
Stored authored level data
    ->
Level Save Load System
    ->
Loaded LevelDefinition
    ->
Runtime Construction Validation
    ->
Level Runtime Builder
    ->
Runtime Object Factory
    ->
Runtime Level Context / Runtime State
    ->
Gameplay Systems
```

Then later:

```text
Gameplay outcome
    ->
Game module determines progression consequence
    ->
Player Progress Data System
    ->
Progress Save Load System
    ->
Stored player progress
```

Core data split:

```text
Level Data
-> what the level is

Progression Data
-> what the player has done
```

Those two data families must remain separate.

---

# Runtime Communication Diagram

During gameplay, system collaboration is better described like this:

```text
Player Input
    ->
Interaction Systems
    ->
Game module rule logic
    ->
Core Board Systems queries
    +
Resource Processing Systems
    +
Runtime Flow Systems state updates
    ->
Event System publishes completed facts
    ->
Presentation Systems react
    ->
Progression Systems update only after player-owned outcomes are decided
```

This is the main runtime truth rule:

* framework systems provide mechanics and state infrastructure
* game modules decide puzzle-specific meaning
* presentation reacts after truth is known
* progression stores player-owned results after outcomes are decided

Gameplay truth does not live inside Presentation Systems.

Gameplay truth also does not live inside generic save/load systems.

It lives in the active runtime state and the game-module rule layer using framework infrastructure.

---

# Framework vs Game Module Dependency Diagram

```text
Game Modules
    ->
Framework Categories
        -> Content Systems
        -> Runtime Construction Systems
        -> Core Board Systems
        -> Interaction Systems
        -> Resource Processing Systems
        -> Runtime Flow Systems
        -> Presentation Systems
        -> Progression Systems
```

Allowed:

```text
Game Modules -> Framework
```

Not allowed:

```text
Framework -> Game Modules
```

Practical meaning:

* framework may define contracts, sockets, patterns, and reusable system behavior
* game modules may provide adapters, validation extensions, runtime factories, rule systems, and puzzle-specific content payloads
* framework must not import or require puzzle-specific nouns

---

# Commands vs Notifications

Communication rules:

```text
Commands:
explicit references/contracts

Notifications:
Event System
```

Never use:

```text
FindObjectOfType
runtime scene searches
unrelated global singletons
```

Practical rule:

* use direct calls or explicit contracts when requesting work
* use the Event System when announcing completed facts that multiple listeners may react to

Examples:

* builder asks factory to create an object -> command
* timer expires and UI reacts -> notification
* drag system asks snap system to resolve placement -> command
* gameplay result triggers particles, UI, and sound -> notification

---

# Where Gameplay Truth Lives

Gameplay truth is produced at runtime through:

* game-module rules
* framework-owned runtime state
* framework queries and mechanics

Examples:

* Core Board Systems answer where movement or placement may happen structurally
* Resource Processing Systems answer queue, buffer, and capacity mechanics
* Runtime Flow Systems answer lifecycle and time facts
* game modules decide what those facts mean for the puzzle
* game modules may add puzzle-specific enterability checks on top of structural board truth without moving that meaning into framework systems
* game modules may trigger collection or state changes during drag before release-time snap alignment

Physics, interpolation, and other feel-oriented presentation layers may visualize or smooth gameplay.

They should not become the authoritative source of puzzle truth.

Presentation reacts to truth.

Progression records player-owned outcomes after truth is already known.

This prevents:

* presentation deciding rules
* persistence deciding gameplay
* construction owning progression

---

# Non-Linear Runtime Collaboration

The category flow is not a strict per-frame pipeline.

Examples of collaboration:

* Core Board Systems and Interaction Systems collaborate during drag and snap
* Core Board Systems and Resource Processing Systems may both support game-module logic during transfer or placement decisions
* Runtime Flow Systems and Presentation Systems collaborate through notifications
* Progression Systems may update only after Game State or game-module outcomes confirm completion

This means the architecture should be read as:

* ownership boundaries first
* collaboration paths second

not:

* one single execution chain for every gameplay moment

---

# Final Design Rule

The framework is organized by ownership boundaries, not by one giant central system.

Content Systems define authored data.

Runtime Construction Systems build runtime state from loaded authored data.

Core Board Systems provide board structure and board queries.

Interaction Systems translate player input into interaction behavior.

Resource Processing Systems provide queue, buffer, and capacity mechanics.

Runtime Flow Systems coordinate lifecycle, time, and notifications.

Presentation Systems react to gameplay truth.

Progression Systems store what the player has done.

Game modules provide puzzle-specific meaning on top of these framework categories.
