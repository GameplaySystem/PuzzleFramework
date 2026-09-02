# Player Progress Data System

## Document Metadata

Category:
- Progression Systems

Status:
- Approved

Parent:
- Overview.md

Related Documents:
- Overview.md
- ProgressSaveLoadSystem.md
- ../ContentSystems/LevelDataSystem.md

Depends On:
- None

Used By:
- Drop Away
- Color Block Jam
- Sky Rush
- Hole People
- Bus Jam

## Purpose

The Player Progress Data System owns player progression state in memory.

It represents what the player has achieved, unlocked, or recorded over time.

It does not own authored level content, runtime construction, or gameplay rule evaluation.

---

# Core Design Idea

The Player Progress Data System is the in-memory owner of progression state.

It should hold player-owned progress separately from authored level definitions and separately from runtime gameplay construction.

Conceptually:

```text
Game module determines progression outcome
    ->
Player Progress Data System updates in-memory progress state
    ->
Progress Save Load System may persist it later
```

The key boundary is:

* this system owns state
* another system persists that state

---

# Responsibilities

The Player Progress Data System is responsible for:

* owning player progression state in memory
* representing unlocked levels
* representing completed levels
* representing stars or equivalent completion ratings
* representing best scores
* representing best times
* representing attempt counts
* exposing player-owned progression state to other systems in a framework-safe way

These are representative examples, not a forced final model.

---

# Should Not Handle

The Player Progress Data System should not handle:

* authored level data
* level definitions
* runtime object construction
* gameplay rule evaluation
* win conditions
* lose conditions
* puzzle-specific validation
* persistence mechanics

Rule:

Owns progression state in memory.

It does not decide gameplay truth or persistence behavior.

---

# Example Progression State

Examples of player-owned progression state may include:

* whether a level is unlocked
* whether a level is completed
* earned stars or equivalent rating
* best score
* best completion time
* total attempts

These examples are useful because they appear across many puzzle games.

However, the framework should not force every game to use the exact same progression record shape on day one.

The reusable pattern matters more than one rigid schema.

---

# In-Memory Ownership Boundary

This system owns progression state in memory.

That means:

* it is the runtime source of truth for player progress while the game is running
* it may be loaded from persistence at startup
* it may be updated when game modules decide progress changed

It should not become a hidden content database or a level-definition substitute.

Progression state is about player history, not authored structure.

---

# Relationship With Progress Save Load

The Player Progress Data System owns state.

The Progress Save Load System persists that state.

Approved split:

* Player Progress Data holds in-memory progress
* Progress Save Load saves and restores that progress

This keeps state modeling and persistence separated cleanly.

---

# Relationship With Content Systems

Content Systems own authored level content.

The Player Progress Data System does not.

Boundary:

* content says what levels exist
* progression says what the player has done with those levels

This distinction is important because authored data and player state evolve for different reasons and on different lifecycles.

---

# Framework vs Game Module Ownership

Framework ownership:

* generic progression-state patterns
* separation between player-owned data and authored level data
* reusable ways to represent progress at a framework level

Game module ownership:

* puzzle-specific progression meaning
* reward interpretation
* additional progression metrics where needed

Framework owns the general progression boundary.

Game modules decide what counts as meaningful progress in puzzle terms.

---

# Data Flow

A typical flow may look like:

```text
Gameplay outcome occurs
    ->
Game module decides progress consequence
    ->
Player Progress Data System updates in-memory state
```

Another common flow:

```text
Saved progress loaded
    ->
Player Progress Data System receives restored progress state
```

The Player Progress Data System should sit in the middle of progression state ownership, not at the start of gameplay rule evaluation.

---

# Example Usage

## Drop Away

The system may track:

* whether a level is unlocked
* whether a level is completed
* best completion rating or attempts

Drop Away game logic still decides when a level completion actually happened.

## Color Block Jam

The system may track:

* completed levels
* best score or move record equivalents if needed later

The game module still decides how completion is interpreted.

## Sky Rush, Hole People, And Bus Jam

The system may track:

* unlock progression
* completion history
* best performance metrics

Each game module still owns the meaning of those metrics.

---

# Edge Cases

## Too-Rigid Progress Model

If the framework hardcodes one progression schema too early, later games may need awkward optional fields or workarounds.

The system should stay generic enough to support shared progression patterns without forcing every game into one exact model.

## Drift Into Gameplay Rules

If the system starts deciding whether a level should count as complete, progression ownership has drifted into rule ownership.

That should remain outside this system.

## Drift Into Content Ownership

If the system starts storing authored level structure or level metadata as if it owns the level itself, content boundaries have broken.

The system should reference progress against content, not absorb content ownership.

---

# MVP Scope

## Approved First Implementation (2026-09-02)

The first slice owns a set of completed stable level IDs and a resume level ID. It does not store
catalog indexes, authored level contents, unlock rules, loop ranges, replay mode, or board snapshots.
Expose completion queries, idempotent completion recording, resume selection, snapshot capture,
and validated restoration. Snapshots contain `FormatVersion`, `CompletedLevelIds`, `ResumeLevelId`.
IDs are opaque, ordinal and case-sensitive; reject blank IDs and duplicate snapshot IDs. An empty
resume string means no selection yet. Snapshot arrays must not alias the in-memory completion set.
The schema starts at version 1. Reject missing required fields and unsupported versions; no migration
or speculative statistics/reward fields in this slice. Games decide when and how state changes.

The first version of the Player Progress Data System should support:

* in-memory ownership of player progress
* representative progression fields such as unlocks, completion, and best-record-style values
* clean separation from authored level content
* clean separation from persistence mechanics

It should stay generic enough to support the target games without overcommitting to one progression schema too early.

---

# Future Extensions

The following can be added later:

* multiple profiles
* richer stats
* progression versioning support
* analytics-facing mirrors
* achievement or reward integration

These should remain subordinate to the core rule:

The Player Progress Data System owns progression state in memory.

---

# Final Design Rule

The Player Progress Data System owns player-owned progression state in memory.

It tracks what the player has done.

It does not own authored level data.

It does not own runtime construction.

It does not own gameplay rule decisions.
