# Color Block Escape Editor And Shared Authoring Core Design

Date: 2026-09-20
Status: Approved by owner on 2026-09-20, including the six approval clarifications.
Requirements: [Color Block Escape MVP](ColorBlockEscapeMVPRequirements.md).
Framework baseline: [Level Editor Foundation](FrameworkSystems/ContentSystems/LevelEditorFoundation.md).

## Problem and evidence

The framework documents a generic editor but has no editor implementation. Drop The Man has a
working dedicated Play-mode editor in `DropTheManEditorBoardController`, its runtime HUD/input
bridge, and a thin SceneView wrapper. The controller mixes reusable board-cell rendering,
ray-to-cell mapping, resize, footprint fit and erase mechanics with hole/stickman modes, palette,
visuals and its own readable JSON. It rotates placed holes and erases items. It does not already
support true placed-item move, Active/Inactive painting, or an editor-to-runtime play-test bridge.

The owner now requires shared authoring behavior to live in PuzzleFramework and be consumed by
both games. The smallest safe extraction is a common editing **core and scene support**, not a
copy of the whole DTM controller or a universal editor-window/plugin platform.
The seven-day target is a reason to keep that core focused, not a reason to duplicate shared
board/footprint editing inside CBE. The CBE editor must consume the framework core once it is
implemented; DTM must adopt the same core for its overlapping operations.
Before package implementation, update the approved framework `LevelEditorFoundation` spec to
name the actual core contracts and dependencies, preserving its game-agnostic ownership rule.

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
colors, place it at a cell, select a placed block, move it by choosing/dragging a new origin,
recolor it, rotate its occupied offsets in 90-degree steps, or erase it. Placement/move/rotation
must check Active cells, bounds, blocked cells and overlaps before changing session data.
Selection is actual placed-item identity, not merely selected palette/color. A failed operation
leaves the previous item state intact. The displayed preview derives from stored offsets and
simple Unity geometry. Preset names never enter saved level data. Allow 3×3 where useful and
the listed L/T/S/Z variants; initial 4×4 limit is a configurable authoring guard, not a schema
limit, pending playtest/reference findings.

Exit tool flow: choose side, select a starting exposed edge cell, choose width and color, then
create/edit/delete. Highlight the whole proposed opening and report boundary, continuity,
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

1. Define the minimal shared authoring contracts and prove them with board resize, cell painting,
   footprint fit/rotation and selection/erase tests. Reuse existing grid and content types.
2. Adapt only DTM's shared operations to that core. Keep its hole/stickman UI, prefab previews,
   rotation behavior and JSON format. Manually verify its existing editor save/load/rotation and
   board visuals before and after migration.
3. Add CBE block and exit tools, save/load, then the isolated play-test bridge. Verify malformed
   import preserves the current session and invalid board edits never discard items.

No current framework editor code exists to “turn on.” The migration changes both framework and
DTM source and must follow the pinned-package update workflow: publish and verify the new
framework revision, update DTM's full-SHA dependency, compile/test DTM, and then commit/push
the consumer under the repository's Git workflow. CBE is a second consumer and follows the
same rule.
Approval of this design does not itself change any package pins or repository content outside
documentation; those changes occur during implementation and verification.

## Seven-day risk

Framework extraction, DTM regression migration, true selection/move, irregular-edge exit editing,
round-trip persistence, and play-test together are probably the largest schedule risk. The
minimum shared core above limits that risk, but the promised seven-day prototype cannot be
treated as guaranteed until an integrated editor and runtime path work in Unity. If extraction
starts consuming the whole budget, report the actual blocker and request a scope decision;
do not silently replace the required shared core with a CBE-only duplicate or omit play-test.
Reduce speculative extension points, unrelated features and polish before considering a change
to the agreed framework/game-module boundary.
