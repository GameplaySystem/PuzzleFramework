# Progression Systems

## Document Metadata

Category:
- Progression Systems

Status:
- Approved

Parent:
- None

Related Documents:
- PlayerProgressDataSystem.md
- ProgressSaveLoadSystem.md
- ../ContentSystems/Overview.md
- ../RuntimeConstructionSystems/Overview.md

Depends On:
- None

Used By:
- Drop Away
- Color Block Jam
- Sky Rush
- Hole People
- Bus Jam

## Purpose

Progression Systems own player-owned progression state.

They track what the player has done over time and persist that state separately from authored level content.

This category exists to keep player progress separate from:

* authored level data
* runtime level construction
* gameplay rule ownership

---

# Why This Category Exists

The framework needs a clean distinction between shipped or authored content and player-owned state.

The core rule is:

```text
Level Data
-> what the level is

Progression Data
-> what the player has done
```

If those concerns are merged, the project will blur:

* content authoring
* save-game state
* runtime setup
* player history

That makes data harder to reason about, harder to migrate, and easier to corrupt.

Progression Systems exist to separate:

* in-memory player progress state
* persistence of that player progress

from:

* authored level definitions
* runtime scene construction
* puzzle-specific gameplay outcomes

---

# Systems In This Category

This category contains two systems:

1. Player Progress Data System
2. Progress Save Load System

Each system owns a different part of progression.

## Player Progress Data System

The Player Progress Data System owns progression state in memory.

It may track examples such as:

* unlocked levels
* completed levels
* stars
* best scores
* best times
* attempt counts

It does not own authored level content.

## Progress Save Load System

The Progress Save Load System persists progression state.

It owns:

* saving player progress
* loading player progress
* persistence of progression data

It does not own authored level save/load or runtime construction.

---

# Progress Data vs Progress Save Load

These systems should remain separate by responsibility.

Player Progress Data:

* owns progression state in memory
* represents current player-owned progress

Progress Save Load:

* persists that progression state
* restores that progression state later

In short:

```text
Progress Data
-> owns player progression state

Progress Save Load
-> persists player progression state
```

This split keeps in-memory state modeling separate from persistence mechanics.

---

# Progression vs Content vs Runtime Construction

The category boundary must stay explicit.

Content Systems own:

* `LevelDefinition`
* authored level data

Progression Systems own:

* player progress

Runtime Construction Systems own:

* building runtime levels

Do not mix these responsibilities.

If Progression Systems start owning level definitions, content boundaries break.

If Progression Systems start building runtime levels, runtime construction boundaries break.

---

# Example Progression Data

Examples of progression data may include:

* level unlocked
* level completed
* star count
* best completion time
* best score
* total attempts

These are examples only.

The framework should not be locked to one rigid progression model too early.

The reusable idea is:

* player-owned progress exists
* that progress can be tracked
* that progress can be persisted

Puzzle-specific interpretation still belongs to game modules.

---

# Framework vs Game Module Ownership

Framework ownership:

* progression data patterns
* progression persistence patterns
* separation between player progress and authored content

Game module ownership:

* puzzle-specific progression meaning
* puzzle-specific reward interpretation
* puzzle-specific progression extensions

Framework owns the generic progression boundary.

Game modules own the meaning of specific progression outcomes.

---

# Data Flow

A typical progression flow may look like:

```text
Gameplay result
    ->
Game module decides progress outcome
    ->
Player Progress Data System updates player-owned state
    ->
Progress Save Load System persists player-owned state
```

On startup or resume:

```text
Persisted progress data
    ->
Progress Save Load System loads state
    ->
Player Progress Data System receives restored progress data
```

The important boundary is:

* game modules decide what happened
* progression systems store and persist what the player now owns

---

# What Does Not Belong Here

The following do not belong inside Progression Systems:

* authored level definitions
* level editor data
* runtime object construction
* puzzle-specific win or lose logic
* puzzle-specific validation
* board mechanics
* pathfinding
* drag or snap logic

More specifically:

* Progression Systems should not decide whether a level was won.
* Progression Systems should not decide whether a score is valid.
* Progression Systems should not define what a level contains.

---

# Future Extensions

Possible future extensions include:

* multiple profiles
* cloud sync
* migration support
* progression analytics
* richer stat tracking
* achievement or reward hooks

These should remain subordinate to the core boundary:

Progression Systems own player-owned state, not authored content or runtime construction.

---

# Final Design Rule

Progression Systems own player-owned progression state and persistence of that state.

Player Progress Data owns progression state in memory.

Progress Save Load persists progression state.

Content Systems own authored level data.

Runtime Construction Systems build runtime levels.
