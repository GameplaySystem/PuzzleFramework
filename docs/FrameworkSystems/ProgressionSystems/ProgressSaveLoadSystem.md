# Progress Save Load System

## Document Metadata

Category:
- Progression Systems

Status:
- Approved

Parent:
- Overview.md

Related Documents:
- Overview.md
- PlayerProgressDataSystem.md
- ../ContentSystems/LevelSaveLoadSystem.md

Depends On:
- Player Progress Data System

Used By:
- Drop Away
- Color Block Jam
- Sky Rush
- Hole People
- Bus Jam

## Purpose

The Progress Save Load System persists player-owned progression state.

It saves player progress and loads it later.

It does not own authored level save/load, runtime construction, or gameplay rules.

---

# Core Design Idea

The Progress Save Load System is the persistence owner for player progress.

It should save and restore progression state without absorbing authored level content responsibilities.

Conceptually:

```text
Player Progress Data System owns in-memory progress
    ->
Progress Save Load System persists it
    ->
Saved progress is restored later
```

The important boundary is:

* player progress data is the subject
* persistence is the responsibility

---

# Responsibilities

The Progress Save Load System is responsible for:

* saving player progress
* loading player progress
* persistence of progression data
* returning success or failure results where appropriate
* keeping player-owned state separate from authored content persistence

---

# Should Not Handle

The Progress Save Load System should not handle:

* authored level save/load
* level definitions
* runtime level construction
* gameplay rule evaluation
* win conditions
* lose conditions
* puzzle-specific progression meaning

Rule:

Persists player progress only.

It does not own authored content persistence.

---

# Progression Persistence Boundary

This system persists player-owned state.

That includes data such as:

* unlock progress
* completion history
* best-record-style metrics
* attempts or similar player-owned counters

It should not be treated as a second level content persistence system.

The framework already has a separate authored content persistence boundary.

Keeping those split is necessary because:

* authored level files are content
* progression saves are player state

Those are different concerns with different lifecycles.

---

# Relationship With Player Progress Data System

The Player Progress Data System owns in-memory progression state.

The Progress Save Load System persists that state.

Approved split:

* progress data owns state in memory
* progress save/load saves and restores it

This keeps storage mechanics separate from state ownership.

---

# Relationship With Content Systems

The `LevelSaveLoadSystem` persists authored level definitions.

The `ProgressSaveLoadSystem` persists player progress.

Boundary:

* Content save/load answers: what is the level
* Progress save/load answers: what has the player done

These should remain separate even if they later use similar persistence techniques.

The persistence format can evolve independently without collapsing the ownership boundary.

---

# Framework vs Game Module Ownership

Framework ownership:

* progression persistence patterns
* separation between player-state persistence and content persistence
* framework-safe result reporting around progress save and load

Game module ownership:

* puzzle-specific interpretation of loaded progress
* puzzle-specific reward meaning
* puzzle-specific extensions to what progress needs to store

Framework owns the persistence boundary.

Game modules own meaning.

---

# Data Flow

A typical save flow may look like:

```text
Game module determines progress changed
    ->
Player Progress Data System updates state
    ->
Progress Save Load System saves state
```

A typical load flow may look like:

```text
Persisted progress data
    ->
Progress Save Load System loads state
    ->
Player Progress Data System receives restored progress
```

The important rule is:

* this system persists progress
* it does not determine whether the progress should exist

---

# Example Usage

## Drop Away

The system may persist:

* unlocked levels
* completed levels
* attempt history

Drop Away-specific logic still decides when those values should change.

## Color Block Jam

The system may persist:

* best score
* completion state
* unlock progression

The game module still decides how those values are earned.

## Sky Rush, Hole People, And Bus Jam

The system may persist:

* player unlock progression
* performance records
* completion history

The framework still does not decide puzzle-specific reward meaning.

---

# Edge Cases

## Save Drift Into Content Persistence

If this system starts saving authored level structures, the content/progression boundary has failed.

That belongs to Content Systems, not here.

## Load Drift Into Runtime Construction

If this system starts spawning runtime objects or initializing gameplay state directly, runtime construction boundaries have failed.

That belongs to Runtime Construction Systems.

## Too-Specific Persistence Assumptions

The framework should not assume one exact progression format forever.

The reusable idea is persistence ownership, not one prematurely rigid storage model.

---

# MVP Scope

The first version of the Progress Save Load System should support:

* saving player-owned progression state
* loading player-owned progression state
* keeping progression persistence separate from authored level persistence
* reporting success or failure cleanly

It should stay scoped to player progress only.

---

# Future Extensions

The following can be added later:

* multiple save slots or profiles
* migration support
* cloud sync support
* encryption or tamper-resistance if ever needed
* backup and recovery helpers

These should remain subordinate to the core rule:

The Progress Save Load System persists player progress only.

---

# Final Design Rule

The Progress Save Load System saves and loads player-owned progression state.

It persists what the player has done.

It does not persist authored level definitions.

It does not build runtime levels.

It does not own gameplay rule meaning.
