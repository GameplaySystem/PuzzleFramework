# Vision

Build a reusable Unity Puzzle Framework by deconstructing and recreating several successful puzzle games.

The framework should prioritize:

- Clean architecture
- Reusability
- Scalability
- Maintainability
- Documentation quality
- Portfolio value
- Learning software engineering fundamentals

The project should improve:

- Unity skills
- C# skills
- Architecture skills
- System design skills
- Technical documentation skills
- Problem solving skills

---

# Project Goals

## Primary Goals

- Build a reusable puzzle framework.
- Create portfolio-quality projects.
- Improve software engineering fundamentals.
- Improve systematic thinking.
- Become capable of designing systems before implementation.
- Increase employability for Unity gameplay programming positions.

## Secondary Goals

- Learn GitHub workflows.
- Learn AI-assisted development workflows.
- Learn professional documentation practices.
- Create material suitable for technical interviews.

---

# Target Games

## Drop Away

Grid-based puzzle game where colored holes collect matching colored stickmen.

Status:

- Deconstructed

## Color Block Jam

Grid-based puzzle game where colored bricks exit through matching colored doors.

Status:

- Deconstructed

## Sky Rush Traffic Puzzle

Grid-based puzzle game where buses collect stickmen from door queues.

Status:

- Deconstructed

## Hole People

Pathfinding-based puzzle game using board holes, buffers, and hole queues.

Status:

- Deconstructed

## Bus Jam

Pathfinding-based puzzle game using buses, buffers, and stickman selection.

Status:

- Deconstructed

---

# Development Philosophy

- Understand before implementing.
- Requirements before architecture.
- Architecture before code.
- Shared systems before game-specific systems.
- Framework systems should remain game-agnostic.
- Game modules should define rules.
- Documentation should be created before implementation.
- Decisions should be documented.

---

# Major Architecture Decisions

## Framework Rule

A system belongs in the framework if it is used by at least two games.

Status:

Approved

## Pathfinding

A* will be used as the primary pathfinding algorithm.

Status:

Approved

## Shape Rotation

Shapes may rotate inside the level editor.

Shapes do not rotate during gameplay.

Status:

Approved

## Board Structure

Boards are cell-based grids.

Status:

Approved

## Cell Types

Inactive Cell:

Not part of the board.

Blocked Cell:

Part of the board but cannot be occupied or traversed.

Status:

Approved

---

# Framework Categories

## Core Board Systems

- Grid System
- Cell Occupancy System
- Shape System
- Wall Generation System
- Pathfinding System

## Runtime Flow Systems

- Game State System
- Timer System
- Event System

## Interaction Systems

- Input System
- Drag Movement System
- Grid Snap System

## Resource Processing Systems

- Queue System
- Buffer System
- Capacity System

## Content Systems

- Level Data System
- Level Save Load System
- Level Editor Foundation
- Level Catalog System

Content Systems handle level content data, level content save/load, shipped level discovery, and
level editor foundation.

Content Systems do not instantiate runtime objects.

## Runtime Construction Systems

- Level Runtime Builder System
- Runtime Object Factory System
- Runtime Construction Validation System

Runtime Construction Systems are responsible for converting loaded authored level data into runtime objects and runtime state.

Framework runtime construction foundations are now implemented, but broader reusable object-factory generalization remains future scope.

## Progression Systems

- Player Progress Data System
- Progress Save Load System

Progression Systems handle player-owned progression state and persistence of that state.

## Presentation Systems

- Color System
- Visual Feedback System
- Modular Board Visual System

---

# Status Language

Progress markers below use these meanings:

- `[implemented]` = reusable code exists in the repositories now
- `[implemented foundation]` = a narrow MVP or reusable foundation exists, but broader generalization is still deferred
- `[prototype-only]` = implemented only in `DropAwayPrototype`, not yet extracted into shared framework code
- `[documented only]` = approved or designed in docs, but not implemented in code yet

# Current Progress

## Game Design

- Drop Away [done]
- Color Block Jam [done]
- Sky Rush [done]
- Hole People [done]
- Bus Jam [done]

## Technical Design

### Shared Systems Analysis

- Complete [done]

### Framework Architecture Diagram

- Complete [done]

### Implementation Roadmap

- Complete [done]

### Framework MVP Plan

- Complete [done]

### Implementation

- Framework package foundation [done]
- Separate Drop The Man prototype setup [done]
- Content Foundation [done]
- Grid System MVP foundation [done]
- Cell Occupancy System MVP foundation [done]
- Runtime Construction Validation foundation [done]
- Runtime Construction contract foundation [done]
- Blocked cell runtime board metadata foundation [done]
- Minimal concrete runtime builder foundation [done]
- Interaction capability contract foundation [done]
- Input System intent-flow foundation [done]
- Input target resolution and UI blocking contract foundation [done]
- Drag Movement contract foundation [done]
- Grid Snap contract foundation [done]
- Board world-layout contract foundation [done]
- Grid Snap runtime foundation [done]
- Drag Movement runtime foundation [done]
- Game State runtime foundation [done]
- Timer runtime foundation [done]
- Color runtime foundation [done]
- Drop The Man MVP game-module requirements definition [done]
- Drop The Man MVP rules definition [done]
- Drop The Man runtime contracts and payload parsing foundation [done]
- Drop The Man runtime model builder foundation [done]
- Implementation context check and watchlist foundation [done]
- Drop The Man movement-rule documentation reconciliation [done]
- Critical rule clarification log foundation [done]
- Drop The Man shape-capacity MVP clarification [done]
- Drop The Man movement and collection rules spec [done]
- Drop The Man movement coordinator design spec [done]
- Drop The Man swept footprint helper foundation [done]
- Shape footprint runtime foundation [done]
- Drop The Man movement coordinator foundation [done]
- Drop The Man drag-session owner foundation [done]
- Drop The Man release snap and occupancy commit foundation [done]
- Drop The Man full-hole completion-flow design [done]
- Drop The Man full-hole completion-flow foundation [done]
- Drop The Man win-predicate reconciliation and outcome-routing design [done]
- Drop The Man outcome router foundation [done]
- Drop The Man runtime integration / playable scene wiring design [done]
- Drop The Man runtime integration foundation [done]
- Drop The Man playable scene adapter design [done]
- Drop The Man dev-only scene bootstrapper / test level source [done]
- Board world-layout axis mapping support for XZ Drop The Man scene wiring [done]
- Drop The Man first playtest scene placeholder and pointer-offset fixes [done]
- Drop The Man footprint-aware boundary drag clamp [done]
- Drop The Man collection presentation timing design [done]
- Drop The Man JSON level pipeline foundation [done]
- Drop The Man collection presentation timing foundation [done]
- Drop The Man level editor design phase 1 [done]
- Drop The Man 10-slot color identity foundation [done]
- Drop The Man blocked-cell JSON authoring foundation [done]
- Drop The Man editor config foundation [done]
- Drop The Man editor phase 2 visual authoring shell [done]
- Drop The Man editor phase 2 play-mode authoring scene foundation [done]
- Drop The Man editor phase 3A save/export JSON foundation [done]
- Drop The Man editor phase 3B import/load JSON foundation [done]
- Drop The Man phase 4A JSON-driven gameplay runtime spawning [done]
- Drop The Man config-owned collectable prefab and gameplay-scene template cleanup [implemented, manual validation pending]
- Drop The Man gameplay movement-feel baseline (drag speed clamp, spawned-view scale, drag clearance inset) [done]
- Drop The Man phase 4B basic level result flow and level sequence [done]
- Drop The Man phase 4B stability fixes for terminal drag cleanup and next-level reload [done]
- Framework Resources level catalog and Drop The Man catalog adapter [implemented and manually validated]
- Drop The Man URP 17.3 rendering-pipeline baseline [done, prototype-owned]
- Drop The Man material-only URP stencil proof assets [manually validated proof, production adaptation pending]
- Drop The Man DOTween single-hole completion presentation [implemented and manually validated, prototype-owned]
- Centered rectangular `GridWorldLayout` construction [implemented, manual gameplay validation pending]

### Core Board Systems

- Grid System [implemented]
- Cell Occupancy System [implemented]
- Shape System [implemented foundation]
- Wall Generation System [implemented]
- Pathfinding System [documented only]

### Runtime Flow Systems

- Game State System [implemented]
- Timer System [implemented foundation]
- Event System [documented only]

### Interaction Systems

- Input System [implemented foundation]
- Drag Movement System [implemented foundation]
- Grid Snap System [implemented]

### Resource Processing Systems

- Queue System [documented only]
- Buffer System [documented only]
- Capacity System [documented only]

### Content Systems

- Level Data System [implemented]
- Level Save Load System [implemented]
- Level Editor Foundation [documented only in framework; first concrete tool is prototype-only]
- Level Catalog System [implemented foundation]

### Presentation Systems

- Color System [implemented]
- Visual Feedback System [documented only]
- Modular Board Visual System [implemented foundation]

### Runtime Construction Systems

- Level Runtime Builder System [implemented foundation]
- Runtime Object Factory System [documented only]
- Runtime Construction Validation System [implemented foundation]

### Progression Systems

- Player Progress Data System [documented only]
- Progress Save Load System [documented only]

---

# Current Focus

The reusable modular board-generation baseline is implemented, wired to the concrete Drop The Man
cell prefab, and manually validated in Unity.

Current category:

Content Systems / Drop The Man Runtime Integration

Current system:

The framework now provides a Resources-backed `LevelCatalogSystem` that treats authored
`TextAsset` contents as opaque, delegates metadata interpretation to game modules, rejects duplicate
ids and sequence numbers, reports sequence gaps, and exposes deterministic ordered entries.
Drop The Man supplies the JSON-validating canonical `Level N` metadata adapter. Its gameplay
bootstrapper now discovers `Assets/Resources/DropTheMan/Levels`, while editor export defaults to the
same folder. The old serialized scene sequence and direct JSON asset references are removed. Unity
Play Mode owner validation confirmed initial Level 1 discovery, Level 1 restart, and deterministic
Next loading of Level 2 without serialized scene references or Console errors.

Drop The Man collectable spawning now resolves one typed prefab from the existing prototype visual
config shared with editor previews. The gameplay scene no longer contains pre-placed primitive
hole/stickman test objects, and the obsolete non-spawning/template-hidden paths were removed.
The config intentionally has no collectable prefab assigned until the owner creates and wires the
placeholder cat prefab; Unity Play Mode validation is therefore pending.

Presentation baseline:

DropAwayPrototype uses a prototype-owned URP 17.3 baseline with one Forward renderer assigned
across Graphics Settings and every Quality tier. Existing project materials use URP Lit. The
pipeline, renderer, materials, and future stencil work remain outside PuzzleFramework; reusable
board topology and modular slot planning remain render-pipeline agnostic. A manually validated
material-only stencil proof now provides an invisible writer plus color-matched grid and cell receivers whose explicit
ForwardLit pass uses URP's Lit input/forward implementation without layers or renderer features.
It remains a prototype-owned rendering concern. DOTween Core is installed in the prototype, and
`DropTheManHolePresentation` defines the logical-root versus presentation-root split, named cap
blend shape, stencil aperture, collection sockets, reset and cancellation behavior, and cap-close/
shrink sequence. The corrected single-hole prefab passed isolated Unity validation. The prototype
runtime now splits `Full -> Closing` from callback-driven `Closing -> Completed`, with an immediate
fallback when presentation is absent or invalid. The callback path passed integrated Unity
validation. `DropTheManSceneController` now exposes a scene-wide toggle between closing from the
final freeform drag position and first applying a view-only framework snap to the nearest valid
footprint-origin cell. Neither option changes committed coordinates or occupancy; the preferred
visual policy remains under manual evaluation.

`GridWorldLayout.CreateCentered(...)` now derives the cell `(0,0)` world origin from a requested
board center, logical width and height, cell size, and orthogonal board axes. Drop The Man's
gameplay bootstrapper uses this shared layout with world zero as its default center, so generated
cells, holes, cats/stickmen, drag, snap, and collection conversion move together. Centering uses
the logical rectangular board bounds; blocked or visually absent cells do not shift the level.
Unity gameplay validation is still required.

The prototype's existing stencil Lit receiver now exposes its comparison operation per material.
Board cell materials continue rejecting aperture pixels with `NotEqual`, while the new
`Hole_Inner_Cavity_Stencil` test material uses `Equal` so the dark inner walls render only through
the matching aperture. The single-hole prefab now uses this material on its inner-wall submesh,
and the material-only real-hole check passed Game-view validation.

---

# Next Steps

1. Create a placeholder cat prefab with `DropTheManStickmanView`, assign it to the shared visual config, and validate editor preview plus gameplay spawn/collection/restart/next.
2. Manually validate that odd and even gameplay boards center on world zero and that blocked-cell changes do not shift the level.
3. Keep diagonal-only participating-cell contact unsupported until a deliberate visual policy is approved.
4. Compare full-hole closing with the scene-controller alignment toggle enabled and disabled, then choose the preferred visual policy.
5. Replace the temporary `OnGUI` editor and gameplay HUDs with real UI assets once they exist.
6. Revisit reusable level-editor extraction only after another prototype proves the shared authoring contract.
7. Begin Color Block Jam on top of the current framework and the documented Drop The Man lessons.

---

# Open Questions

- How should diagonal-only cell contact render if a future level contains that topology?
- After a second prototype uses authoring, which level-editor concerns are proven reusable enough to move into `PuzzleFramework`?
- Should Bus Jam buses use Shape System or remain road-only entities?
- Should Door System be framework-level?
- Should Visual Feedback remain framework-level or game-specific?

---

# Chat Migration Instructions

When continuing this project in a new ChatGPT conversation:

1. Provide this document.
2. Provide current progress.
3. Provide the current focus section.
4. Continue from the latest unfinished task.

This document is the source of truth for the project.
