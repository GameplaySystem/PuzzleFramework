# Level Runtime Builder System

## Document Metadata

Category:
- Runtime Construction Systems

Status:
- Approved

Parent:
- Overview.md

Related Documents:
- Overview.md
- RuntimeObjectFactorySystem.md
- RuntimeConstructionValidationSystem.md
- ../ContentSystems/LevelDataSystem.md
- ../ContentSystems/LevelSaveLoadSystem.md

Depends On:
- Runtime Construction Validation System
- Runtime Object Factory System
- Level Data System

Used By:
- Drop Away
- Color Block Jam
- Sky Rush
- Hole People
- Bus Jam

## Purpose

The Level Runtime Builder System coordinates conversion of loaded authored level data into a playable runtime level.

It owns runtime construction flow.

It does not own low-level object creation details, content persistence, or gameplay rule meaning.

---

# Core Design Idea

The builder is the coordinator.

It receives authored level data that has already been loaded.

It ensures validation runs at the correct time, board systems are initialized in the correct order, factories are called at the correct time, and runtime state is handed off cleanly.

Conceptually:

```text
Loaded LevelDefinition
    ->
Builder coordinates validation
    ->
Builder initializes shared runtime systems
    ->
Builder calls factories
    ->
Builder produces runtime level context
    ->
Gameplay begins
```

The important boundary is:

* Builder coordinates
* Factory creates
* Game modules interpret gameplay meaning later

---

# Responsibilities

The Level Runtime Builder System is responsible for:

* coordinating construction flow
* defining build order
* ensuring validation happens before build
* initializing shared board-level runtime systems
* coordinating one or more runtime object factories
* registering runtime state or runtime references
* producing a runtime level context or equivalent handoff structure

---

# Should Not Handle

The Level Runtime Builder System should not handle:

* low-level object creation details
* save or load operations
* player progression
* puzzle-specific rule evaluation
* win conditions
* lose conditions
* presentation ownership
* audio or UI ownership

The builder should also not become a hidden service locator or a god object that absorbs every startup responsibility.

---

# Builder Ownership Boundary

The builder decides when construction steps happen.

It should not decide how every object is individually created.

That separation matters because the builder owns orchestration, not per-object creation mechanics.

A healthy split looks like:

* builder defines construction sequence
* factories create specific runtime objects
* gameplay systems use the finished runtime context afterward

If the builder starts creating every object directly, factory boundaries become meaningless.

If the builder starts judging puzzle outcomes, runtime construction and game rules become coupled.

---

# Typical Construction Flow

The approved typical flow is:

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

In practice, the builder owns the sequence and handoff discipline.

It makes sure construction happens in a predictable order rather than leaving runtime setup to scattered scripts.

---

# Runtime Context Concept

A `RuntimeLevelContext` or equivalent concept is reasonable for the builder to produce.

It may later contain:

* board systems
* occupancy state
* runtime object references
* shared runtime lookup structures
* runtime-level shared state needed by gameplay systems

This is useful because a fully constructed level usually needs to hand off more than a single object.

However, this document does not finalize the implementation shape.

The approved design is the ownership idea:

* builder produces coordinated runtime handoff state

not:

* one final concrete class definition

---

# Relationship With Validation

Validation happens before construction.

The builder should treat validation as a prerequisite, not as an optional late-stage convenience.

That means:

* do not partially build and hope validation catches problems later
* do not let construction continue after known build-safety failure

The builder should coordinate around validation results.

It should not absorb validation ownership itself.

---

# Relationship With Runtime Object Factory

The builder coordinates factories.

It does not replace them.

The approved split is:

* builder decides when factories are called
* factories create and initialize runtime objects
* builder gathers results into runtime context

This allows creation mechanics to evolve without turning the build coordinator into the only object-creation entry point.

---

# Relationship With Content Systems

Content Systems define and load authored level data.

The builder consumes that data after load.

Boundary:

* `LevelDataSystem` defines the authored model
* `LevelSaveLoadSystem` restores that model from persistence
* `LevelRuntimeBuilderSystem` turns the loaded model into runtime state

The builder must not redefine content schema or persistence behavior.

---

# Framework vs Game Module Ownership

Framework ownership:

* runtime build sequencing
* shared initialization flow
* framework-safe coordination contracts
* shared runtime handoff pattern

Game module ownership:

* puzzle-specific build extensions
* puzzle-specific adapters
* puzzle-specific runtime object meaning
* puzzle-specific post-build rule hookup

Framework owns construction coordination.

Game modules own puzzle-specific meaning layered onto constructed runtime objects.

---

# Example Usage

## Drop Away

The builder may coordinate:

* board initialization
* shared occupancy setup
* creation of game-module-provided runtime objects
* registration of runtime references for later gameplay systems

The Drop Away module still decides gameplay meaning such as collection rules or win conditions.

## Sky Rush

The builder may coordinate:

* shared board setup
* shared runtime timer setup where needed
* factory-driven creation of puzzle-specific runtime entities

The Sky Rush module still decides queue logic, routing meaning, and completion rules.

## Hole People And Bus Jam

The builder may coordinate:

* pathfinding-ready board systems
* occupancy-ready runtime state
* factory calls for puzzle-specific object creation

The game modules still decide movement interpretation, collection, boarding, and puzzle outcomes.

---

# Edge Cases

## Partial Build Failure

If construction fails partway through, the builder should not silently leave half-built runtime state as the normal success path.

This is one reason validation should happen first.

## Builder Absorbs Too Much

A common failure mode is turning the builder into the owner of:

* validation
* object creation
* gameplay setup
* UI hookup
* progression hookup

That is too much.

The builder should remain a coordinator.

## Scattered Startup Logic

If unrelated scene scripts begin constructing level pieces independently, runtime ownership becomes unclear.

The builder exists to keep startup flow explicit.

---

# MVP Scope

The first version of the Level Runtime Builder System should support:

* consuming loaded level data
* coordinating pre-build validation
* initializing shared board runtime systems
* calling runtime object factories in a defined order
* registering runtime references
* producing a runtime handoff context for gameplay

---

# Future Extensions

The following can be added later:

* richer runtime context structures
* build diagnostics
* partial build rollback support
* debug-only build tracing
* adapter registries for game-module construction hooks
* async or staged construction if needed later

These extensions should remain subordinate to the core rule:

The builder coordinates runtime construction.

---

# Final Design Rule

The Level Runtime Builder System owns construction flow.

It validates first.

It initializes shared systems.

It coordinates factories.

It produces runtime handoff state.

It does not own low-level object creation or gameplay rule meaning.
