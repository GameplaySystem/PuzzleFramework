# Level Editor Foundation

## Document Metadata

Category:
- Content Systems

Status:
- Approved implementation baseline; shared session implemented and adopted by DTM and CBE

Parent:
- Overview.md

Related Documents:
- LevelDataSystem.md
- LevelSaveLoadSystem.md
- ../RuntimeConstructionSystems/Overview.md
- ../CoreBoardSystems/GridSystem.md
- ../CoreBoardSystems/ShapeSystem.md
- ../CoreBoardSystems/WallGenerationSystem.md
- ../PresentationSystems/ModularBoardVisualSystem.md
- ../InteractionSystems/InputSystem.md
- ../../ColorBlockEscapeEditorTechnicalDesign.md

Depends On:
- Level Data System
- Level Save Load System
- Grid System
- Shape System
- Wall Generation System
- Modular Board Visual System
- Input System (board-plane projection/picking)

Used By:
- Drop Away
- Color Block Jam
- Sky Rush
- Hole People
- Bus Jam

## Dual-adoption checkpoint

Drop The Man uses the live session with center anchoring while retaining its game-specific
payload, tools and warned prune-on-resize policy. Color Block Escape uses the same session,
shared cell/edge picking and tool host with corner anchoring; its module owns block colors,
exit exterior validation, reject-on-conflict edits, codec mapping and play-test handoff.
The CBE authoring scene compiled under Unity 6000.3.17f1 and its Edit Mode and Play Mode suites
passed 21/21 and 2/2. Hands-on Game-view validation of editor ergonomics remains open. The
historical design discussion below describes the pre-implementation state and is retained as
the rationale for the shared boundary.

## Approved second-consumer implementation clarification

`LevelAuthoringCore` already exists in the framework. It owns explicit board cells, metadata and
timer fields, cell/item selection, generic footprint placement and move checks, rotation math,
erase, snapshots and cell picking. Drop The Man already calls it for picking, blocked-cell edits,
fit and rotation, but reconstructs a temporary core from its game-owned data for each structural
check. This is partial reuse, not yet a shared editor foundation with one live authoring session.
Color Block Escape has no dedicated editor yet. The task is to complete and adopt the existing
core, not to copy Drop The Man's large scene controller into the package.

The smallest coherent foundation has four focused parts:

1. **Authoring session and structural operations.** Keep `LevelAuthoringCore` as the generic
   board/metadata/footprint authority. Add only the missing session operations demonstrated by
   the two games: restoration from an authored snapshot, lookup/selection of a footprint item at
   a cell, move/rotation/erase commands with failed edits leaving the old state intact, and
   non-mutating fit/preview results. A placed cell entity is an ID, origin and explicit offsets;
   game payloads and visual prefabs stay outside the session. Structural inspection reports
   affected IDs and non-default cells without changing the board. DTM may explicitly prune on
   resize; CBE rejects invalidating edits. A game may report its own edge-attached content.
2. **Board picking and visualization composition.** Reuse `GridWorldLayout`, the existing
   board-plane projection/picking, `WallGenerationSystem`, and the modular board visual
   planner/builder. Supply a narrow authoring board-state/view mapping only where these existing
   services leave a gap. The plan exposes cell states and derived boundary edges; game adapters
   choose which cells participate in a particular visual profile and supply prefabs, materials,
   camera framing and previews. Do not build a second wall engine or force one blocked-cell
   visual meaning on both games. Picking and visuals must explicitly share one cell-anchor
   convention: DTM currently picks integer-centered cells with rounding, while the CBE plain
   movement fixture draws logical unit squares at integer corner origins. Support that proven
   difference through an explicit `GridCellAnchor` on `GridWorldLayout`, shared by picking and
   world placement. DTM uses center anchoring; CBE uses corner anchoring. Test picks at centers,
   edges and irregular-board gaps.
3. **Explicit tool dispatch and validation boundary.** One active tool is selected from an
   explicitly registered set through a small framework-owned tool contract. The host routes
   generic cell/edge picks and preview/apply requests; registered game tools interpret them and
   own palettes, colors, IDs and payload edits. Framework placement checks cover bounds,
   Active/Inactive/Blocked state and footprint overlap. Game hooks may reject a structural edit
   that invalidates game-owned content, or add game-specific validation, without inspecting
   opaque payloads in framework code. No reflection-based discovery, arbitrary plugin platform,
   service locator, undo stack or monolithic universal controller is required.
4. **Document handoff.** The session produces a detached generic board/metadata snapshot for
   existing `LevelDefinition` and persistence services. Each game maps its own payload and
   stages imports before replacing the live editor state. Framework provides snapshot and
   structural validation entry points, not a new JSON format or a runtime play-test service.
   A game-owned play-test bridge may consume a detached snapshot through its normal runtime
   construction path.

`BoardBoundaryEdge` and wall geometry are shared structural facts. A CBE exit is not a generic
cell-footprint item: its side, width, color, exterior connectivity, overlap with other exits and
preview remain CBE-owned. DTM currently has no edge-authored entity. Do not invent a generic
edge-entity schema until another concrete consumer needs one.

The foundation is accepted only after **both** consumers use it in their working authoring
flows. DTM must use the persistent shared session and common picking, structural edits,
footprint operations and board-view composition while preserving its existing scene, tools,
JSON and resize-with-warning behavior. CBE must use the same foundation for its board and
block editing while supplying its own block and exit tools and reject-invalidating-edit policy.
DTM's prune-on-resize policy stays in its adapter; the shared operation reports conflicts and
never silently discards content. CBE rejects conflicts until the author explicitly changes
the affected content. No DTM play-test or placed-item move UI is implied by migration.

Implement and verify the framework session/tool/view slice first, then migrate DTM and verify
its current editor workflow, then build CBE tools and verify the second consumption. Report each
layer before proceeding. The package pin workflow applies whenever either prototype consumes a
new framework revision. This clarification changes no gameplay, persistence or runtime
construction ownership.

## Approved 2026-09-20 implementation slice

Implement the smallest generic authoring core demonstrated by Drop The Man and required by
Color Block Escape: explicit board dimensions/Active-Inactive-Blocked cell edits; metadata/timer
edits; board-local cell picking and board visualization; selection by placed-item identity;
generic footprint fit, translation, editor-only offset rotation and erase operations; structural
validation; save/load integration through existing content services; and a narrow hook for
game-owned placement tools and previews. Reject structural edits that would invalidate existing
authored items; report the affected items without silently deleting, moving or cropping them.
PuzzleFramework owns this core and both games consume it. Each game owns item payloads, palette,
concrete previews, rules and UI. The CBE play-test bridge is game-owned and invokes normal runtime
construction from an isolated authored snapshot. No undo/redo or generic plugin platform is in
this slice. See the approved
[editor technical design](../../ColorBlockEscapeEditorTechnicalDesign.md).

Drop The Man's approved editor design intentionally prunes out-of-bounds authored content on
resize with a warning. Its adapter retains that behavior; it consumes shared picking, footprint
fit/rotation and blocked-cell validation, while Color Block Escape uses the non-destructive
structural edit operation. This difference must not be hidden inside a universal resize policy.

## Purpose

The Level Editor Foundation provides generic editor capabilities for creating and editing `LevelDefinition` data.

It exists to give the framework a reusable editor shell and a small set of extension points without embedding puzzle-specific authoring logic inside shared editor code.

The Level Editor Foundation is not responsible for puzzle-specific authoring tools.

---

# Core Design Idea

The approved editor direction is:

* framework provides the editor foundation
* game modules provide editor-specific adapters or extensions

Conceptually:

```text
Framework Editor
    ↓
Editor Extension Contract
    ↓
Game Module Editor Tools
```

Examples of game-module tools:

* Drop Away Editor Tools
* Color Block Jam Editor Tools
* Sky Rush Editor Tools
* Hole People Editor Tools
* Bus Jam Editor Tools

The framework editor must not know about these concrete game tools.

This is the right balance for now because the project needs reuse across five target games, but it does not need a full Unity Asset Store-grade plugin framework.

---

# Responsibilities

The Level Editor Foundation is responsible for:

* Grid visualization
* Cell visualization
* Active cell editing
* Inactive cell editing
* Blocked cell editing
* Generic shape placement support
* Level metadata editing
* Save integration
* Load integration
* Validation hook support
* Tool registration support
* Editor extension support
* Selection support
* Placement preview support

The framework editor owns generic editing mechanics.

Game modules own puzzle-specific interpretation.

---

# Should Not Handle

The Level Editor Foundation should not handle:

* Hole placement logic
* Stickman placement logic
* Bus placement logic
* Door placement logic
* Brick placement logic
* Puzzle-specific validation
* Puzzle-specific win/loss rules
* Runtime object spawning
* Runtime object initialization
* Runtime level construction

The editor foundation may support generic placement workflows.

It must not know the gameplay meaning of the thing being placed.

---

# Editor Extension Model

The approved architecture is:

```text
Framework Editor
    ↓
Editor Extension Contract
    ↓
Game Module Editor Tools
```

The framework provides:

* the shared editor shell
* generic editing workflows
* extension registration points
* shared validation hooks

Game modules provide:

* puzzle-specific editor tools
* puzzle-specific placement logic
* puzzle-specific content inspectors
* puzzle-specific authoring validation where needed

Useful conceptual framing:

* framework provides the socket or contract
* game module provides the adapter

This should remain lightweight.

The current project does not need a deeply layered plugin ecosystem.

It only needs enough extension capability to support the five target games cleanly.

---

# Generic Editing Capabilities

The framework editor may support generic placement and editing workflows.

Examples:

* place object
* move object
* remove object
* rotate shape (editor only)

Other shared generic capabilities may include:

* selecting cells
* selecting placed items
* previewing placement
* editing generic metadata

Important boundary:

The framework editor may support how something is placed generically.

It should not know what the placed thing means.

For example:

* generic shape placement support belongs here
* hole-specific behavior does not
* bus-specific meaning does not
* brick-specific rule interpretation does not

---

# Validation Philosophy

The framework editor may perform:

* structural validation
* missing field validation
* invalid cell coordinate checks
* invalid board bounds checks

These are generic content-structure concerns.

The framework editor must not perform:

* puzzle-specific validation
* game-specific success checks

Those belong to:

* game module editor extensions
* future validation systems where appropriate

This boundary matters because editor validation often expands too easily into gameplay logic.

The framework should validate structure.

Game modules should validate puzzle meaning.

---

# Relationship With Level Data System

The Level Editor Foundation creates and edits `LevelDefinition` data.

Conceptually:

```text
Level Editor Foundation
    ↓ creates/edits
LevelDefinition
```

The Level Data System owns the structure of the level model.

The Level Editor Foundation provides the editor-facing capabilities to author that structure.

Boundary:

* `LevelDataSystem` defines what the data model is
* `LevelEditorFoundation` defines how users and tools edit that data model

The editor should not redefine the structure owned by the Level Data System.

---

# Relationship With Level Save Load System

The Level Save Load System persists authored level data.

Conceptually:

```text
Level Editor Foundation
    ↓ creates/edits
LevelDefinition
    ↓ persisted by
Level Save Load System
    ↓
JSON
```

The editor may integrate with save and load workflows.

It should not absorb persistence responsibility itself.

Boundary:

* editor edits level data
* save/load persists level data

---

# Relationship With Runtime Construction Systems

The editor creates `LevelDefinition` data.

The editor does not create playable runtime levels.

Runtime Construction Systems are responsible for converting loaded level data into runtime objects.

Conceptually:

```text
Level Editor Foundation
    ↓ creates/edits
LevelDefinition

Level Save Load System
    ↓ persists
JSON

Runtime Construction Systems
    ↓ consume
Loaded LevelDefinition

Game Modules
    ↓ interpret
Game-specific content
```

This is a major architecture boundary.

If the editor starts spawning playable runtime content directly as part of its shared responsibility, it will blur into runtime construction and puzzle-specific tooling.

That belongs elsewhere.

---

# Example Workflow

One expected authoring flow is:

```text
Designer opens framework editor shell
    ↓
Designer edits board and generic metadata
    ↓
Game module editor tools expose puzzle-specific authoring controls
    ↓
Editor produces LevelDefinition
    ↓
Level Save Load System saves LevelDefinition as JSON
    ↓
Runtime Construction Systems later load and build runtime objects
```

This flow keeps responsibilities separated:

* editor authors data
* save/load persists data
* runtime construction builds runtime objects
* game modules define puzzle-specific authoring meaning

---

# Edge Cases

## Extensibility Overbuilt Too Early

Risk:

The editor foundation becomes an abstraction-heavy plugin framework before the five target games actually require that complexity.

Rule:

Only introduce extension points that support known shared needs across the target games.

Do not design for hypothetical third-party plugin ecosystems.

---

## Shared Editor Shell Becomes Puzzle-Specific

Risk:

Convenient game-specific tools are added into the framework shell because they are useful for one game right now.

Rule:

If the tool requires puzzle-specific nouns or puzzle-specific rules, it belongs in a game module extension, not in framework editor code.

---

## Validation Drifts Into Game Rules

Risk:

Generic editor validation expands into “is this level good?” instead of “is this structure valid?”

Rule:

Framework editor validates structure.

Game modules validate puzzle-specific meaning.

---

## Preview Flows Start Building Runtime State

Risk:

Placement previews or editor tooling accidentally become runtime construction paths.

Rule:

Editor previews should remain authoring-time support, not runtime level-building ownership.

Runtime Construction Systems remain responsible for converting loaded level data into runtime objects.

---

## Shape Editing Scope Expands Too Far

Risk:

Generic shape editing begins to assume puzzle-specific semantics.

Rule:

Shared shape manipulation is acceptable only while it remains generic authoring behavior.

Game-specific meaning belongs in extensions.

---

# MVP Scope

The first version of the Level Editor Foundation should support:

* A reusable framework editor shell
* Grid visualization
* Cell visualization
* Active, inactive, and blocked cell editing
* Generic shape placement support
* Selection support
* Placement preview support
* Level metadata editing
* Save integration
* Load integration
* Lightweight extension points for game module tools
* Basic structural validation hooks

The MVP should not attempt to become a general-purpose editor framework beyond the needs of the five target games.

---

# Future Extensions

The following can be added later:

* undo and redo
* copy and paste
* bulk editing
* editor shortcuts
* validation reports
* custom editor windows
* import and export tools

These are future extensions only.

They should not drive the initial architecture before concrete reuse needs appear.

---

# Final Design Rule

The Level Editor Foundation provides generic editor capabilities for authoring `LevelDefinition` data.

Framework provides the editor foundation.

Game modules provide editor-specific adapters and extensions.

The framework editor must remain generic.

It must not absorb puzzle-specific tools, runtime construction, or gameplay logic.
