# Color Block Escape Runtime Technical Design

Date: 2026-09-20
Status: Approved by owner on 2026-09-20, including the six approval clarifications.
Requirements: [Color Block Escape MVP](ColorBlockEscapeMVPRequirements.md).
Baseline: [framework architecture](FrameworkArchitecture.md) and
[implementation preflight](ColorBlockJamArchitecturePreflight.md).

## Problem, assumptions and boundaries

The game needs continuous footprint movement and exits through a wall. The current framework
`DragMovementSystem` only checks a rounded destination, `GridSnapSystem` only checks one rounded
release candidate, and `CellOccupancySystem` has no owner IDs or atomic footprint transfer. Treating
these as a complete mover would permit tunneling or incorrect self-collision. Drop The Man has a
swept-footprint helper, a structural/occupancy clearance loop, and a per-cell
release/occupy/rollback sequence, but its coordinator also owns collectible/capacity rules.
Both games need those rule-free geometry and occupancy operations; they belong in
PuzzleFramework rather than being copied into CBE.

Assume one active pointer drag, fixed block orientation during gameplay, orthogonal grid cells,
static exits, and no special reference-game obstacles. The framework owns grid, shape, occupancy,
world layout, generic timer/state, and opaque level transport. Color Block Escape owns blocks,
exits, eligibility, lifecycle, presentation and payload interpretation. Framework code must not
contain block/exit nouns. No event bus, pathfinder, shared object factory, queue, or capacity system
is needed for this slice.

The seven-day planning benchmark does not move shared work into the game module. Implement the smallest
demonstrated shared slices in the framework, verify them against Drop The Man, publish/pin the
framework revision under the dependency workflow, then consume them in CBE. Leave raw
platform-specific input polling, view hit targets and game rules with the scene/game adapter.
Before changing package code, update the approved `DragMovementSystem`, `CellOccupancySystem`
and relevant `InputSystem` markdown specs with the narrow public contracts and relationship
metadata reviewed here. Approval of this design should set that direction; it does not make the
current package classes already capable of these operations.

## Data and construction

Use the existing `LevelDefinition` metadata, explicit Active/Inactive/Blocked board cells,
countdown timer, and opaque typed content payload. The CBE payload contains a version; a block
list of stable unique ID, `ColorIdentity`, integer origin and unique footprint offsets; and an
exit list of stable unique ID, side, start edge cell, positive integer width and color. Save the
resulting rotated offsets, not a preset or a second rotation field. The CBE parser validates
schema, IDs, enum values, duplicates, and supported version. The framework JSON service transports
the envelope without interpreting CBE content.

The proposed module profile requires at least one block and one exit, and each footprint must be
orthogonally connected so one authored object is one physical block. A block color with no
matching authored exit is an editor warning about likely unwinnable content, not a runtime
dead-state detector or a persistence error. These are CBE authoring rules, not new framework
`ShapeFootprint` constraints.

Construction first validates the shared board through `LevelRuntimeBuilder`, then CBE validates
each block fits Active cells without overlap and each exit identifies valid contiguous exposed
exterior edges. Only after all placement checks pass does the module populate occupancy and create
views. Failed construction discards its unpublished session. Restart/play-test builds a new session;
callbacks from an old session must not mutate it. Rendered obstacles and walls are derived from
authored structure and exit masks, never saved as world-space placements.

For an irregular outline, the start edge cell must be Active, the neighboring cell in the exit
direction must be Inactive or outside rectangular bounds, and every edge in the width run must have
the same orientation and line. To exclude openings into an enclosed hole, determine whether the
non-board side connects through Inactive/out-of-bounds space to the unbounded exterior. Blocked
cells are structural and are not automatically exit surfaces. Reject an
exit that crosses a corner, a discontinuous boundary, an incompatible existing opening, or a run
whose visual mask cannot be represented. This connectivity check is authoring/construction work,
not a per-frame movement query.

## Continuous movement and collision

The scene adapter samples pointer input and resolves a block view. A shared board-plane
projection/pointer-offset primitive should cover the same geometry currently embedded in Drop
The Man's pointer adapter; the module still owns camera/layer/UI configuration and game target
selection. Use the framework `InputSystem` for generic single-pointer intent/lifecycle where its
current contracts fit, adapting both consumers rather than inventing a second generic lifecycle.
The module drag session stores selected block ID, committed footprint cells, previous accepted
continuous pose, and current candidate pose. It
passes board-local poses plus occupied-cell offsets to a **rule-free swept-footprint query**.
Extract Drop The Man's `SweptFootprintHelper` geometry into PuzzleFramework with generic names
and no collection/reservation side effects. Compare its rectangle/contact ordering with both
games' tests before changing behavior; make Drop The Man use the extracted implementation. Do
not relocate the DTM movement coordinator or call the old destination-only
`DragMovementSystem` sweep-safe.

The framework should also expose a small rule-free clearance query over ordered swept contacts:
check bounds, structural presence, Blocked state and occupied/reserved cells, with the caller's
actual committed footprint excluded from self-collision. This is the shared part of Drop The Man's
`TryGetBlockingReason`; its subsequent stickman/color check remains in that game. Normal CBE
travel consumes the same clearance result, then adds closed-wall/exit policy in the CBE module.
Do not place gate exceptions inside the framework query. Reuse that structural check inside the
existing framework destination-only drag path where practical, so it does not diverge.

Each candidate travel interval is checked in contact order. On an invalid candidate, retain the
last accepted pose or clamp to the last valid point. Collision and exit eligibility use logical
footprint geometry, never colliders. An on-board release uses the existing snap result as a
candidate and verifies reachability and full footprint
clearance. Add a generic all-or-nothing footprint transfer operation to framework occupancy:
validate the source set is occupied and the destination set is unique, structural and free apart
from the source, then replace source-minus-destination with destination-minus-source as one
operation. The caller checks Blocked cells and other placement rules before transfer, preserving
the occupancy system's existing ownership boundary. Drop The Man's release service and CBE
should both call it. The caller still owns entity identity, snap policy, and rule eligibility;
the current package does not yet offer this atomic transfer or a nearest-free search.

### Geometry convention

Each footprint offset denotes one unit cell square in board-local space. The block's current
continuous pose translates the whole set. For Top/Bottom exits, projected span is
`max(offset.x) - min(offset.x) + 1`; for Left/Right it is the equivalent Y span. This is the
complete bounding projection, so internal gaps do not shrink the required opening. Board layout
provides world conversion; logic remains in board-local units and is independent of visual scale.

## Gate approach and capture

An exit is an outward portal on a set of derived boundary edges, not an Active cell outside the
board and not a hole cut into board occupancy. The CBE movement query treats all boundary edges as
solid except when the selected block is being evaluated against a valid matching, idle exit from
the playable side. A wrong-color or busy exit is solid.

Capture occurs only during a player-driven movement sample with an outward motion component and
while the block's leading edge is within a configurable capture distance of the opening. The
projected bounding interval must overlap the opening by at least a configurable fraction of the
block's projected span (initial test value `0.70`). Width must be at least the required span.
Before capture, calculate an aligned on-board **integer origin** whose full projected interval
fits inside the aperture and verify a collision-free path from the current pose to that origin.
A 70% candidate overlap must never let the remaining 30% pass through a jamb. The chosen origin
should minimize lateral shift; tie-breaking is deterministic. During captured alignment/entry,
logical occupancy must include every playable cell covered by the moving footprint. An irregular
footprint may acquire newly covered in-board cells while releasing vacated cells; use the
framework's collision-safe atomic footprint transfer, never overwriting another occupant.
Before accepting, validate the complete strictly outward corridor as well as alignment.
At the aligned pose, atomically transfer the selected block's committed occupancy to the
validated set of in-board cells its remaining outward path can cover. This may conservatively
reserve a cell ahead of the currently visible irregular footprint, so another block cannot move
into the corridor before the exiting block reaches it. It also prevents drag-start cells from
becoming ghost blockers. The module records the transition once, ends
pointer ownership, marks the exit busy, and begins a straight outward logical exit trajectory.
Visual alignment may ease toward that trajectory but cannot veto or duplicate acceptance.

Capture requires a preceding player-driven outward sample; initial proximity alone is inert.
The module does not evaluate exit acceptance on construction, animation updates, load, or passive
timer ticks. Outside-to-inside samples do not qualify.

## Exiting occupancy and concurrency

Use states such as `OnBoard -> Exiting -> Removed` for each block and `Idle -> Busy -> Idle`
for each exit. Each accepted block stores its exit ID, outward direction, accepted logical pose,
and a set of board cells still occupied. The exit remains Busy until the entire block has
finished leaving the playable board. A later chipper sequence may extend visual gate use, but
presentation cannot determine capture success or release gameplay occupancy. A block cannot be
captured twice or selected again after acceptance.

Advance the accepted block strictly outward along the logical exit trajectory. Its retained
occupancy is the set of in-board cells that the current footprint position or its remaining
outward path can still cover. It must include every playable cell required by the current
footprint position. From the aligned pose onward, this set is monotonic non-increasing: release
a cell only when no remaining part of the validated trajectory can cover it. Use board-local
geometry and a consistent epsilon at exact boundaries; never infer release from renderer bounds
or fragment position. Cells not yet released remain collision blockers for other blocks. Cells
already released may be used by other blocks, even while this exit is busy. Keep the block entity
until it is fully outside and its exit sequence finishes; then remove it and clear Busy. A
game-owned block-to-retained-cells map supplies identity missing from the framework occupancy
service. No new in-board cell may be acquired after strictly outward travel begins.

Prototype measurements may expose an expensive or fragile edge case here. Log that evidence and
return for a scope decision; do not silently free all cells at acceptance.

## Timer, result and presentation

The gameplay session orders accepted-exit and timer-expired facts. The final accepted block
locks a win immediately if acceptance occurs before or exactly at expiry; stop/freeze the timer
for that result.
Exit/chipper and result-screen delay then run as presentation. If expiry occurs before the
last required acceptance, lock a loss. At an exact simultaneous timestamp, process successful
final acceptance before timer loss, regardless of callback order; this approved tie rule must be
covered by a boundary test. A terminal result cannot be reversed by later callbacks. Restart
invalidates old-session animation callbacks.

Build the block and gate views from simple Unity geometry. Derive a gate opening mask from
validated exit edge runs. The existing modular board planner has no aperture input, so the first
visual adapter may suppress/replace game-owned wall pieces around an opening; do not change shared
wall topology to embed gate meaning. Keep logic independent of whether a custom FBX is available.

The chipper consumes an accepted-exit presentation request. It may subdivide each footprint cell
into pooled fragments, stagger movement and scatter/rotate them with DOTween. The existing CBE
Unity project already contains DOTween; no new package install is part of this design. Each pool
checkout resets transform/material/visibility, and each return kills or stops prior tweens and
clears callbacks. Configuration includes every parameter listed in the requirements document.
Pool exhaustion should degrade presentation (skip/reduce fragments), never block gameplay exit.
Result UI delay and post-result fragment visibility are presentation policy. A missing/failed
animation cannot prevent a locked win or permanently retain gameplay occupancy.

## Verification gates and delivery risk

Focused pure-logic tests should cover projected span for rectangles and irregular footprints;
wider-than-needed openings; approach direction and 70% trigger versus full-clearance target;
wrong color, busy exit and start-adjacent inertia; swept collision against a one-cell obstacle;
collision-safe corridor reservation for irregular shapes, monotonic post-alignment release with
another block using a cleared cell; timer-before-acceptance and
acceptance-before-timer cases; duplicate callbacks; invalid exit boundary/overlap data; and
save/load round trips. A Unity playtest must confirm continuous feel, irregular board visuals,
gate openings, pooled tween reuse, and result timing. Static tests alone cannot prove input feel.

The highest technical risks are shared sweep/occupancy integration, capture alignment without
wall penetration, progressive release while other blocks move, and wall-visual openings. Deliver
a plain-geometry vertical slice before chipper polish. Implement shared sweep, structural
clearance, projection and occupancy primitives in PuzzleFramework, while leaving CBE exit rules
in its module. If the seven-day benchmark is exceeded, report concrete causes and options rather
than weakening the owner rules or duplicating shared infrastructure locally.
