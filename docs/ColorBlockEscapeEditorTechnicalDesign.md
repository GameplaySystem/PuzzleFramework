# Color Block Escape Editor And Shared Authoring Core Design

Date: 2026-09-20
Status: Approved baseline; framework, DTM and CBE editor adoption checkpoints implemented and verified.
Requirements: [Color Block Escape MVP](ColorBlockEscapeMVPRequirements.md).
Framework baseline: [Level Editor Foundation](FrameworkSystems/ContentSystems/LevelEditorFoundation.md).

## CBE implementation checkpoint

The CBE Play-mode authoring scene now consumes one live framework `LevelAuthoringCore`,
`AuthoringToolHost`, anchor-aware `BoardAuthoringPicker`, and derived board boundaries.
Its game-owned session retains block colors and exits, rejects structural edits that affect
blocks/exits or crop non-default cells, and stages import through the CBE payload codec and
runtime builder before replacing live state. Presets and custom connected offsets share the
same placement path, with a configurable default 4×4 guard for newly authored footprints.
The scene offers cell painting, resize, block placement/selection/move/recolor/rotation/erase,
exterior-edge exit editing, timer settings, JSON save/load and isolated gameplay play-test.
It uses `GridCellAnchor.Corner`, matching the existing CBE movement unit-square coordinates.

The primary mode workflow is keyboard-driven rather than a row of clickable mode buttons. CBE
owns the mnemonic bindings: `B` block placement, `M` block selection/move, `O` obstacle toggle,
`E` exit placement/editing, and `S` exit selection. Ordinary cells are Active by default; `O`
toggles `Active <-> Blocked`, so CBE authors do not manage separate Active, Inactive and Blocked
modes. The underlying framework states remain readable for payload compatibility. The HUD
displays the bindings and current mode. Mode shortcuts are ignored while an IMGUI text field owns
keyboard focus. `0-9` choose the corresponding shared color slot; the mouse wheel cycles the
block footprint presets while the pointer is over the board.
Block and exit erase are contextual actions rather than modes: right-click in `B` removes the
block under the picked cell, and right-click in `E` removes the exit under the picked exterior
edge. Left-click retains placement/edit behavior. Selection/move and structural painting remain
separate modes because their left-click semantics are materially different.

The CBE project imports the configured DTM modular board cell prefab and only its mesh/material
dependencies. The editor and authored play-test build that board through the framework modular
board planner/builder, while a narrow presentation-only edge override hides the two wall halves
covered by each validated CBE exit. Block views remain simple generated geometry for now; the
owner will prepare basic-shape block prefabs for later editor/runtime use.

Unity 6000.3.17f1 compiled the scene against the local framework source. CBE Edit Mode tests
passed 37/37 and Play Mode tests passed 4/4. The PlayMode bridge uses the existing runtime builder,
drag adapter, exit capture and outcome path. A hands-on Game-view pass for UI ergonomics, edge
highlighting and visual alignment remains open.
The current codec/runtime builder require at least one block and one exit before save or
play-test; partial draft export would need a separately approved content-policy decision.

## Problem and evidence

The framework has a partial `LevelAuthoringCore`, not a complete reusable editor. Drop The Man has a
working dedicated Play-mode editor in `DropTheManEditorBoardController`, its runtime HUD/input
bridge, and a thin SceneView wrapper. The controller mixes reusable board-cell rendering,
ray-to-cell mapping, resize, footprint fit and erase mechanics with hole/stickman modes, palette,
visuals and its own readable JSON. It rotates placed holes and erases items. It does not already
support true placed-item move, Active/Inactive painting, or an editor-to-runtime play-test bridge.
DTM currently uses the framework core for picking, blocked-cell edits, footprint fit and rotation,
but rebuilds it as a temporary validator rather than using it as the live editor session.

The owner now requires shared authoring behavior to live in PuzzleFramework and be consumed by
both games. The smallest safe extraction is a common editing **core and scene support**, not a
copy of the whole DTM controller or a universal editor-window/plugin platform.
The seven-day planning benchmark is a reason to keep that core focused, not a reason to duplicate shared
board/footprint editing inside CBE. The CBE editor must consume the framework core once it is
implemented; DTM must adopt the same core for its overlapping operations.
The approved second-consumer clarification in the framework
[`LevelEditorFoundation`](FrameworkSystems/ContentSystems/LevelEditorFoundation.md) now names the
needed session, view composition, tool dispatch and document boundaries. The owner approved it
with explicit cell anchoring, framework consequence reporting and game-owned structural policy.

## Second-consumer acceptance and migration boundary (approved)

The editor core is complete only when the existing DTM authoring scene and the new CBE authoring
scene both use its live board/footprint session, board picking, structural validation and board
visual composition. DTM's current use of temporary `LevelAuthoringCore` instances proves several
algorithms are reusable but does not satisfy that acceptance rule. Keep game HUDs, hotkeys,
camera framing, palettes and concrete item previews in their respective modules.

Use explicit registration of game-owned tools through one narrow framework tool contract. Shared
dispatch passes a picked cell or boundary edge and an authoring session to the active tool;
game tools provide previews and candidate edits. Framework validates generic cell-footprint
fit and structural state. DTM tools interpret stickmen and holes; CBE tools interpret blocks and
exits. Do not move either game's mode switch, color logic or payload schema into framework code.

Derived board edges use the existing framework wall-generation result. CBE's exit tool can pick
those edges, but its exterior-connectivity rule, opening width/color and overlap validation stay
in CBE. A generic edge-entity model is not justified by DTM's editor. CBE reports exits as
affected game-owned IDs through the structural-edit veto when resize or cell painting would
invalidate them.

Board picking needs an explicit anchor decision before UI code. The existing shared picker
rounds around integer-centered cells, matching DTM's board views. CBE's plain movement fixture
draws each logical footprint square from its integer corner to the next corner. Reusing the
rounding picker unchanged with those CBE visuals would select the wrong cell near half-cell
boundaries. The proposed shared picking/layout contract therefore exposes center-versus-corner
anchoring as a `GridWorldLayout` contract. DTM uses center anchoring and CBE uses corner
anchoring unless a separately reviewed movement/view alignment change is made. Verify picks at
cell centers and edges and at inactive gaps.

The shared session supplies a detached board/metadata snapshot and generic restore operation.
DTM maps its existing readable JSON and editor data into that session without changing the
format. CBE maps its own opaque payload into the same session. Import must validate a temporary
candidate before replacing the live session and game payload together; save/load services retain
persistence ownership. The play-test bridge uses a detached snapshot but remains CBE-owned,
because DTM has no corresponding editor play-test workflow to generalize.

## Ownership and minimal common core

PuzzleFramework owns an authoring session over generic `LevelDefinition` fields: explicit board
dimensions/cell states, metadata and timer data, selected cell/item identity, and operations for
board resize, cell-state change, footprint translation/rotation fit, and erase. It provides
board-local cell picking using `GridWorldLayout`, a simple shared board-cell visual update path,
and a narrow tool hook so game-owned tools can receive selection/placement actions and return
validation/preview data. It exposes save/load *integration* through existing content services;
serialization and game payload interpretation remain with their owners. Generic structural checks
remain distinct from module placement rules and gameplay solvability.

Keep this contract small: one current tool, one current selection, a generic footprint and
coordinate operation set, and explicit request/result data. Do not create a registry of arbitrary
plugins, service locator, broad undo command framework, or special noun in framework code.
The framework may offer placement preview of cells/footprint; concrete object preview prefabs,
materials and inspector controls are game-owned. A saved block footprint is explicit offsets,
not a shared shape enum.

The DTM adapter continues to own hole/stickman modes, hole palette, color material mapping and
its existing JSON format. At import/export boundaries it maps its current authored model to the
shared board/metadata session; its payload stays opaque to framework code. CBE owns block shape
presets, color/rotation controls, exit edge tools, gate previews and payload codec. Each game's
dedicated authoring scene/HUD may remain separate. DTM must be changed to call the new common
core for the behavior it demonstrably shares, while preserving existing content format and
editor behavior. No new DTM play-test or item-move feature is implied by the migration.

## CBE authoring model and tools

The live editor session holds a complete generic board plus a CBE block/exit payload in memory.
The board supports Active, Inactive and Blocked painting. Default newly added cells are Active.
On resize or changing Active to Inactive/Blocked, identify every block/exit that would become
invalid and reject the operation with a clear list unless the user deliberately removes or
relocates those items first. Do not silently delete, relocate or crop authored data. Rebuild
derived walls and previews from
the accepted board state.

Block tool flow: choose a footprint preset or existing explicit footprint, choose one of ten
colors, press `B` and left-click to place it at a cell, press `M` to select a placed block and
move it by choosing/dragging a new origin, recolor it, rotate its occupied offsets in 90-degree
steps, or press `B` and right-click it to erase it. Placement/move/rotation
must check Active cells, bounds, blocked cells and overlaps before changing session data.
Selection is actual placed-item identity, not merely selected palette/color. A failed operation
leaves the previous item state intact. The displayed preview derives from stored offsets and
simple Unity geometry. Preset names never enter saved level data. Allow 3×3 where useful and
the listed L/T/S/Z variants; initial 4×4 limit is a configurable authoring guard, not a schema
limit, pending playtest/reference findings.

Exit tool flow: choose side, select a starting exposed edge cell, choose width and color, then
press `E` and left-click to create or update it. Press `S` to select an existing exit for field
editing; press `E` and right-click an exit edge to delete it. Highlight the whole proposed
opening and report boundary, continuity,
exterior connectivity, overlap and width errors before applying. Permit multiple same-color
exits. The visual preview masks derived walls at the validated opening and positions a simple
gate/chipper view outside it. The game payload stores side/start/width/color only; preview
transforms are derived. Timer fields are edited using the shared level envelope.

## Save, load and play-test

One CBE payload parser/serializer serves editor save/load and gameplay startup. Export preserves
IDs, full footprint offsets, colors, cell states, exits and timer. Import parses into a temporary
model, performs schema and construction-readiness validation through their owning services, and
replaces the live editor session only on complete success. Error messages identify the offending
item/cell/exit. Unknown payload versions fail explicitly. A save/load round trip is checked
semantically rather than by JSON formatting. The DTM adapter keeps its own existing format; the
shared core does not force a schema migration on that maintenance baseline.

Play-test creates an isolated runtime session from a snapshot of the current valid authored
model, using the **same** CBE construction and gameplay path as a loaded level. It does not
convert editor preview objects into runtime state. Returning to the editor disposes runtime
views, timer and callbacks, then restores the unchanged editing session and selection. Play-test
does not write progression or silently save. A failed runtime build displays the construction
errors and leaves the editor usable. This bridge is CBE-owned; the framework core only supplies
the authored snapshot and generic validation entry points.

## Migration and verification sequence

1. Extend the existing `LevelAuthoringCore` into a live session and add only the small tool/view
   composition needed by both consumers. Verify board import, structural edits, footprint
   selection/move/rotation/erase, preview fit, boundary picks and failure atomicity.
2. Migrate DTM's working scene onto that session and shared composition. Preserve its current
   hole/stickman tools, blocked-cell behavior, JSON, visuals, HUD and warned prune-on-resize.
   Verify existing editor import/export, rotation, picking and resize before proceeding.
3. Add CBE block/exit tools on the same shared foundation, then CBE save/load and isolated
   play-test handoff. Verify malformed import leaves the session unchanged, invalid structural
   edits list affected blocks/exits, and authored exits never target enclosed inactive holes.

The existing framework core is partial and cannot simply be “turned on” as a full editor. The migration changes both framework and
DTM source and must follow the pinned-package update workflow: publish and verify the new
framework revision, update DTM's full-SHA dependency, compile/test DTM, and then commit/push
the consumer under the repository's Git workflow. CBE is a second consumer and follows the
same rule.
Approval of this design does not itself change any package pins or repository content outside
documentation; those changes occur during implementation and verification.

## Seven-day risk

Framework extraction, DTM regression migration, true selection/move, irregular-edge exit editing,
round-trip persistence, and play-test together are probably the largest schedule risk. The
minimum shared core above limits that risk, but completion within seven development days is not
guaranteed. If necessary extraction takes longer, report the actual cause and options;
do not silently replace the required shared core with a CBE-only duplicate or omit play-test.
Reduce speculative extension points, unrelated features and polish before considering a change
to the agreed framework/game-module boundary.
