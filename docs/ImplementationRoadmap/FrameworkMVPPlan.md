# Framework MVP Plan

## Purpose

This document defines the smallest implementation slice required to produce a playable Drop Away prototype on top of the approved framework architecture.

This is implementation planning only.

It does not redesign the architecture.

The goal is to identify the minimum framework work needed to make one real game playable.

The implementation structure is:

* reusable `PuzzleFramework` package or package-ready source
* separate `DropAwayPrototype` Unity project consuming that framework locally

The MVP is not a plan to build Drop Away inside one giant framework Unity project.

---

# MVP Definition

```text
Framework MVP
=
Minimum framework implementation required
to support Drop Away.
```

The target is:

```text
Playable Drop Away
```

not:

```text
Perfect framework
```

Decision rule:

Every planned system must answer:

```text
Does Drop Away require this to function?
```

If the answer is:

```text
No
```

defer it.

The same narrowing rule applies to project structure:

* only create as much framework packaging structure as Drop Away needs to consume the framework cleanly
* keep prototype-specific scenes, assets, rules, and nouns outside the framework

---

# Drop Away Requirements

Drop Away MVP must support the following gameplay requirements:

* board creation
* active cells
* blocked cells where needed
* hole placement
* stickman placement
* color assignment
* level loading
* runtime object construction from loaded level data
* dragging holes
* grid snapping
* occupancy validation
* matching collection
* hole capacity or collection limit behavior if required by the ruleset
* timer-driven lose condition
* win condition
* lose condition
* basic gameplay feedback that keeps the state readable
* separate prototype project consuming the framework locally

These are gameplay requirements, not implementation commitments.

Anything beyond these should be treated as suspect unless Drop Away truly needs it for playability.

---

# Required Framework Systems

The framework MVP should only implement the systems Drop Away needs now.

Everything else should be deferred until a later prototype proves the need.

Implementation structure rule:

```text
1. Prepare PuzzleFramework as reusable package/package-ready source.
2. Create or use separate DropAwayPrototype Unity project.
3. Reference PuzzleFramework locally from DropAwayPrototype.
4. Implement framework MVP only as Drop Away requires it.
5. Implement Drop Away game module inside DropAwayPrototype.
6. Keep prototype-specific scenes, assets, rules, and nouns outside the framework.
```

## Framework vs DropAwayPrototype Boundary

`PuzzleFramework` may contain:

* reusable systems
* framework contracts
* framework runtime infrastructure
* generic test or demo utilities only if clearly framework-safe

`DropAwayPrototype` owns:

* Drop Away scenes
* Drop Away assets
* Drop Away rules
* Drop Away game module
* holes
* stickmen
* Drop Away-specific level content
* build settings
* app metadata

Forbidden dependency:

```text
PuzzleFramework
    ->
DropAwayPrototype
```

Allowed dependency:

```text
DropAwayPrototype
    ->
PuzzleFramework
```

## Content Systems

### Required Now

`Level Data System`

Reason:
Required to define a loadable Drop Away level structure.

`Level Save Load System`

Reason:
Required to load authored level definitions into the runtime slice.

`Level Catalog System` [implemented follow-up]

Reason:
Removes manually maintained level-reference arrays while keeping shipped-content discovery,
ordering, and duplicate validation reusable across puzzle game modules.

### Deferred

`Level Editor Foundation`

Reason:
Not required to make Drop Away playable.

Hand-authored or simple test-authored level data is enough for the MVP.

## Runtime Construction Systems

### Required Now

`Runtime Construction Validation System`

Reason:
Required to reject structurally invalid level data before building runtime state.

`Level Runtime Builder System`

Reason:
Required to coordinate board setup, runtime object creation, and runtime handoff.

`Runtime Object Factory System`

Reason:
Required to create runtime objects cleanly without collapsing construction flow into game-module scripts.

### Deferred

Advanced runtime diagnostics and richer runtime context abstractions.

Reason:
Not required for a playable Drop Away slice.

## Core Board Systems

### Required Now

`Grid System`

Reason:
Required for board structure.

`Cell Occupancy System`

Reason:
Required for placement checks and movement validity.

For the first `Drop The Man` slice, collectible targets may still be tracked separately from structural occupancy when puzzle rules require a cell to stay structurally enterable.

`Shape System`

Reason:
Required because the first playable `Drop The Man` slice includes multi-cell holes with footprint-driven behavior.

`Wall Generation System`

Reason:
Not required for playability.

Post-baseline decision:

The reusable Wall Generation System is approved as the structural dependency for the modular
board-visual replacement phase. This does not change the fact that it was deferred from the
minimum playable baseline.

Implementation status:

The post-baseline topology foundation is implemented from an explicit participation mask.

`Pathfinding System`

Reason:
Drop Away does not require pathfinding to function.

## Interaction Systems

### Required Now

`Input System`

Reason:
Required to detect player interaction.

`Drag Movement System`

Reason:
Required to move holes during drag.

`Grid Snap System`

Reason:
Required to resolve final board-aligned placement.

### Deferred

Advanced drag variants, editor-specific interaction modes, and broader interaction extensibility.

Reason:
Not required for playable Drop Away.

## Resource Processing Systems

### Deferred

`Capacity System`

Reason:
Prototype-level hole capacity behavior is required for the first playable `Drop The Man` slice.

However, reusable framework `CapacitySystem` generalization is still deferred because current capacity meaning is tightly coupled to prototype hole shapes and close or disappear behavior.

`Queue System`

Reason:
Drop Away MVP does not require ordered waiting.

`Buffer System`

Reason:
Drop Away MVP does not require temporary slot-based storage as a framework concern.

## Runtime Flow Systems

### Required Now

`Game State System`

Reason:
Required for clean win or lose flow.

`Timer System`

Reason:
Required if the MVP uses a timer-driven lose condition.

### Deferred

`Event System` as a broad reusable notification layer.

Reason:
A fully generalized notification layer is not required for Drop Away to function.

Direct calls are acceptable in the MVP wherever one clear owner exists.

Events should only be introduced when real multi-listener notification needs appear.

MVP rule:

* If event-based communication is introduced during MVP, it must use the approved framework `EventSystem`.
* Do not create temporary event architectures.
* Do not create prototype-specific notification frameworks.
* Do not create parallel event implementations that will later be replaced.

## Presentation Systems

### Required Now

`Color System`

Reason:
Required so matching rules are visible to the player.

Minimal feedback support for:

* selection
* invalid move readability
* win or lose readability

Reason:
The prototype must be understandable while being played.

### Deferred

Broader `Visual Feedback System` generalization.

Reason:
Drop Away only needs enough presentation to remain readable and usable.

Do not build a large feedback framework during MVP.

`Modular Board Visual System`

Reason:

Not required for the minimum playable baseline. It is approved as a focused post-baseline system
for converting reusable Wall Generation results into modular cell-prefab visual state. It must
remain separate from gameplay runtime object construction and puzzle-specific obstacle meaning.

Implementation status:

The initial half-wall, convex-cap, concave-elbow, passive cell-view, and narrow builder profile is
implemented. Concrete prefab wiring and visual validation remain game-module work.

## Progression Systems

### Required Now

None.

Reason:
Drop Away MVP does not require player-owned progression state to become playable.

### Deferred

`Player Progress Data System`

Reason:
Not required for a playable first slice.

`Progress Save Load System`

Reason:
Not required for a playable first slice.

---

# Recommended Implementation Order

The implementation sequence should favor dependencies and fast proof of playability.

## 1. Framework Package Foundation

Implement:

* package or package-ready framework source structure
* local package consumption path expected by prototype projects
* separation between framework-owned source and prototype-owned source

Why first:

The MVP must prove the intended portfolio structure, not just the gameplay slice.

The framework should be consumable by a separate Unity project before game-specific implementation begins.

## 2. DropAwayPrototype Project Foundation

Implement:

* a separate `DropAwayPrototype` Unity project
* local reference from `DropAwayPrototype` to `PuzzleFramework`
* initial project shell for prototype-owned scenes, assets, and game-module code

Why second:

The first playable slice should run in the same physical structure the portfolio intends to keep.

## 3. Content Foundation

Implement:

* minimal `LevelDefinition` support for Drop Away
* basic level loading through `Level Save Load System`

Why third:

The runtime slice needs real data to build from.

## 4. Core Board Foundation

Implement:

* grid structure
* active and blocked cell support
* occupancy truth

Why fourth:

Runtime construction and interaction both depend on a valid board model.

## 5. Runtime Construction Foundation

Implement:

* construction validation
* builder flow
* object factory path

Why fifth:

The prototype must be able to turn loaded level data into runtime state before gameplay can exist.

## 6. Interaction Foundation

Implement:

* input detection
* drag behavior
* snap resolution

Why sixth:

This is the first player-facing interaction layer needed to move holes around the board.

## 7. Drop Away Game Module Rules

Implement:

* drag-time matching collection
* wrong-color entry blocking where needed
* shape-based capacity completion
* hole-specific gameplay logic
* win or lose evaluation

Why seventh:

Framework mechanics should exist before puzzle-specific meaning is layered on top.

All Drop Away-specific rules and nouns should live in `DropAwayPrototype`, not in `PuzzleFramework`.

## 8. Runtime Flow Foundation

Implement:

* game state transitions
* timer behavior
* end-state flow

Why eighth:

The slice needs clean start, active, won, and lost flow once core interaction and rules exist.

## 9. Minimal Presentation

Implement:

* color readability
* basic feedback for selection, invalid actions, and end states

Why ninth:

Playability comes before polish, but some readability is required for the slice to be understandable.

Keep broad presentation-system generalization deferred.

## 10. MVP Integration and Cleanup

Implement:

* end-to-end testing of the slice
* small fixes to framework seams
* documentation updates if implementation reveals design drift

Why tenth:

The last step is proving the slice works coherently, not expanding feature scope.

---

# Drop Away Vertical Slice

The first playable milestone should be:

```text
Load level
    ->
Drag hole
    ->
Collect matching stickmen during drag
    ->
Release and snap hole for alignment
    ->
Win or Lose
```

Nothing more.

That milestone is enough to prove:

* authored data can load
* a separate prototype project can consume the framework
* runtime state can be built
* interaction works
* puzzle-specific rules can sit on top of framework systems
* runtime flow reaches a complete outcome

---

# Deferred Features

The following should not be implemented during the Framework MVP unless Drop Away unexpectedly proves they are required:

* cloud save
* multiple profiles
* analytics
* theme packs
* accessibility variants beyond basic playability
* generic plugin architecture
* asset store packaging
* advanced editor tooling
* optimization passes
* pathfinding
* queue system
* buffer system
* progression systems
* full visual feedback framework
* advanced event infrastructure
* broad shape generalization beyond what the slice actually needs
* wide runtime construction abstraction beyond the minimum build flow
* merging framework and prototype into one Unity project

The MVP should prove architecture, not complete the portfolio.

---

# Success Criteria

Framework MVP is complete when all of the following are true:

* a Drop Away level can be loaded
* `DropAwayPrototype` runs as a separate Unity project
* `DropAwayPrototype` references `PuzzleFramework` locally
* runtime state can be built from authored level data
* a hole can be dragged
* same-color targets can be collected during drag
* occupancy validation prevents invalid placement
* wrong-color targets block drag-time entry
* a hole can be snapped to valid board positions on release
* timer-driven lose flow works if the level uses a timer
* win flow works
* lose flow works
* the slice is playable from start to finish
* framework boundaries remain intact
* `PuzzleFramework` contains no Drop Away-specific nouns or scenes
* Drop Away-specific code lives in `DropAwayPrototype`
* a reviewer can inspect framework and prototype separately
* framework remains reusable by a second prototype without requiring architectural redesign
* puzzle-specific nouns and rules remain inside the Drop Away game module layer

---

# Final Planning Rule

Framework MVP implementation should target the smallest credible slice that makes Drop Away playable.

If a system is not required for playable Drop Away, defer it.

The first implementation win is:

* one playable game
* on top of real framework structure

not:

* maximum framework completeness
