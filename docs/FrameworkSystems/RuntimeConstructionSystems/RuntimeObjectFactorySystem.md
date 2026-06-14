# Runtime Object Factory System

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
- RuntimeConstructionValidationSystem.md
- ../ContentSystems/LevelDataSystem.md

Depends On:
- Level Data System

Used By:
- Drop Away
- Color Block Jam
- Sky Rush
- Hole People
- Bus Jam

## Purpose

The Runtime Object Factory System creates runtime objects from construction-ready level data.

It owns object creation and object initialization.

It returns runtime references to the builder or equivalent construction coordinator.

---

# Core Design Idea

The factory is responsible for creation mechanics.

It should receive the construction-ready inputs it needs and return created runtime references.

Conceptually:

```text
Builder requests object creation
    ->
Factory creates runtime object
    ->
Factory initializes runtime object
    ->
Factory returns runtime reference
```

The important boundary is:

* builder decides when
* factory decides how to create

That split is the main reason this system should remain separate from the builder.

---

# Responsibilities

The Runtime Object Factory System is responsible for:

* creating runtime objects
* initializing runtime objects with construction-ready data
* returning runtime references
* hiding low-level creation details from the builder
* supporting reusable creation patterns where multiple games share them

---

# Should Not Handle

The Runtime Object Factory System should not handle:

* construction order
* pre-build validation policy
* save or load operations
* player progression
* gameplay rule decisions
* win conditions
* lose conditions
* authored level editing

The factory should also not become a hidden rule interpreter.

It creates runtime objects.

It does not decide what those objects mean in puzzle logic.

---

# Factory Ownership Boundary

The factory owns object creation details.

That may include:

* choosing the correct creation path
* applying construction-time initialization data
* returning the created runtime reference in a usable form

The factory should not own:

* the full runtime build flow
* whether the level is safe to build
* whether gameplay rules are satisfied

If the factory starts deciding build order, it turns into a builder.

If it starts rejecting levels for gameplay reasons, it turns into a rule system.

---

# Relationship With The Builder

The builder coordinates factories.

The factory creates on request.

Approved split:

* builder owns orchestration
* factory owns creation mechanics

This allows one builder to coordinate one or more factories without taking on per-object construction detail itself.

---

# Relationship With Validation

Validation should happen before factories are asked to create runtime objects.

That means factories should generally assume they are receiving construction-ready data.

Factories may still guard against obviously invalid creation inputs for robustness, but they should not become the primary owner of level build validation.

The main validation rule remains:

* validate first
* construct second

---

# Relationship With Content Systems

Content Systems define and persist authored data.

Factories do not own authored content.

Factories consume construction-ready data that has already passed through:

* content definition
* persistence load
* construction validation
* builder coordination

That keeps authored data concerns separate from runtime object creation mechanics.

---

# Framework vs Game Module Ownership

Framework ownership:

* shared factory patterns
* reusable creation contracts
* framework-safe initialization boundaries

Game module ownership:

* puzzle-specific runtime object types
* puzzle-specific creation adapters
* puzzle-specific initialization meaning

Framework may provide generic factory sockets or base patterns.

Game modules provide puzzle-specific object knowledge.

The framework must not know whether it is creating a hole, stickman, bus, door, or brick.

---

# Example Usage

## Shared Board-Level Creation

A framework-level factory may help create reusable board-related runtime structures such as:

* shared board runtime containers
* generic occupancy-related objects
* generic runtime lookup structures

## Game-Module Object Creation

A game module factory adapter may create puzzle-specific runtime objects using game-owned data.

The Runtime Object Factory System can still define the shared creation boundary without owning puzzle-specific nouns.

## Builder Coordination

The builder may request:

* board-related creation first
* puzzle-specific object creation second
* registration of returned runtime references afterward

The factory creates.

The builder sequences.

---

# Edge Cases

## Factory Drift Into Builder Logic

If the factory starts deciding creation order, dependency sequencing, or overall level startup flow, the architecture has drifted.

That belongs to the builder.

## Factory Drift Into Rule Logic

If the factory starts rejecting objects based on puzzle-specific rule meaning, validation and gameplay logic are leaking inward.

That does not belong here.

## Too Many Factory Layers Too Early

There is a real overengineering risk here.

The framework does not need a massive asset-store-grade factory architecture on day one.

It only needs enough reusable creation structure to support the target games cleanly.

---

# MVP Scope

The first version of the Runtime Object Factory System should support:

* object creation from construction-ready data
* object initialization at construction time
* returning runtime references to the builder
* shared creation patterns where genuine reuse exists

It should stay modest in scope.

---

# Future Extensions

The following can be added later:

* specialized factory registries
* pooled creation paths
* diagnostics around object creation failures
* stronger adapter patterns for puzzle-specific object creation
* editor-time runtime-preview creation if needed later

Those should remain subordinate to the core rule:

The factory creates runtime objects.

The builder decides when that happens.

---

# Final Design Rule

The Runtime Object Factory System owns object creation and object initialization.

It returns runtime references.

It does not own construction order.

It does not own validation policy.

It does not own gameplay rule meaning.
