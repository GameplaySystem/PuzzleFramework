# Color Block Jam Architecture Preflight And Prototype Transition

Date: 2026-09-17.

Status: owner-authorized project transition recorded; Color Block Jam architecture and MVP scope
below are **proposals pending requirements approval**, not approved implementation specs.

## Decision And Daily Milestone

Freeze Drop The Man at **DEVELOPMENT-COMPLETE FOR CURRENT MVP SCOPE + PRESENTATION/ASSET
INTEGRATION DEFERRED**. Make Color Block Jam the active second prototype in architecture/design.

Today's milestone, supplied by the owner's transition request: inspect the existing framework,
record the freeze without losing deferred work, research the reference rules, inspect the supplied
asset, and deliver a reviewable preflight. Project area: cross-prototype architecture/documentation.
Secondary goal: identify concrete reuse friction. Scope: medium; design uncertainty is medium/high,
repository-change risk low. Sequence: repository/docs audit -> reference/asset evidence -> proposed
boundaries -> documentation transition -> consistency review. Done means this report and project
state agree, uncertainty is explicit, and no gameplay code, Unity content, package changes, git
publication, or new project setup has occurred. No Unity check is required for the documentation
change; reference play observation and later Unity asset inspection remain requirements inputs.
This is not an end-of-day report; work time and usage were not tracked.

## 1. Documentation And Source Preflight

Documents inspected in `PuzzleFramework` (system examples are design context, not reference-game proof):

- `AGENTS.md`; `docs/PROJECT_STATE.md`; `docs/IMPLEMENTATION_WATCHLIST.md`;
  `docs/CRITICAL_RULE_CLARIFICATIONS.md`; `docs/DropTheManRemainingWork.md`; `README.md`.
- `docs/FrameworkArchitecture.md`; `docs/ImplementationRoadmap/FrameworkMVPPlan.md`;
  Color Block Jam phase/reuse sections of `docs/ImplementationRoadmap.md` and `docs/ProjectStructure.md`.
- `docs/Workflow/DailyMilestoneWorkflow.md`; `docs/Workflow/FrameworkPackageDependencyWorkflow.md`.
- `docs/FrameworkSystems/CoreBoardSystems/`: `GridSystem.md`, `CellOccupancySystem.md`,
  `ShapeSystem.md`, `WallGenerationSystem.md`; ownership/MVP sections of `PathFindingSystem.md`.
- `docs/FrameworkSystems/InteractionSystems/`: `InputSystem.md`, `DragMovementSystem.md`,
  `GridSnapSystem.md`.
- `docs/FrameworkSystems/ContentSystems/`: `LevelDataSystem.md`, `LevelSaveLoadSystem.md`,
  `LevelEditorFoundation.md`, `LevelCatalogSystem.md`.
- `docs/FrameworkSystems/RuntimeConstructionSystems/`: `LevelRuntimeBuilderSystem.md`,
  `RuntimeConstructionValidationSystem.md`, `RuntimeObjectFactorySystem.md`.
- Metadata, purpose, ownership, exclusions and MVP sections of `GameStateSystem.md`, `TimerSystem.md`,
  `EventSystem.md` under Runtime Flow; `QueueSystem.md`, `BufferSystem.md`, `CapacitySystem.md` under
  Resource Processing; `ColorSystem.md`, `VisualFeedbackSystem.md`, `ModularBoardVisualSystem.md`
  under Presentation (including its profile/input sections); `PlayerProgressDataSystem.md` and
  `ProgressSaveLoadSystem.md` under Progression, including approved first-implementation sections.

In sibling `DropAwayPrototype`, inspected `docs/DropTheManBoardVisualConstructionDesign.md` and
`docs/DropTheManProgressionImplementationHandoff.md`, plus relevant purpose/content/JSON/input
ownership sections of `DropTheManLevelEditorDesign.md`, `DropTheManRuntimeIntegrationDesign.md`
and `DropTheManPlayableSceneAdapterDesign.md`. These explain reusable boundaries; they do not
define Color Block Jam gameplay. No Drop The Man behavior change was undertaken.

Source audit: package runtime/test inventory; concrete drag, drag request, snap, footprint,
occupancy, input, timer, game state, level DTOs/schema validation, board validator/builder and
modular visual planner; prototype swept-helper algorithm and related source inventory. This is
a scoped static audit, not a full code review or fresh runtime verification.

Rules affecting this task:

- GameModules -> Framework only; puzzle nouns, gate rules, content interpretation and concrete art
  stay in the new prototype. Two games needing something similar does not alone prove one contract.
- Persistence validates schema; construction validates build safety; gameplay owns move/exit/outcome
  decisions. Framework payload transport remains opaque. Invalid duplicate IDs/coordinates/keys
  must be rejected by their owning layer.
- Shared board cells remain structural; blocked does not imply absent geometry. Framework shapes
  preserve authored orientation; gameplay rotation is not currently approved.
- Direct calls/results are sufficient for a single owner. No speculative event bus, generic factory
  registry, shared editor extraction, or resource-processing subsystem.
- A separate consumer must eventually use a verified, pushed, full-SHA package dependency. No
  dependency setup or remote git operation is part of this preflight.

Drift/gaps found:

- Old current-focus/next-step text kept Drop The Man finishing work ahead of Color Block Jam. The
  explicit owner transition supersedes that prioritization; unchecked tests remain unchecked.
- Color Block Jam was marked deconstructed/done, but no detailed game-owned rule spec was found in
  the inspected repositories. Shared examples cannot settle its mechanics.
- Shared drag docs describe more than the implemented destination-only query. README wording about
  swept movement previously overstated package implementation; corrected in this task.
- Snap documentation describes nearest-valid alignment; current code validates only the rounded
  nearest candidate. There is no alternate-candidate or reachability search.
- Conceptual shared shape placements and factory coordination are not implemented level fields or
  factory infrastructure. Current shared construction builds a board/context only.
- CountUp exists in authored data but not the timer runtime. Generic JSON validation checks a
  positive metadata version, not a supported-version migration policy. The new module must enforce
  its supported content/rule profile before startup without teaching framework storage game meaning.
- Existing ShapeSystem's editor-only rotation decision is the present project constraint, not proof
  that the reference game never rotates. Contrary evidence requires a documented decision first.

## 2. Repository Snapshot

| Repository | Branch / HEAD at inspection | Initial status |
| --- | --- | --- |
| PuzzleFramework | `main` / `d4600b10af26f1529e4951f375a1e1a5041e6203` | Clean |
| DropAwayPrototype | `main` / `dbbeff4e9473580052902c639a529d746421e0ea` | Clean |
| ColorBlockJamPrototype | No directory in the inspected Projects folder | Not created |

DropAwayPrototype's manifest pins framework
`96e9b7751686f2652c0374a40841e74c96c74c9f` through the Git package subfolder URL.
No remote fetch/check was performed; publication evidence here is the existing handoff, not a new
remote verification. Documentation edits leave branch, HEAD, package pins and runtime code unchanged.

## 3. Drop The Man Final Development State

The owner confirms the current MVP milestone: play-mode arbitrary-level authoring; hole,
cat/collectible/stickman and blocked-cell placement; color selection; JSON export/import and
edit/save/load round-trip; JSON gameplay loading and runtime view spawning; drag and shape-aware
clearance inset; matching/blocking, collection, capacity and hole completion; win/fail, timer,
restart, next-level loading and sequential flow; basic temporary result UI and documented development.

The closed loop is level load -> play -> win/fail -> next/restart -> load again. Existing docs also
record local progression/replay/loop delivery. This is a scope-completion decision supported by
the owner's report and historical handoffs, not a claim that all device/visual acceptance passed today.
Keep the prototype as proof of one complete loop, a shared-system regression target and a future
presentation target. Do not abandon, rewrite or keep adding features.

## 4. Deferred Drop The Man Work

Retain the detailed [deferred backlog](DropTheManRemainingWork.md): final gameplay/result/selection
UI; remaining artist assets and all-shape presentation acceptance; closing/cavity visual policy;
camera/aspect checks; sandbox replay/save/retry UI acceptance; full level-sequence/demo acceptance;
device performance, input and filesystem checks. External UI/art support is currently unavailable.
These are not prerequisites for Color Block Jam. Reopen only for a critical bug, framework
regression, or an explicit return to final-asset presentation integration.

Board topology and the initial modular visual implementation already exist and were owner-validated.
Do not regress them to "design only." Present-to-absent orthogonal adjacency yields a straight wall;
one present vertex quadrant is convex, three concave, two adjacent straight, two diagonal diagnostic.
Framework uses Convex/Concave; art can call these Outer/Inner Corner. Only Drop The Man maps blocked
cells to absent visuals. Further asset/presentation work is deferred; diagonal-only contact remains
unsupported by the current modular profile.

The framework catalog test assertion issue is a separate known validation caveat, not evidence of
a newly found gameplay blocker. It must not be hidden when future framework suites are reported.

## 5. Reference Evidence And Preliminary Gameplay Decomposition

Reference identified as Rollic Games' Color Block Jam, not similarly named store/browser games.
Primary sources consulted on 2026-09-17:

- Publisher help describes sending blocks through matching-color doors, clearing all blocks before
  time expires, and failure on timeout or absence of valid matching doors. It does not precisely
  define that second failure predicate. [Rollic: How Do I Play?](https://rollic.helpshift.com/hc/en/24-color-block-jam/faq/1222-how-do-i-play-color-block-jam/)
- The publisher's store description supports player-directed sliding and obstacle avoidance, but
  "freely" is not a specification for diagonal motion, swept collision or snap thresholds.
  [Rollic App Store listing](https://apps.apple.com/us/app/color-block-jam/id6504332779)
- The reference includes special mechanics such as directional blocks, layered/locked/frozen blocks,
  changing doors and color-restricted paths. These are separate scope candidates, not automatic MVP
  requirements. [Rollic: Obstacles](https://rollic.helpshift.com/hc/en/24-color-block-jam/faq/1234-obstacles-in-color-block-jam/)

Decomposition proposed for approval: board topology; movable shape instances; player-controlled
translation; collision/enterability; gate matching/admission; block exit lifecycle; timer and terminal
outcomes; presentation; authored content/editor. Treat resources/queues as a question, not a default
subsystem. This task did not observe a reference-game play session or measure controls.

Smallest proposed MVP: static board, ordinary colored footprint blocks, static matching exits,
single-pointer movement, countdown, clear-board victory, restart and minimal results. No special
mechanics, boosters/economy, queue/buffer, persistent campaign or gameplay rotation in this proposal.
Timeout-only failure would be an intentional simplification until the other reference failure rule
is understood and explicitly accepted; it must not be presented as exact reference fidelity.

## 6. Framework Reuse Matrix

Classifications apply to that proposed basic slice. DIRECT REUSE means an existing usable contract,
not a complete game subsystem. REUSE WITH EXTENSION identifies a gap to prove and approve, not code
authorized now. GAME-MODULE IMPLEMENTATION includes prototype-owned adapters and policies.
NOT NEEDED means no current demonstrated requirement, not that the reference never uses it.

| System / concern | Classification | Evidence, applicability and limit |
| --- | --- | --- |
| Grid System | DIRECT REUSE | `GridBoard`/coordinates provide cell structure and board queries. No gate semantics. |
| Cell concepts | DIRECT REUSE | `GridCell`, authored Active/Inactive/Blocked and builder conversion exist; no separate CellSystem is needed. Module defines rendered obstacles versus absent cells. |
| Shape / footprint | DIRECT REUSE | `ShapeFootprint` accepts nonempty unique offsets and resolves them at an origin. No fixed catalog, connectivity enforcement or runtime rotation service. |
| Occupancy | DIRECT REUSE | Occupied/reserved coordinate sets, footprint availability and per-cell mutation exist. No owner IDs or atomic move transaction; module owns the selected block and safe commit. Occupancy alone does not reject Blocked cells. |
| Wall/boundary topology | DIRECT REUSE | Explicit mask -> edges/Convex/Concave/diagonal diagnostic. Gate eligibility is not a wall-generation rule. |
| Coordinates / world layout | DIRECT REUSE | `GridWorldLayout` provides common conversion/axes/rectangular centering. Use one instance across movement, visuals and editor. |
| Input intent contracts | DIRECT REUSE | Single target, capabilities, resolver and UI-blocker contracts exist. |
| Platform input/hit testing | GAME-MODULE IMPLEMENTATION | Mouse/touch polling, camera raycasts, board-plane conversion, pointer offset and scene gating are concrete adapters; not supplied by the package InputSystem. |
| Continuous drag / collision sweep | REUSE WITH EXTENSION | Package drag rounds and validates destination only. Swept helper is internal to DropAwayPrototype. If reference needs continuous motion, approve a narrow geometry-only extraction/extension; module supplies gate/obstacle decisions. Discrete motion still needs intermediate traversal checks. |
| Grid snap | DIRECT REUSE | Rounded footprint validation/result works for on-board release. Caller owns reachable fallback and occupancy; do not assume nearest-free search or exit handling. |
| Axis locks / diagonal policy | GAME-MODULE IMPLEMENTATION | Decide permitted motion from approved rules; a generic axis primitive is only a later extension candidate if actually reused. |
| Game State | DIRECT REUSE | Existing lifecycle transitions and results; no puzzle win predicate. |
| Timer | DIRECT REUSE | Countdown start/pause/resume/reset/advance and warning/expiry results. No CountUp or add-time API; add-time requires a separate approved extension if scoped. |
| Terminal outcomes/restart/next | GAME-MODULE IMPLEMENTATION | New game decides win/loss precedence, exit completion, input cutoff, teardown and level selection. No Drop The Man completion/capacity reuse. |
| Level Data | DIRECT REUSE | Existing metadata + shared Board/Timer + opaque typed payload. There is no implemented shared ShapePlacements field. |
| Level Save/Load | DIRECT REUSE | `JsonLevelSaveLoadService` transports the shared envelope; it does not understand block/gate payloads. |
| JSON payload adapter | GAME-MODULE IMPLEMENTATION | New module owns payload parsing/version checks and schema. Prefer one canonical envelope; a readable alternative format needs an explicit adapter decision. |
| Level Editor Foundation | GAME-MODULE IMPLEMENTATION | Approved framework design, no implemented shared editor. Build a dedicated module tool using existing data/geometry services; compare two editors before extracting. |
| Level catalog | DIRECT REUSE | Existing opaque Resources discovery and deterministic ordering, once multiple levels are needed; new module supplies metadata reader and resource path. |
| Runtime board builder/context | DIRECT REUSE | `LevelRuntimeBuilder` returns shared grid/occupancy context only. |
| Shared construction validation | DIRECT REUSE | Existing validator checks board build safety, not payload placements or gate references. |
| Block/gate runtime model build | GAME-MODULE IMPLEMENTATION | Interpret module data, validate placements/references, initialize occupancy and runtime entities. |
| Runtime Object Factory / view binding | GAME-MODULE IMPLEMENTATION | Shared factory is documented only. One narrow module factory and explicit instance registry suffice; no framework registry needed. |
| Color Identity / mapping | DIRECT REUSE | Ten stable shared slots and visual mapping; module owns equality/eligibility rules and materials. |
| Visual feedback | GAME-MODULE IMPLEMENTATION | Selection, rejected movement, exit and results use direct view calls. General shared feedback remains documented only. |
| Modular board visuals | REUSE WITH EXTENSION | Existing fixed half-wall/corner profile has no gate-aperture input and rejects diagonal-only contact. First use a module presentation adapter for openings or a simpler renderer; only add generic edge suppression if a second actual consumer needs it. Asset compatibility remains unverified. |
| Gate behavior / exit processing | GAME-MODULE IMPLEMENTATION | Color, aperture, admission, removal and presentation coordination are this game's meaning. |
| Queue | NOT NEEDED | No confirmed ordered waiting in basic slice; framework queue is documented only. A block physically waiting near an exit does not imply FIFO. |
| Buffer | NOT NEEDED | No confirmed temporary storage slots; documented only. |
| Capacity | NOT NEEDED | Gate width is geometry, not a count limit. No confirmed usage quota; documented only. |
| Event System | NOT NEEDED | No implemented bus or demonstrated independent listener fan-out; direct calls/results now. |
| Pathfinding | NOT NEEDED | Player-directed sliding needs collision validation, not automatic routes or a solver. Documented only. |
| Player Progress Data | NOT NEEDED | Existing completed/resume snapshot is available later, but unnecessary for first playable proof. |
| Progress Save/Load | NOT NEEDED | Existing safe local snapshot persistence is available later; do not import Drop The Man replay/loop policy. |

## 7. Proposed Subsystem Architecture

Problem: prove a second game's footprint movement and exits without tying the framework to it.
Assumptions: orthogonal cell structure and footprint authority remain appropriate; exact motion and
exit rules are not settled. Risks: destination-only collision, pretending an exit is ordinary board
space, and extracting the first game's coordinator wholesale.

Simpler alternatives: adapt the current destination query for a cell-step prototype if reference
evidence supports that motion; otherwise add a focused swept-geometry query after approval. Keep
one module scene composition root and small data/rule owners, rather than a universal puzzle engine,
entity-component rewrite, factory plugin registry or generic gate/resource architecture.

Recommended ownership groups (responsibilities, not a commitment to one class/interface per row):

| Prototype-owned group | Owns | Uses |
| --- | --- | --- |
| Content adapter | Payload schema, parsing and authoring model | Shared level envelope/storage |
| Build coordinator + model builder | Validation ordering, fresh runtime model and occupancy setup | Shared board builder/validator |
| Block state + movement/session owner | Selected instance, committed anchor, accepted continuous pose, permitted movement, commit/cancel | Footprint/grid/occupancy, approved drag query, snap |
| Gate/exit rules | Match, aperture, direction, acceptance and exit/removal lifecycle | Structural queries and block state |
| Outcome/session owner | Timer interpretation, win/failure, terminal precedence, restart/next teardown | GameState, Timer, optional catalog |
| Scene/view adapter | Input sampling, view creation/binding, selection/exit feedback and UI | Explicit runtime references, shared colors/layout |
| Play-mode authoring tool | Edit content and preview it independently of runtime play | Same content codec and validated shape/layout services |

Framework exposes only reusable mechanics; the new prototype never depends on DropAwayPrototype.
Use explicit dependencies, no singleton/service locator or scene-search coordination. Add public
interfaces only at useful seams such as a view or input adapter, not for every small data holder.

## 8. Proposed Content / Level Data Responsibilities

Keep authored footprint -> runtime block -> derived visual as the authority chain. Do not serialize
a list of pack mesh names as the definition of available gameplay shapes.

| Layer | Proposed data |
| --- | --- |
| Existing shared metadata | Stable level ID, display name and format version |
| Existing shared board | Width/height, coordinate entries and Active/Inactive/Blocked state; decide one canonical explicit-cell encoding |
| Existing shared timer | Enabled/countdown duration/warning threshold, subject to module's supported MVP profile |
| Module payload header | Content type and supported payload version; reject unknown versions rather than silently losing data |
| Module blocks | Unique stable instance ID, origin coordinate, explicit footprint offsets in authored orientation, shared color identity |
| Module gates | Unique ID, color, board-relative aperture/direction definition; exact side/span versus edge-list encoding pending geometry evidence |
| Module special content | Only approved obstacle/special-mechanic fields; ordinary static blocked cells need no duplicate obstacle entities |
| Optional catalog metadata | Separate ordering from stable identity via the module reader; do not inherit canonical `Level N` naming automatically |

Prefer inline footprint offsets for the first slice over a required shared shape-library registry.
Editor presets are convenience inputs that expand to the same shape data. If rotation is approved
for authoring, store the resulting offsets; avoid two independent authorities (offsets plus a rotation
field) unless a later schema explicitly defines their relationship. Shape connectivity, allowable
size, origin convention and negative-offset handling need approval.

No prefabs/material references, world-space positions, drag poses, occupancy snapshots, animation
state, transient exit reservations or player progress in level JSON. No per-color capacity totals
borrowed from Drop The Man. Gate reachability/solvability is not a save/load check.

Validation ownership: shared storage validates envelope/cell collection integrity; module schema
checks block/gate IDs, offset duplicates, color/version fields and gate encoding; construction checks
active-cell fit, initial overlaps, valid gate-boundary references and required runtime inputs. Asset
binding checks are module presentation construction. Gameplay evaluates move/exit eligibility and
terminal predicates. Editor may present these reports by calling their owners.

## 9. Proposed Runtime Construction Flow

1. Select a level (direct test asset first; existing catalog when needed).
2. Load the shared JSON envelope; parse and validate the module payload and supported rule profile.
3. Run shared board build-safety validation through `LevelRuntimeBuilder`; receive a fresh disposable
   context. Validate all module placements and gate references before occupying anything.
4. Create block/gate model instances; populate initial footprints once. Keep ID-to-instance lookup
   module-owned. Validate the complete destination set before multi-cell mutation.
5. Validate presentation inputs, create views through the module factory, and bind IDs explicitly.
   Build board visuals using the module's visual-presence mapping and selected renderer.
6. Wire movement, gate rules, state/timer and direct view callbacks. Enable input and start the timer
   only after successful construction. Construction errors are load errors, not gameplay losses.
7. On any failure discard the unpublished context and all newly created views. Prefer disposal over
   a reusable generic rollback framework. Never hand gameplay half-built state.
8. Restart/next stops input/timer, invalidates pending callbacks, disposes the prior session, and
   builds a fresh one from authored content. Old callbacks must not remove new-session blocks.

Builder sequencing does not move payload interpretation into framework construction. View spawning
does not move into persistence or the shared level editor. This flow proposes no factory abstraction
that the current package does not already contain.

## 10. Proposed Interaction / Movement Split

Scene adapter: pointer input, UI blocking, selection hit tests, pointer-to-anchor offset and conversion
to the common board plane. Framework input contracts: single-selection intent delivery. Module
session: one selected block, committed origin and accepted preview pose. Shared geometry: shape,
grid, occupancy queries and approved movement/snap mechanics. Module rules: gate admission and any
special restrictions. Views: render accepted poses and feedback; colliders are not gameplay truth.

Normal proposed movement validates the whole translated footprint and its travel between accepted
poses. Other blocks obstruct regardless of color under the proposed basic collision model; matching
is evaluated at gates. This is a proposal to verify, not a rule imported from Drop The Man. A same-color
block must not become a collectible or enterable target simply because Drop The Man permits that.

Do not feed a moving preview anchor into self-occupancy exclusions while committed cells still refer
to the drag-start anchor. Current shared occupancy stores coordinates without owners. A single-pointer
module can track its selected block's original occupied set; multi-touch would require a separate
ownership design. Validate release destination before per-cell release/occupy and restore on failure;
do not advertise atomic footprint moves as an existing framework API.

Gate boundary problem: shared on-board drag rejects outside cells. Do not solve this by marking arbitrary
outside cells valid, globally disabling bounds checks, or cutting gates into structural board holes.
Verify whether admission occurs while the block is still on-board, during partial traversal or after
full traversal. If admission can commit on-board, a module-owned visual departure is the smallest
solution. If partial traversal is gameplay, the module must validate the approved corridor/direction
and remaining in-board footprint; shared geometry can be reused without interpreting gate meaning.

Decide the removal/occupancy-release point separately from animation completion. Do not inherit
Drop The Man's reservation/capacity/callback-gated win. After evidence, define exactly-once removal,
release-during-exit behavior, input termination, final-exit versus timeout precedence, and whether
presentation may delay the result screen. Reachable release fallback must never snap through a wall
just because another cell is free. Swept clearance is a rule/interaction parameter, not visual scale.

## 11. Proposed Dedicated Play-Mode Level Editor

Author -> save JSON -> load JSON -> edit -> save -> gameplay consumes the same content.

Proposed tools: metadata and board size; explicit active/inactive/blocked editing if approved; shape
preset/custom footprint selection; place/select/move/remove/recolor blocks; editor-only rotation if
approved; place/edit/remove gates with color and aperture/direction; countdown settings. Add special
mechanic authoring only with its approved rules. Board resize must report stranded content rather
than silently deleting it. Preview and export must use the same coordinate/footprint convention.

Load into a temporary authored model, validate, then replace the editing session only on success.
Invalid import must preserve the current level. Save/load round-trip preserves IDs, shapes, colors,
gates, timer and cell states; compare data semantically, not JSON byte ordering. Unsupported payload
versions fail explicitly rather than dropping fields. Show actionable validation/import/export errors.

Use the same module visual builder/config for preview and gameplay appearance, while preview edits
never mutate gameplay occupancy, run timers or save progression. A Play/Test bridge, undo/redo and
shared tool registration infrastructure are separate later decisions. Reusing existing service
contracts is valuable now; extracting the full Drop The Man editor is premature.

## 12. Asset Inspection And Proposed Configuration

Owner supplied `tetra_pack.fbx` outside the repository during this task. Inspected read-only by
parsing FBX 7400 objects, connections, transforms and polygon data, then viewing a diagnostic X/Z
mesh projection. File: 3,202,540 bytes; SHA-256:
`94ca882e31fc662931c88c47470af423bf4d7e5bf0459fe36d2d29544d218bc6`.
No import, extraction, prefab creation or source-asset modification was performed.

Observed:

- 36 geometry objects and 36 mesh model nodes; six connected material definitions; no Texture or
  Video objects. Eighteen meshes have color materials and eighteen have no connected material.
- Local geometry shows four-cell bars, squares, L, zigzag and T silhouettes with repeated variants;
  it does not supply an arbitrary-polyomino catalog or a standalone single-cell model.
- Every unmaterialed mesh has four disconnected 216-vertex cube components. Colored meshes contain
  23-26 disconnected 96-vertex components, including decorative geometry; they are not four clean
  ready-to-instance unit objects. Extracting a base cube is possible in principle, but reconstructing
  the complete decorated look needs deliberate asset preparation.
- Mesh model transforms have scale 100 and varied rotations/translations; some local pivots are
  offset. Local base-cube dimensions are about 1.085 per side, not an authored one-cell contract.
  Normalize assets under a logical root; do not infer board cells from imported transform values.
- Polygon fan-triangulation count is 111,656 across the complete file, including repeated variants.
  This is an inspection estimate, not Unity's final imported triangle count or a device performance
  measurement. Do not instantiate the entire display pack as one block.

The selected colorful toy-like 3D style remains appropriate as the owner's visual direction. This
file's actual shape breadth is narrower than the initial asset description; record geometry facts
without assuming the missing shapes will be supplied later.

| Representation | Tradeoff after inspection |
| --- | --- |
| Complete supplied shape prefabs | Quick route to supplied appearance after transform/material cleanup, but limited shapes, duplicate orientations and irregular pivots require mappings. Cannot be the gameplay shape catalog. |
| Footprint-composed unit visual | Supports arbitrary authored shapes with one visual contract. Requires preparation of a reusable unit and checks for seams/decorative edges; no standalone unit is currently delivered. |
| Hybrid overrides | Allows special appearance on selected shapes but adds matching/fallback cases. Add only if composition fails an actual art requirement. |

Recommendation: **unit composition as the primary proposed representation**, using a deliberately
prepared unit visual; approve that preparation separately. Validate a bar, square, L and a shape absent
from this file in Unity before finalizing the config. Whole-shape prefabs can be a bounded initial
art experiment; a hybrid is a fallback based on fidelity evidence, not infrastructure to build now.

Prototype-owned visual config responsibilities after this inspection: unit visual/prefab; shared
color-slot -> material palette; floor/cell visual; optional boundary and gate visuals; only approved
obstacle visuals; normalized scale, pivot offsets and presentation tuning. If needed later, a small
explicit footprint-to-visual override list. Exact serialized ScriptableObject schema remains subject
to Unity inspection and design approval. No assembly-wide registry or framework asset references.

Keep gate rules, collision tolerance, time limits, quotas, required-block logic and exit acceptance
thresholds out of visual config. Keep mesh/rendering configuration, animation duration and visual
offsets out of gameplay JSON. Hit targets represent the intended footprint; visible seams do not
create navigable gaps. Colors do not inherit gameplay identity from imported material names.

Remaining asset evidence: Unity normals/shading/material behavior; intended layering of colored and
unmaterialed geometry; pivot normalization; composed-unit fidelity; selection coverage; actual gate
and board assets. Pack source/license was not supplied with the FBX and was not established here;
record provenance when assets are prepared for portfolio distribution. No dependency was installed.

## 13. Rule Decisions Still Required

| Question | Current evidence / decision needed | Observation that would resolve it |
| --- | --- | --- |
| What is a block? Arbitrary polyomino? | Shape variety supported by publisher; arbitrary connected offsets are a proposed content capability, not verified reference breadth. | Record ordinary shapes and decide allowed size/connectivity/origin. |
| Translation or rotation? | Project currently permits editor-only rotation; reference behavior unverified. | Try gameplay rotation and distinguish it from arrow restrictions. |
| Continuous, stepped or axis-constrained drag? | Publisher describes sliding; subcell/diagonal and corner behavior unresolved. | Slow drag, diagonal drag, corridor and corner tests, with platform/version noted. |
| Collision/pass-by behavior? | Obstacles block movement; footprint collision/contact tolerance not specified. | Two blocks side by side, same-color contact, narrow passage, fast pointer sweep. |
| Gate geometry/direction? | Matching-color doors confirmed; edge span and approach constraints unknown. | Wider/narrower blocks, off-center approach, corner exit, wrong color/direction. |
| When is exit accepted/removal committed? | No precise threshold in consulted sources. | Partial insertion, reversing/releasing during entry, occupancy after entry. |
| Gate queue, buffer or count capacity? | Not established for ordinary exits. | Send consecutive blocks during an exit animation; inspect any authored counter/waiting slots. |
| Exact win predicate? | Clear blocks is confirmed at high level; which special/non-removable blocks count is outside proposed MVP. | Final ordinary exit; special entities only if included. |
| Exact failure predicate? | Timeout and absence of valid matching doors described; no operational definition of latter. | Distinguish temporary blocked route, no current move, incompatible colors and exhausted door sequence. Do not substitute an A* or solvability test. |
| Timer/start/pause/precedence? | Timer confirmed; exact start, menu/background behavior and same-tick final-exit precedence unresolved. | Observe initial input, pause/background, final exit near zero. |
| Obstacles/special mechanics? | Reference has many; no owner-selected subset yet. | Choose explicit include/exclude list, then capture each chosen rule separately. |
| Starting placement? | Proposed non-overlapping on-board instances. Start-at-exit behavior and automatic admission unresolved. | Load a block touching a matching exit without dragging. |
| What must JSON express? | Candidate responsibilities above; gate encoding and special fields depend on rules. | Approve example levels covering ordinary shape, cutout, gate and timer cases. |

Capture app publisher/version/platform, level or scenario, action, observation and confidence for
each reference experiment. Publisher descriptions establish direction; they cannot settle frame-level
semantics. Approve intentional simplifications explicitly instead of labeling guesses as facts.

## 14. Documentation Changes And Follow-On Specs

Created this report. Updated `docs/PROJECT_STATE.md`, `docs/DropTheManRemainingWork.md`,
`docs/IMPLEMENTATION_WATCHLIST.md`, `docs/CRITICAL_RULE_CLARIFICATIONS.md`,
`docs/ImplementationRoadmap.md`, `docs/ImplementationRoadmap/FrameworkMVPPlan.md`, and `README.md`
to record the transition, preserve deferred work, and correct the package swept-movement claim.
No framework system specification was silently expanded.

After review, create the following **in the new prototype's docs once its location/setup is approved**:

- `ColorBlockJamMVPRequirements.md`: reference evidence, selected scope, explicit exclusions.
- `ColorBlockJamMovementAndExitRules.md`: collision, direction, aperture/admission/removal,
  occupancy, release and terminal precedence, with concrete examples.
- `ColorBlockJamLevelContentDesign.md`: canonical schema/version/validation and sample levels.
- `ColorBlockJamRuntimeIntegrationDesign.md`: model construction, scene composition, teardown,
  terminal state and view-binding boundaries.
- `ColorBlockJamLevelEditorDesign.md`: required operations and JSON round-trip acceptance.
- `ColorBlockJamAssetAndPresentationDesign.md`: final inspected unit/shape strategy, config, pivots,
  palette and gate/board visuals.

Create only specs needed for the next approved slice. If swept geometry or generic visual apertures
qualify for framework extension, amend the relevant system spec and its Depends On/Used By links
first, including the impact on Drop The Man. Do not create a shared DoorSystem.

## 15. Recommended Order After Requirements Approval

1. Resolve movement/exit/terminal rules and approve a few representative content examples. This is
   the next smallest task; do not start gameplay while those contracts are undecided.
2. Authorize separate prototype setup and immutable package consumption. Reuse a known published
   framework revision; no pin change merely to follow current main.
3. Implement module content codec/schema and construction checks with round-trip/invalid-input
   coverage; build pure block/gate runtime state on the existing board context.
4. Implement the approved movement slice. If a shared sweep extension is justified, document/test it,
   deliver framework first, then resolve/compile/test the new consumer. Exercise Drop The Man's
   affected behavior as regression coverage without reopening its polish backlog.
5. Implement gate admission/removal and occupancy release, then timer/terminal/restart flow; cover
   fast traversal, irregular footprint collisions, wrong gate, partial exit and final-exit timeout.
6. Wire a minimal playable scene with prepared unit visuals or explicit temporary placeholders and
   basic result UI. Verify one complete level and clean restart before broader presentation work.
7. Implement the dedicated play-mode editor incrementally against the same canonical content;
   finish save/load/edit round-trip and gameplay-consumption acceptance.
8. Add multi-level discovery/next flow as needed. Persistent progression and special mechanics need
   their own scope decision. Compare both prototypes before shared editor/factory/feedback extraction.

Each implementation task still requires its approved spec and normal maintainer handoff. This order
is a proposal; neither new projects/dependencies nor commits/pushes are authorized here.

## 16. Principal Architecture Risks

- **False direct reuse:** package drag has no sweep, editor/factory/resource/event designs have no
  shared implementation. Plan against code, not category names.
- **Exit versus board bounds:** globally loosening validity to make gates work breaks ordinary
  collision and other consumers. Keep traversal exceptions in module-owned exit rules.
- **Stale occupancy:** preview pose, committed footprint and departing blocks must not become three
  conflicting authorities. Specify state transitions before asynchronous presentation.
- **Overbuilding:** queue/capacity, runtime rotation, automatic deadlock solver, hybrid art registry
  and shared editor extraction are not justified by the basic slice.
- **Asset-driven rules:** imported names, transforms, decorative seams and incomplete shape coverage
  must not define gameplay geometry, color identity or allowed levels.
- **Boundary-mask drift:** Color Block Jam must define its own blocked/non-board visual mapping and
  gate apertures. Do not copy Drop The Man's stencil/hole geometry assumptions.
- **Partial construction and stale callbacks:** failed startup or restart must dispose session state
  coherently; choose a fresh-context approach before generic rollback machinery.
- **Misleading verification:** historic focused tests passed, but the full-suite catalog assertion
  caveat remains. Neither freeze nor this static audit certifies device readiness.

## 17. Verification And Stop Point

Documentation-only repository change. Source and supplied FBX inspected read-only; diagnostic mesh
projection/data written outside both repositories under the local Codex diagnostics folder.
No framework/gameplay code implemented, no Unity assets imported/edited, no package manifests or
settings changed, no project created, no branch created, and no commit/push/remote git operation.
Both worktrees began clean; DropAwayPrototype remains clean and untouched. Seven existing Markdown
files changed and this report is new/untracked. `git diff --check` passed; all local Markdown links
in those eight documents resolve; all 17 requested report sections are present. The FBX SHA-256
was rechecked and is unchanged. Unity compile/tests/playtests were not run for this task; historical
results remain labeled historical. Stop here for design review.
