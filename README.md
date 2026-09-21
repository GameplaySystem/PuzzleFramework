# Puzzle Framework

Puzzle Framework is a reusable Unity package for grid-based puzzle games. I build playable
prototypes first, then promote mechanics into the package only after a second game proves that the
same abstraction is useful. The repository contains the installable framework, its architectural
specifications, and a Unity validation project; it is not itself a standalone game.

> **Gameplay reel pending:** the portfolio capture will show the same board, movement, content,
> runtime-flow, and authoring foundations operating in
> [Drop The Man](https://github.com/GameplaySystem/DropTheMan) and
> [Color Block Escape](https://github.com/GameplaySystem/PuzzleFramework-ColorBlockEscape).
> No placeholder animation is presented as gameplay.

## Key Features

- coordinate-based boards, explicit cell states, shape footprints, occupancy, and derived walls
- pointer projection, continuous drag requests, grid snapping, swept-footprint traversal, and
  collision/structure clearance queries
- collision-safe atomic footprint transfers for multi-cell moving entities
- JSON level save/load, Resources catalog discovery, duplicate validation, and opaque game payloads
- runtime construction validation and reusable runtime level contexts
- game-state transitions, countdown timers, player-progress data, and resilient JSON persistence
- shared color identities plus modular board visual planning and prefab construction
- a live level-authoring core with structural consequence reporting, placement, selection, move,
  rotation, erase, cell/edge picking, staged restore, and extensible game-owned tools

## Architecture Overview

| Area | Implemented responsibility |
| --- | --- |
| Board / Grid | Coordinates, cells, board bounds, shape footprints, occupancy, world layout, and wall derivation. |
| Interaction | Pointer projection, target contracts, dragging, snapping, swept traversal, and clearance queries. |
| Level Data / Loading | Generic level definitions, JSON persistence, Resources catalogs, and opaque game payload transport. |
| Runtime Construction | Build-safety validation and construction of reusable board, occupancy, flow, and timer contexts. |
| Runtime Flow | Explicit game-state transitions and countdown timer results. A broad shared event bus is documented but is not shipped package code. |
| Presentation | Stable color identities and data-driven modular board visual plans/builders. Game animation remains game-owned. |
| Progression | Versioned player progress plus local JSON save/load; each game owns unlock and campaign policy. |
| Editor Tooling | A live authoring session, cell/edge picking, structural consequence detection, and a small tool host used by both prototypes. |
| Resource Processing | Queue, buffer, and capacity systems have approved designs but are not currently implemented in the package. |

## Architecture Diagram

```mermaid
flowchart LR
    DTM[Drop The Man] -->|game rules and adapters| PF
    CBE[Color Block Escape] -->|game rules and adapters| PF

    subgraph PF[Puzzle Framework package]
        Content[Level data, catalogs, save/load]
        Authoring[Live authoring core]
        Construction[Runtime construction]
        Board[Board, shapes, occupancy, walls]
        Interaction[Input contracts, drag, sweep, snap]
        Flow[Game state and timer]
        Presentation[Color and modular board plans]
        Progression[Progress data and persistence]

        Content --> Construction
        Authoring --> Content
        Construction --> Board
        Interaction --> Board
        Construction --> Flow
        Board --> Presentation
    end
```

The framework owns reusable data and mechanics. Each prototype composes those services, decides
what a move means, and supplies concrete visuals and outcomes. A detailed GitDiagram repository
map will be added during the media pass; this smaller diagram remains as the recruiter-facing
explanation rather than asking readers to decode a full dependency graph unaided.

## Framework vs. Game-Specific Code

| Puzzle Framework owns | Game modules own |
| --- | --- |
| coordinates, board structure, footprints, occupancy, wall facts | cats, holes, blocks, exits, and their gameplay meaning |
| generic drag/sweep/snap and clearance calculations | movement permission, collection, exit admission, and scoring rules |
| level envelope, schema persistence, catalogs, and construction contracts | opaque payload interpretation and game-specific construction |
| game-state and timer lifecycle | win/loss policy and exact outcome precedence |
| color identities and modular board planning | materials, animation, effects, prefabs, and UI |
| generic authoring session, picking, placement, move, rotation, and erase | palettes, entity tools, validation rules, and play-test policy |

Dependencies point from a game module to the framework. Framework assemblies do not reference
prototype assemblies or interpret prototype payloads.

## Reusable Systems Demonstrated

| Shared system | Drop The Man | Color Block Escape |
| --- | --- | --- |
| Board, footprints, occupancy | Tracks holes, cats, blocked cells, and capacity-bearing shapes. | Tracks irregular block footprints, structural cells, and progressive occupancy during exits. |
| Continuous movement | Moves a hole while applying DTM collection rules. | Moves fixed-orientation blocks with swept collision checks and no tunnelling. |
| Runtime flow | Coordinates level lifecycle, optional timer, progression, restart, and next level. | Resolves countdown loss and final-block acceptance, including win precedence at an exact timer tie. |
| Level data and construction | Loads catalogued JSON levels into cats, holes, board state, and prototype views. | Validates and constructs blocks, colored exits, timer data, and runtime occupancy. |
| Modular board presentation | Builds the configured DTM board surface and boundaries. | Reuses the same cell prefab and suppresses validated exit wall segments through a presentation-only override. |
| Authoring foundation | Uses center-anchored picking and preserves DTM's warned prune-on-resize policy. | Uses corner anchoring, rejects invalidating structural edits, and adds block/exit tools plus play-test handoff. |

## Level Creation / Editor Tooling

Both prototype editors run on the same live `LevelAuthoringCore`. The framework reports whether a
footprint fits, which authored content a structural change would affect, and which board cell or
boundary edge was picked. DTM and CBE retain their own tools, payloads, HUDs, and response policies.

```mermaid
flowchart LR
    Tool[Game-owned authoring tool] --> Session[LevelAuthoringCore]
    Session --> Definition[Generic level definition]
    Tool --> Payload[Opaque game payload]
    Definition --> JSON[JSON level file]
    Payload --> JSON
    JSON --> Validation[Game + framework validation]
    Validation --> Runtime[Runtime construction]
```

The tooling exists so new puzzle prototypes can define their entities and rules without rebuilding
board resize, picking, footprint placement, overlap checks, selection, movement, rotation, erase,
and save/load coordination.

## Technical Decisions

1. **Generalize after demonstrated reuse.** DTM shipped its own editor first. The shared authoring
   core was extracted only when CBE provided a second concrete use case.
2. **Keep authored data separate from runtime state.** JSON describes stable content; runtime
   construction creates occupancy, timers, and mutable gameplay sessions.
3. **Transport game payloads without interpreting them.** Framework persistence validates its
   schema while game modules validate cats, holes, blocks, exits, and game rules.
4. **Prefer composition and result objects.** Small services return explicit requests/results and
   are composed by prototype controllers instead of relying on a global service locator.
5. **Keep presentation downstream of logic.** Color and board visual plans consume game facts;
   animations cannot decide collection, exit success, or outcomes.

## Performance Considerations

- Core board, movement, validation, timer, and authoring policies are plain C# objects and do not
  create one Unity `Update` loop per logical entity.
- Swept-footprint queries operate on grid/occupancy data, allowing fast pointer motion to remain a
  deterministic board query rather than relying on Rigidbody collision callbacks.
- Modular board visuals are planned from derived topology and rebuilt through configured prefabs.
- No framework performance benchmark is published yet. Generic pooling and resource-processing
  implementations are not claimed as finished features.

## Project Structure

```text
Packages/com.gaming.puzzleframework/
  Runtime/
    Content/               Level data, catalogs, save/load, authoring core
    CoreBoard/             Coordinates, boards, footprints, occupancy, walls
    Interaction/           Pointer, drag, sweep, clearance, snap
    Presentation/          Color and modular board visual planning
    Progression/           Progress data and local persistence
    RuntimeConstruction/   Validation and runtime contexts
    RuntimeFlow/           Game state and timer
  Tests/EditMode/          Framework tests
docs/                      Approved designs, project state, and implementation notes
PuzzleFramework/           Unity validation project
```

## Technologies

- Unity `6000.3.17f1`
- C# and Unity assembly definitions
- Unity Test Framework `1.6.0`
- Unity Input System contracts in consuming projects
- Universal Render Pipeline in the validation project and current prototypes
- Git-based Unity Package Manager distribution pinned to immutable commit SHAs

Pathfinding/A* is documented but not implemented and is therefore not listed as a shipped
technology.

## Current Status

- **Framework package:** active and consumed from Git by two Unity prototypes.
- **Drop The Man:** current systems-showcase MVP complete; final UI, art polish, and device
  acceptance deferred.
- **Color Block Escape:** movement, shared editor adoption, exit capture, and timer/outcome
  checkpoints complete; block prefab integration and chipper presentation remain in progress.
- **Validation:** focused framework movement/authoring tests pass. The latest recorded broad
  framework run passed 52/53 tests; the remaining failure is a tracked catalog-test assertion
  mismatch rather than a runtime compilation failure.

## What I Built / Role

This is my independent portfolio engineering project. I own the framework architecture, gameplay
systems, prototype integrations, custom level-authoring workflow, data formats, runtime
construction, documentation, tests, and cross-repository package/version workflow.

## Running and Using the Project

### Install the package

Add the package through Unity Package Manager using a verified full commit SHA:

```text
https://github.com/GameplaySystem/PuzzleFramework.git?path=/Packages/com.gaming.puzzleframework#<full-commit-sha>
```

### Run framework tests

1. Open `PuzzleFramework/` with Unity `6000.3.17f1`.
2. Follow [FrameworkValidationSetup.md](docs/FrameworkValidationSetup.md) to expose the sibling
   package tests in the validation project.
3. Run **Window > General > Test Runner > EditMode**.

To play the systems in context, use the linked DTM and CBE repositories; this repository is the
package and validation host.

## Screenshots / Media

The final portfolio media pass should add:

- a short cross-prototype gameplay reel near the top of this README
- one DTM/CBE side-by-side image showing shared modular-board or authoring behavior
- DTM and CBE editor screenshots
- the GitDiagram export, followed by the concise architecture explanation above

Only captures from the running projects will be used.

## Further Documentation

- [Framework architecture](docs/FrameworkArchitecture.md)
- [System specifications](docs/FrameworkSystems)
- [Project state](docs/PROJECT_STATE.md)
- [Implementation watchlist](docs/IMPLEMENTATION_WATCHLIST.md)
- [Framework package workflow](docs/Workflow/FrameworkPackageDependencyWorkflow.md)
