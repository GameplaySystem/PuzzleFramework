# Runtime Construction Validation System

## Document Metadata

Category:
- Runtime Construction Systems

Status:
- Approved

Parent:
- Overview.md

Related Documents:
- Overview.md
- LevelRuntimeBuilderSystem.md
- RuntimeObjectFactorySystem.md
- ../ContentSystems/LevelDataSystem.md
- ../ContentSystems/LevelSaveLoadSystem.md
- ../CoreBoardSystems/GridSystem.md
- ../CoreBoardSystems/ShapeSystem.md
- ../CoreBoardSystems/CellOccupancySystem.md

Depends On:
- Level Data System
- Grid System
- Shape System
- Cell Occupancy System

Used By:
- Drop Away
- Color Block Jam
- Sky Rush
- Hole People
- Bus Jam

## Purpose

The Runtime Construction Validation System validates whether loaded authored level data can be safely built into runtime state.

Its role is narrow and important:

* protect the runtime construction flow
* catch build-safety problems early
* reject invalid construction input before object creation begins

It is not a puzzle-quality or puzzle-rules system.

---

# Core Design Idea

Validation happens before construction.

The Runtime Construction Validation System inspects the loaded level definition and answers whether the runtime build can safely proceed.

Conceptually:

```text
Loaded LevelDefinition
    ->
Runtime Construction Validation
    ->
Build-safe or build-failed result
    ->
Builder proceeds or stops
```

The important boundary is:

* validation checks buildability
* builder coordinates construction
* gameplay systems judge puzzle meaning later

---

# Responsibilities

The Runtime Construction Validation System is responsible for:

* build-safety validation
* placement validity checks
* shape fit checks
* overlap checks
* required construction data checks
* rejecting levels that cannot be safely constructed

Examples of questions it may answer:

* does required board data exist
* are placements inside valid board bounds
* do shared shapes fit within the board
* do placements overlap in impossible ways
* is required construction data missing

---

# Should Not Handle

The Runtime Construction Validation System should not handle:

* puzzle solvability
* matching logic
* collection logic
* queue behavior
* win conditions
* lose conditions
* player progression
* save or load ownership
* presentation ownership

Rule:

Validates whether a level can be built, not whether it is a good puzzle.

---

# Validation Boundary

This system validates construction readiness, not puzzle quality.

That means it may reject:

* impossible placements
* missing required construction inputs
* shape overlap conflicts
* invalid board references

It should not reject a level because:

* the puzzle is too easy
* the puzzle is too hard
* the puzzle has a bad strategy curve
* a color match rule feels unfair
* a puzzle-specific objective is weak

Those concerns belong elsewhere.

If construction validation absorbs puzzle judgment, the boundary becomes unclear and the framework starts owning game-design decisions it should not own.

---

# Pre-Build Validation Rule

Validation should happen before construction.

Approved direction:

* pre-build validation is required
* post-build validation may exist later only as debug support

This reduces partial construction failure and keeps startup flow easier to reason about.

The default expectation is:

```text
Load level data
    ->
Validate build-safety
    ->
Construct runtime level
```

not:

```text
Start construction
    ->
discover invalid data halfway through
    ->
leave partial runtime state behind
```

---

# Relationship With Core Board Systems

Construction validation may rely on shared board-level framework data such as:

* board bounds
* cell validity
* shape footprint rules
* occupancy-related overlap checks

That does not make validation the owner of those systems.

It consumes board-structure information to answer construction-safety questions.

This is one of the main reasons Runtime Construction Validation belongs near Core Board Systems in dependency terms, while still remaining a distinct category.

---

# Relationship With The Builder

The builder coordinates around validation results.

The validator should not become the builder.

Approved split:

* validator answers whether runtime construction may proceed
* builder decides what happens next

If validation starts coordinating initialization order or object creation, the category boundary has drifted.

---

# Relationship With Content Systems

Content Systems define and load authored level data.

Validation inspects that loaded data for runtime build safety.

Boundary:

* Content Systems own authored schema and persistence
* Runtime Construction Validation owns build-readiness checks on loaded data

This keeps content definition separate from runtime startup safety.

---

# Framework vs Game Module Ownership

Framework ownership:

* shared construction-safety validation patterns
* board-level construction checks
* generic runtime build-readiness rules

Game module ownership:

* puzzle-specific validation extensions
* puzzle-specific construction constraints
* puzzle-specific meaning of why a configuration matters

The framework must remain unaware of:

* Hole
* Stickman
* Bus
* Door
* Brick

The framework can validate generic structural readiness.

Game modules validate puzzle-specific meaning where needed.

---

# Example Usage

## Shared Board Placement Validation

The validator may confirm:

* placed content sits inside the board
* blocked or inactive cell constraints are respected
* generic shape placement does not overlap invalidly

## Game-Module Extension Validation

A game module may add puzzle-specific validation layers after shared construction validation.

That extension point is appropriate.

What is not appropriate is pushing those puzzle-specific checks back into framework-owned validation.

## Builder Gatekeeping

The builder may use validation as a gate:

```text
Validate
    ->
Success: continue build
Failure: stop build and report
```

That is the correct use of this system.

---

# Edge Cases

## Overlap With Puzzle Validation

Some checks can appear ambiguous.

For example:

* a placement may be structurally valid but puzzle-mechanically invalid

Rule:

If the level can still be built safely, it does not automatically belong to Runtime Construction Validation.

## Post-Build Debug Checks

Post-build validation can be useful later for diagnostics.

It should remain secondary and debug-oriented rather than replacing pre-build validation.

## Validation Expands Too Far

A common failure mode is letting validation absorb every kind of correctness check because it already has access to level data.

That must be resisted.

Construction validation is about build safety.

Not puzzle design quality.

---

# MVP Scope

The first version of the Runtime Construction Validation System should support:

* required construction data checks
* board-bounds checks
* shape fit checks
* overlap checks
* basic placement validity checks
* a build-safe or build-failed result for the builder

It should stay focused on buildability.

---

# Future Extensions

The following can be added later:

* richer validation reports
* debug-only post-build validation
* warnings versus errors
* module extension hooks for puzzle-specific validation
* editor integration with construction-safety reports

These should remain subordinate to the core rule:

The validator answers whether the level can be built safely.

---

# Final Design Rule

The Runtime Construction Validation System validates build safety before runtime construction begins.

It checks whether the level can be built.

It does not decide whether the level is a good puzzle.

It does not own gameplay rules.
