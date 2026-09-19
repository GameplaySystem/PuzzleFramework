# Puzzle Framework

A reusable, game-agnostic Unity package for building grid-based puzzle games. The project extracts
proven systems from playable prototypes instead of attempting to design a universal engine in the
abstract. Its goals are reusable architecture, testable gameplay foundations, clear ownership
boundaries, and portfolio-quality engineering documentation.

The first consumer is [Drop The Man](https://github.com/GameplaySystem/DropTheMan), a reconstruction of
the core interaction loop from *Drop Away*. Its current MVP development milestone is complete;
final presentation/UI integration is deferred. Color Block Jam is now the active second prototype
in architecture/design preflight, to validate which ideas are reusable before further extraction.

## Design Principles

- Framework code is independent of game-specific concepts such as holes, cats, buses, or doors.
- Game modules translate generic capabilities into puzzle-specific rules and presentation.
- Dependencies flow from a game module to the framework, never in the opposite direction.
- Authored game payloads remain opaque to framework content and persistence services.
- Systems are documented before implementation and generalized only after demonstrated reuse.
- Pure C# state and policies are preferred where Unity scene dependencies are unnecessary.

## Implemented Foundation

The current package includes focused foundations for:

- grid coordinates, cells, boards, occupancy, shapes, and wall generation
- pointer input contracts, destination-based drag validation, and grid snapping
- authored level data, JSON save/load, Resources catalog discovery, and metadata validation
- runtime construction validation and level runtime contexts
- game-state transitions and countdown timers
- shared color identity and modular board-visual planning/building
- versioned player progress data and resilient local JSON persistence

Some documented systems remain intentionally unimplemented. The authoritative status is maintained
in [PROJECT_STATE.md](docs/PROJECT_STATE.md), and known risks are tracked in
[IMPLEMENTATION_WATCHLIST.md](docs/IMPLEMENTATION_WATCHLIST.md).

Continuous swept movement currently lives in Drop The Man's game module. Its suitability for a
shared geometry extension is part of the second-prototype audit.

## Repository Layout

```text
Packages/com.gaming.puzzleframework/   Installable Unity package
  Runtime/                             Framework runtime assemblies
  Tests/EditMode/                      Package Edit Mode tests
  README.md                            Package-level summary
docs/                                  Architecture, system specs, roadmap, and reports
PuzzleFramework/                       Unity validation project assets/settings
AGENTS.md                              Repository engineering and contribution rules
```

## Requirements

- Unity `6000.3` or later in the Unity 6.3 line
- Git when consuming the package directly from this repository

The framework itself does not require a game-specific render pipeline or presentation package.
Consumers own their visual stack and any third-party dependencies.

## Installation

Use Unity Package Manager with the package subfolder and an immutable commit SHA:

```text
https://github.com/GameplaySystem/PuzzleFramework.git?path=/Packages/com.gaming.puzzleframework#<full-commit-sha>
```

For reproducible builds, pin a verified 40-character commit rather than `main`. The complete
cross-repository update process is documented in
[FrameworkPackageDependencyWorkflow.md](docs/Workflow/FrameworkPackageDependencyWorkflow.md).

## Validation

Package tests live under `Packages/com.gaming.puzzleframework/Tests/EditMode`. The nested Unity
project does not install or enable the sibling package's tests by default. Follow the
[framework validation setup](docs/FrameworkValidationSetup.md), then run them from
**Window > General > Test Runner > EditMode**.

The most recent progression slice passed its 18 focused framework tests, while its Drop The Man
consumer passed 27 Edit Mode tests. A pre-existing catalog-test assertion compatibility issue is
recorded in the watchlist and is not hidden by those focused results.

## Documentation

- [Framework architecture](docs/FrameworkArchitecture.md)
- [System documentation](docs/FrameworkSystems)
- [Implementation roadmap](docs/ImplementationRoadmap/FrameworkMVPPlan.md)
- [Critical gameplay clarifications](docs/CRITICAL_RULE_CLARIFICATIONS.md)
- [Drop The Man remaining work](docs/DropTheManRemainingWork.md)
- [Color Block Jam architecture preflight](docs/ColorBlockJamArchitecturePreflight.md)

## Current Scope

This is an actively developed learning and portfolio project, not a production-ready Unity SDK.
Near-term work is Color Block Jam requirements and architecture, with no gameplay implementation
started. Drop The Man remains a maintenance/regression target; return only for critical bugs,
framework regressions, or explicitly resumed presentation integration when final assets arrive.
Editor extraction, generalized feedback infrastructure, and other broad systems remain deferred
until cross-game evidence justifies them.
