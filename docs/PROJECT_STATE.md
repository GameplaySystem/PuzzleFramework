# Active Prototype Transition - 2026-09-17

Drop The Man is **DEVELOPMENT-COMPLETE FOR CURRENT MVP SCOPE** with
**PRESENTATION/ASSET INTEGRATION DEFERRED**, by owner decision. Its core loop is closed:
level load -> play -> win/fail -> next/restart -> load again. It remains a maintenance/regression
target and a future presentation-integration target, not an abandoned or rewritten prototype.

Color Block Jam is the active second prototype, in architecture/design preflight. No gameplay
implementation is authorized by this transition. See the
[Color Block Jam architecture preflight](ColorBlockJamArchitecturePreflight.md) for evidence,
reuse classification, proposed boundaries, and unresolved requirements.

Return to Drop The Man only for a critical bug, a shared-framework regression affecting it, or
an explicit return to presentation integration when final UI/art assets are available. Existing
[remaining work](DropTheManRemainingWork.md) is retained as deferred acceptance/presentation work;
unchecked items are not newly verified and do not block beginning Color Block Jam.

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

- Active second prototype: architecture/design preflight.
- Earlier high-level deconstruction is not an approved detailed gameplay requirements baseline.

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
- Color Block Jam [high-level deconstruction recorded; detailed reference rules/MVP scope pending]
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
- Drop The Man editor concrete hole preview and placed-hole rotation workflow [implemented, manual validation pending]
- Drop The Man board-size-aware gameplay/editor camera positioning [implemented, manual validation pending]
- Drop The Man phase 4A JSON-driven gameplay runtime spawning [done]
- Drop The Man config-owned collectable prefab and gameplay-scene template cleanup [done]
- Drop The Man asynchronous cat collection presentation and moving-hole socket tracking [implemented; facing, Idle, and runtime state selection validated; owner collection visual confirmation pending]
- Drop The Man gameplay movement-feel baseline (drag speed clamp, spawned-view scale, drag clearance inset) [done]
- Drop The Man phase 4B basic level result flow and level sequence [done]
- Drop The Man phase 4B stability fixes for terminal drag cleanup and next-level reload [done]
- Framework Resources level catalog and Drop The Man catalog adapter [implemented and manually validated]
- Framework commit-pinned Git package consumption workflow [implemented and Unity-resolved]
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

- Player Progress Data System [implemented; snapshot tests passed]
- Progress Save Load System [implemented; local JSON persistence tests passed]

---

# Current Focus

Color Block Jam architecture/design preflight, following the owner-authorized Drop The Man MVP
freeze on 2026-09-17. Establish reference rules, asset evidence, content and construction boundaries,
movement/exit ownership, and the smallest justified reuse slice before implementation.

The [preflight report](ColorBlockJamArchitecturePreflight.md) is a proposal, not an approved system
spec. No Color Block Jam Unity project has been created. The framework drag implementation checks
rounded destinations only; continuous swept movement is prototype-owned today. Gate traversal and
exact removal timing require reference evidence before an extension can be approved.

## Preserved Drop The Man Delivery History

The following records delivered work and remaining verification at the time of each slice. It is
maintenance context, not the active work queue; the transition above governs prioritization.

September 2 follow-up: owner approved progression persistence, completed-level replay and
configurable post-campaign looping. The framework now owns versioned completed-ID/resume-ID
snapshots and safe local JSON replacement, separate from authored content. The prototype owns
campaign/replay/range policy and a profile session. All 38 new tests passed in isolated Unity
validation, including real runtime completion callbacks, loss-before-completion, duplicate wins,
file replacement and Editor path isolation. Full runtime source compiles in that validation host.
The broader suite exposed one existing catalog-test assertion failure (`Has.Count` on an array),
not a progression failure. On September 3 the owner approved publication and consumer adoption.
Framework `96e9b7751686f2652c0374a40841e74c96c74c9f` is pushed and pinned in the prototype manifest
and Unity-resolved lockfile. September 3 replay continuation amendment is now implemented: Next
Level chains completed replays, joins first unfinished campaign content, and returns to saved
campaign/loop selection after the final shipped replay. Resume Campaign remains separate; replay
does not move saved progress. Actual prototype compilation and all 27 Edit Mode tests passed;
gameplay-scene startup passed again with the Editor sandbox HUD. No profile was created or advanced.
Prototype `cee412bc27ff5956819cbfcd35dd165c54fd19bb` is committed and pushed, excluding unrelated
scenes/material/new-level edits. Owner UI win/stop/reopen/replay acceptance and device verification
remain. See prototype `docs/DropTheManProgressionImplementationHandoff.md` for maintenance and
[Drop The Man remaining work](DropTheManRemainingWork.md) for the prioritized finishing checklist.

September 4 repository cleanup published the intentional prototype asset state: `2dd7def` preserves
the tuned 0.5s approach/1.0s fall cat presentation, eight-prefab gameplay configuration, progression/
camera scene settings, and modular editor-board preview after removing nine redundant inactive root
prefab staging objects. `a743278` adds canonical `Level 3` (10x10, five four-cell holes, 20 cats).
Unity recompiled and all 27 prototype Edit Mode tests passed; Level 3 also passed an explicit ID,
bounds, overlap, and per-color capacity audit. Both repositories were clean before README work.

The reusable modular board-generation baseline is implemented, wired to the concrete Drop The Man
cell prefab, and manually validated in Unity.

Category at delivery:

Progression Systems / Drop The Man Integration And Finish Validation

Delivered systems:

The framework now provides a Resources-backed `LevelCatalogSystem` that treats authored
`TextAsset` contents as opaque, delegates metadata interpretation to game modules, rejects duplicate
ids and sequence numbers, reports sequence gaps, and exposes deterministic ordered entries.
Drop The Man supplies the JSON-validating canonical `Level N` metadata adapter. Its gameplay
bootstrapper now discovers `Assets/Resources/DropTheMan/Levels`, while editor export defaults to the
same folder. The old serialized scene sequence and direct JSON asset references are removed. Unity
Play Mode owner validation confirmed initial Level 1 discovery, Level 1 restart, and deterministic
Next loading of Level 2 without serialized scene references or Console errors.

Cross-repository package delivery now uses a Git package URL targeting
`/Packages/com.gaming.puzzleframework` and an immutable full framework commit SHA. Drop The Man no
longer uses a relative local `file:` dependency, and Unity Package Manager resolved the pinned
remote revision and compiled the project successfully. The canonical workflow requires framework
verification, commit, and push before any consumer updates its manifest and resolved lock file.
This removes sibling-folder assumptions and prevents a prototype from referencing a framework
commit that collaborators cannot fetch.

Drop The Man collectable spawning resolves one typed cat prefab from the existing prototype visual
config shared with editor previews. The gameplay scene no longer contains pre-placed primitive
hole/stickman test objects, and the obsolete non-spawning/template-hidden paths were removed. The
cat prefab now owns a six-variant animation plus DOTween collection presentation. It rises toward
the closest unclaimed socket in its already-reserved hole, follows that live socket while the hole
moves, falls below it while shrinking, and reports completion by direct callback. Runtime capacity
fills only after that callback and resolves the reserved hole independently of active drag state,
so release does not cancel an in-flight collection. Integrated Play Mode validation covered a
second hole movement update, release during presentation, cat disappearance, and later full-hole
completion. Each spawned cat now receives one persistent falling variant from a shared shuffled
cycle, ensuring all configured jump clips are assigned before any clip repeats. The September 2
artist delivery in `Cat.fbx` now supplies the canonical mesh, Generic Avatar, `Idle_1`, and
`Jump_1` through `Jump_6`. Both Idle takes loop; Jump takes do not. The old `Cat_3D` mesh and the
new clips have incompatible rest/bind poses despite matching bone names, so they are not mixed.
The cat-model root owns the Animator, and collection immediately plays the preassigned controller
state from normalized time zero. The repeatable setup no longer creates rebased `CatCollection_*`
animation copies. The replacement was supplied locally, not through a commit. Its renamed takes
required removing stale importer entries that otherwise imported no clips. Matched early renders
now show distinct motion with correct facing, intact mesh deformation, and white details. The
runtime tint is restricted to the body material slot. Collection is tuned to 0.5s approach + 1.0s fall;
the FBX clips are neither rebased nor stretched to that duration.

September 2 validation passed: editor motion/binding/early-mesh checks; repeatable setup; 36 cats
covering six complete shuffle cycles, live target movement, shrink and once-only callbacks; all
20 level cats across five holes with move/release, delayed fill, hole completion and win; restart
mid-fall and next-level reload. Obsolete separate FBXs, generated animation copies, and temporary
diagnostic scripts were removed. Setup retains valid authored trims and existing controller/model
tuning. See the prototype's `docs/DropTheManCatAnimationIntegrationHandoff.md` for maintenance,
verification details and remaining visual checks. Unrelated scene/content/material edits are
excluded from the animation commit; the framework package pin is unchanged.

Concrete Drop The Man hole prefab preparation now supports multiple pointer-selection colliders per
view so non-rectangular L, T, and plus surfaces can be covered without changing runtime footprint
authority. Existing single-collider prefab data remains supported through a serialized migration
fallback. All eight canonical shape prefabs are now authored in the prototype visual config, and a
prototype-owned resolver matches exact runtime footprint sets across quarter-turn rotations before
spawning the corresponding prefab. Missing, duplicate, or rotationally ambiguous mappings fail
startup explicitly. JSON and framework runtime construction remain presentation-agnostic. Owner
validation confirmed distinct runtime visuals for all eight configured canonical unrotated
footprints. Rotated variants, individual root alignment, every collider set, and each presentation
contract still require focused Unity validation.

The dedicated Drop The Man authoring scene now resolves those same concrete hole prefabs for
authored hole visuals instead of rebuilding shapes from square placeholder blocks. Hole placement
uses the palette's canonical footprint orientation, while `R` now switches the editor into a
prototype-owned hole-rotation mode that rotates an already placed authored hole on click with the
same structural bounds, blocked-cell, stickman, and overlap validation used during placement. This
keeps rotation editor-only and content-owned without adding runtime gameplay rotation behavior.

DropAwayPrototype now has one prototype-owned perspective-camera positioning calculation shared by
the gameplay bootstrap and editor board refresh paths. It derives the complete logical board
rectangle from `GridWorldLayout`, fits all four padded corners against the current aspect ratio and
field of view, centers the rectangle, and changes only camera position. Gameplay uses the pointer
adapter's explicit camera reference; the editor accepts its runtime camera reference and falls back
to the scene's tagged main camera for edit-time refreshes. Rotation, FOV, projection settings,
content data, and framework runtime state remain unchanged.

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
validation. `DropTheManCatCollectionPresentation` now uses six controller states backed by
directly referenced non-looping falling clips and a two-phase rise/fall tween with concurrent shrink.
Hole presentation claims the closest available
authored socket and the tween reevaluates that socket every update. Missing or invalid cat
presentation retains an immediate callback fallback so visual setup cannot strand runtime state.
`DropTheManSceneController` exposes a scene-wide toggle between closing from the
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

1. Review the [Color Block Jam preflight](ColorBlockJamArchitecturePreflight.md) and approve a
   bounded reference-game/MVP rule set, especially continuous movement, rotation, gate geometry,
   exit timing, and terminal precedence.
2. Review the read-only `tetra_pack.fbx` findings and proposed footprint-composed unit visuals.
   The supplied pack has repeated four-cell meshes and no standalone unit model. Approve asset
   preparation and Unity fidelity/pivot/material checks before finalizing the visual-config schema.
3. Approve Color Block Jam content, construction, interaction and editor specs; authorize project
   setup separately. Keep all game-owned code/assets in a separate prototype.
4. Implement only the smallest approved slice, reusing existing framework contracts first. Any
   shared movement or visual extension requires documented cross-game evidence and approval.
5. Preserve Drop The Man's pinned regression baseline. Follow the framework-first package workflow
   if either consumer adopts a new framework revision. The catalog assertion issue remains open;
   historical focused test results are not proof of a green full suite.
6. Keep [Drop The Man deferred work](DropTheManRemainingWork.md) for the permitted maintenance or
   presentation return; it is not a prerequisite for the second prototype.

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
