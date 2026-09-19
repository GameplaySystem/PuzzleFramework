# Implementation Watchlist

## Active Prototype Transition (2026-09-17)

Status: owner-authorized. Drop The Man is development-complete for its current MVP scope; final
presentation/asset integration is deferred. Its load/play/win-or-fail/next-or-restart loop is closed.
Color Block Jam architecture/design is now active. See the
[preflight report](ColorBlockJamArchitecturePreflight.md).

Drop The Man's outstanding UI/art acceptance, camera, replay/save/retry, level/demo and device
checks below remain recorded but deferred. They are not blockers for beginning Color Block Jam
and are not marked verified by this transition. Return only for a critical bug, framework regression
affecting it, or an explicit presentation-integration task when final assets become available.
Historical notes below preserve delivered behavior and must not automatically reactivate old work.

### Color Block Jam Preflight Findings

- **Open: reference rules.** Publisher sources support matching exits, timer and block clearing;
  precise movement, gate aperture/direction, exit/removal timing and failure precedence still need
  observation and approved module rules. Do not import Drop The Man collection/capacity behavior.
- **Open: shared movement gap.** `DragMovementSystem.Evaluate` checks a rounded destination, not
  intervening travel. Continuous sweep/clearance support lives in DropAwayPrototype. Treat reuse
  as a potential narrow geometry extraction only after both games' requirements align. Do not
  copy the game-specific coordinator or call the current package sweep-safe.
- **Open: release placement.** `GridSnapSystem` checks only the rounded nearest candidate; it does
  not find a nearest reachable free alternative. Module session owns self-footprint exclusions,
  safe per-cell occupancy commit/fallback and exit-related removal. Occupancy has no owner IDs
  or atomic multi-cell move, and does not independently reject structurally Blocked cells.
- **Open: gate visuals.** Modular board planning has a fixed half-wall/corner profile, no gate
  aperture input, and rejects diagonal-only contact. Use module-owned gate presentation adaptation
  first; a generic edge-suppression extension needs demonstrated reuse and spec approval.
- **Inspected / preparation deferred: block assets.** Owner supplied Downloads `tetra_pack.fbx`.
  Read-only inspection found 36 mesh objects, six material definitions, repeated four-cell shapes,
  and no standalone unit model. Eighteen unmaterialed meshes contain four disconnected cube
  components each; colored meshes include decorative geometry. Proposed primary representation:
  prepared unit visual composed from authored footprints. Unity fidelity/pivots/material checks
  and final visual-config schema remain pending. No asset was imported or modified.
- **Not activated:** queue, buffer, capacity, pathfinding, event bus, persistent progression and
  shared editor/factory extraction have no demonstrated need in the proposed first slice.
- **Existing validation caveats retained:** countdown-only runtime versus CountUp schema; generic
  JSON positive-version check is not a compatibility/migration policy; catalog NUnit assertion
  failure remains open. The preflight did not run a new compile or test suite.

## Repository Cleanup And Level 3 (2026-09-04)

* Prototype `2dd7def` preserves scene/prefab tuning after removing nine redundant inactive root
  prefab staging objects from the gameplay scene. The large level-editor scene diff is intentional:
  its 5x5 generated preview now contains concrete modular board cells instead of square placeholders.
* Cat collection timing is now 0.5s approach + 1.0s fall. This is presentation tuning only; live
  socket following, randomized six-clip assignment, callback timing and gameplay ownership are unchanged.
* Prototype `a743278` adds `Level 3`. Its five four-cell holes match 20 cats by color, and an explicit
  structural audit found no duplicate IDs, out-of-bounds coordinates, or overlaps. Unity compilation
  and all 27 prototype Edit Mode tests passed after cleanup. A complete manual Level 3 playthrough,
  visual acceptance and target-device checks remain.

## Progression First Slice (2026-09-02)

* Owner approved completed-level replay and a configurable inclusive post-campaign loop range.
  Keep that policy prototype-owned and out of the version-1 save: completed stable IDs + resume ID.
* Unknown/deleted IDs remain history; new unfinished shipped levels take priority over loops.
  Existing `Level N` IDs must not be repurposed when reordering content.
* Invalid/unsupported/unreadable saves block progression startup without overwriting the file.
  Save failures retain dirty progress for explicit retry and pause/quit retry. Atomic replacement
  is validated on Windows; platform filesystem behavior still needs target-device verification.
* Editor Play Mode uses a separate sandbox save; inspector dev data/disabled editor progression
  bypass saving. Normal saves must never be modified by automated tests or authoring previews.
* All 38 progression tests passed in isolated Unity 6000.3.17f1 validation, including actual runtime
  terminal callbacks. September 3: framework `96e9b7751686f2652c0374a40841e74c96c74c9f` published and
  adopted with owner approval; prototype manifest/Unity lock agree. Normal-project compilation,
  all 27 prototype Edit Mode tests (including seven replay continuation cases) and gameplay startup
  with Editor sandbox HUD passed. Prototype `cee412bc27ff5956819cbfcd35dd165c54fd19bb` is pushed.
  Owner win/stop/reopen/replay UI acceptance and target-device checks remain. See
  [remaining work](DropTheManRemainingWork.md) for the finishing checklist.
* Replay Next chains completed catalog entries without changing saved progress. An unfinished
  successor resumes the earliest unfinished campaign entry; final replay returns to saved campaign/
  loop selection. A completed successor matching the loop cursor is still replay. Selection is
  read-only and requires accepted win; construction begins the next session. Resume Campaign and
  relaunch remain independent of replay. No schema change or saved replay cursor was introduced.
* Broader validation exposed an existing catalog-test assertion failure: `Has.Count.EqualTo(1)`
  targets an array in `Build_ReportsSequenceGapsWithoutRejectingCatalog`. Runtime progression tests
  pass. Fix that assertion separately; do not misreport the full existing suite as green.
* The isolated host enables New Input System, unlike the prototype. Its copied authoring controller
  needed legacy input defines to match the prototype because two preexisting new-input branches
  lack null-device return paths. This was validation-only, not a production input/settings change.
* Set loop bounds and the Editor progression toggle before Play Mode. Hot-reconfiguring the active
  profile/catalog is not supported; reopening the picker does not pause the level timer.

## Purpose

This document records implementation caveats, open questions, and design-to-code drift discovered during context checks.

It is not a replacement for:

* `docs/PROJECT_STATE.md`
* `docs/FrameworkSystems/**`
* prototype-owned rule documents

Its job is narrower:

* track risks that are not urgent enough to block work immediately
* record design gaps that should be revisited before later milestones
* prevent caveats from being lost across implementation handoffs

---

## Snapshot

Audit date:

* 2026-08-16

Repos reviewed:

* `PuzzleFramework`
* `DropAwayPrototype`

Compared against:

* `docs/PROJECT_STATE.md`
* `docs/FrameworkArchitecture.md`
* `docs/ImplementationRoadmap.md`
* `docs/ImplementationRoadmap/FrameworkMVPPlan.md`
* `docs/ProjectStructure.md`
* implemented framework MVP slices
* implemented `DropTheMan` prototype slices

Owner review incorporated:

* yes

---

## Confirmed Alignment

The following areas are currently aligned enough to continue:

* framework and prototype remain physically separated as approved
* `PuzzleFramework` remains game-agnostic
* game-specific nouns remain inside `DropAwayPrototype`
* framework content systems still transport opaque game payloads without interpreting puzzle meaning
* the current prototype rule docs explicitly keep collection meaning out of framework interaction systems
* the prototype uses a separate stickman coordinate index instead of forcing collectible targets into structural occupancy

---

## Active Watchlist

### 1. `PROJECT_STATE.md` status language needed correction

Status:

* Resolved 2026-08-16

Why it matters:

* `docs/PROJECT_STATE.md` previously marked multiple system categories as `[done]` even though only a subset had been implemented in code so far
* this can mislead future context checks, handoffs, and milestone decisions

Examples:

* `Shape System [done]`
* `Wall Generation System [done]`
* `Pathfinding System [done]`
* `Event System [done]`
* `Visual Feedback System [done]`
* `Progress Save Load System [done]`

Interpretation risk that existed:

* readers may assume those systems are implemented rather than merely documented or approved

Resolved decision:

* `docs/PROJECT_STATE.md` now distinguishes `implemented`, `implemented foundation`, `prototype-only`, and `documented only`
* future progress tracking should keep that explicit wording instead of collapsing everything into `[done]`

Recommended follow-up:

* keep status wording explicit whenever new slices are added
* avoid reintroducing category-level `[done]` claims that imply code exists when only documentation exists

---

### 2. Timer content schema is broader than current runtime support

Status:

* Open

Why it matters:

* framework content data supports `TimerMode.CountUp`
* current `TimerSystem` implementation only supports countdown behavior

Current state:

* authored schema can describe more than the runtime currently honors

Risk:

* future level content may serialize `CountUp` and appear supported even though no runtime path exists yet

Owner decision:

* future games may include time-boost mechanics
* that does not automatically justify `CountUp`
* adding time to a countdown is still countdown behavior, not count-up behavior

Current interpretation:

* keep MVP timer behavior countdown-only
* allow future countdown time-extension behavior without treating that as proof that `CountUp` is required
* revisit `CountUp` only if a real elapsed-time mode becomes necessary across actual games

Recommended follow-up:

* either constrain MVP-authored timer content to countdown only
* or add explicit validation that rejects unsupported timer modes before runtime integration

---

### 3. Event and feedback architecture is intentionally deferred but still needs a trigger point

Status:

* Monitor

Why it matters:

* the MVP plan defers broad `EventSystem` and broad `VisualFeedbackSystem` generalization
* the architecture docs still describe notification-driven reactions as the long-term model

Current state:

* current framework slices rely on direct calls and result-returning APIs
* no reusable event layer or feedback trigger layer exists yet

Risk:

* later UI, timer warning, collection feedback, and end-state feedback may push ad hoc fan-out logic into gameplay owners if the transition point is not chosen deliberately

Owner decision:

* continue with the current direction
* do not add a temporary prototype-only event bus

Recommended follow-up:

* continue with direct calls while ownership is singular
* introduce the approved framework `EventSystem` only when real multi-listener notification pressure appears

---

### 4. Prototype runtime model building mutates occupancy and has no rollback path

Status:

* Monitor

Why it matters:

* `DropTheManRuntimeModelBuilder` occupies starting hole coordinates inside the provided framework context during build
* this is acceptable for the current thin slice, but later build steps could introduce partial-failure cleanup needs

Current state:

* current builder validates first and has very few post-validation failure paths
* that keeps the risk low today

Future risk:

* if scene-object creation, prefab loading, or richer prototype startup failures are added later, failed builds could leave partially mutated runtime state

Owner decision:

* risk acknowledged
* proposed mitigation accepted

Recommended follow-up:

* before adding scene or prefab creation, decide whether runtime-model building is single-use and disposable
* if not, add rollback or staged-commit behavior at the prototype runtime orchestration layer

---

### 5. Prototype rules currently reject authored start-on-target states

Status:

* Accepted MVP Constraint

Why it matters:

* current rules and runtime model builder reject a level where a hole starts on a stickman coordinate
* this is a valid MVP simplification, but it also becomes an authored-content constraint

Current state:

* this is documented and intentional

Risk:

* if later level design wants immediate starting collections or overlap-based starts, current runtime assumptions will block that content shape

Owner decision:

* keep the current rule for the first playable slice
* no immediate change is needed because this start state has not appeared in the reference puzzle examples being used

Recommended follow-up:

* revisit only if real level design pressure appears
* if revisited, document the new start-state behavior before changing runtime code

---

### 6. Hole capacity is now a confirmed MVP prototype rule

Status:

* Accepted Decision

Why it matters:

* the first playable `Drop The Man` slice depends on holes filling up, closing, and disappearing
* old planning assumptions that deferred capacity for MVP are no longer safe

Resolved decision:

* prototype-level capacity behavior is required for `DropTheMan` MVP
* hole capacity is shape-based for MVP
* holes of the same color are interchangeable collectors for that color
* per-color authored collectible counts are expected to match provided hole capacity totals
* broad framework `CapacitySystem` generalization still remains deferred until later cross-game pressure proves it

Owner decision:

* accepted

Current interpretation:

* require prototype capacity behavior now
* keep reusable framework capacity generalization deferred for now

Recommended follow-up:

* treat `ShapeSystem` as required for the first playable `DropTheMan` slice
* keep hole-capacity meaning in prototype code until later reuse pressure is proven

---

### 7. Shared color identity breadth is now a confirmed authoring requirement

Status:

* Accepted Decision

Why it matters:

* the upcoming level-editor workflow needs stable shared color hotkeys and palette slots
* a four-color identity set is now too narrow even before the second prototype is implemented

Resolved decision:

* framework-owned color identities now use a stable ten-slot model
* legacy names such as `Red`, `Blue`, `Green`, and `Yellow` remain compatibility aliases for the first four slots
* color meaning still remains prototype-owned or game-module-owned
* the framework `ColorSystem` still maps identities to presentation only

Owner decision:

* accepted

Current interpretation:

* keep game-rule color matching inside the game module
* allow editor hotkeys `0-9` to map to stable shared framework identities without adding gameplay meaning to framework code

Recommended follow-up:

* reuse the ten shared slots across future prototype authoring tools
* revisit richer palette or accessibility behavior separately from gameplay color meaning

---

### 8. Wrong-color movement behavior is now settled

Status:

* Accepted Decision

Why it matters:

* wrong-color behavior sits at the boundary between interaction flow and prototype-owned gameplay meaning
* if this rule is described incorrectly, later prototype rule coordination will drift across drag, snap, and collection ownership

Resolved decision:

* a wrong-color stickman cell is not enterable for that hole
* during drag, movement should stop at the last valid position instead of allowing the hole to overlap the wrong-color cell
* matching collection happens during drag when the hole enters or overlaps a same-color target cell
* snap remains a release-time alignment concern rather than the owner of collection timing
* gameplay truth should come from prototype-owned rule or query logic, not from physics-authoritative collider resolution

Owner decision:

* accepted
* later interpolation, smoothing, or collider-assisted feel may still be used for presentation, but not as the gameplay authority

Recommended follow-up:

* keep prototype rule docs aligned with entry-blocking behavior
* keep drag-time collection and drag-time enterability in prototype-owned rule coordination rather than moving color meaning into framework drag or snap systems

---

### 9. Drop The Man editor and gameplay runtime remain intentionally separated

Status:

* Accepted Decision

Why it matters:

* editor work can still blur into gameplay runtime or a premature framework editor extraction if the scene boundaries become fuzzy
* runtime-spawning and result-loop work needed to stay prototype-owned and avoid turning into either a framework spawner or an editor one-click test bridge

Resolved decision:

* editor phase 1 covers design, shared color-slot expansion, blocked-cell authored data, and editor config foundation only
* editor phase 2 is now a dedicated play-mode authoring scene foundation rather than selected-object SceneView tooling as the main UX
* editor phase 2 may add blocked-cell painting, narrow stickman/hole placement, runtime input, and lightweight runtime HUD feedback on top of that authored-data path
* editor phase 3A may add save/export from the play-mode scene by reusing the existing Drop The Man JSON schema and validation path, while still deferring load/import UI and gameplay play/test bridging
* editor phase 3B may add import/load back into the authoring scene without becoming gameplay runtime
* obstacle mode currently means blocked-cell painting, not separate obstacle entities
* inactive-cell authoring remains deferred for now
* gameplay play/test behavior remains deferred from the editor workflow
* JSON-driven runtime spawning, drag/collision feel tuning, and the basic result loop are now implemented as separate gameplay-scene slices
* the gameplay test scene remains a separate dev-only runtime path from the authoring scene

Owner decision:

* accepted

Recommended follow-up:

* keep editor authoring and gameplay runtime scenes separate unless a new approved design intentionally bridges them
* keep gameplay play/test bridging and broader runtime asset/prefab pipeline decisions as separate documented slices
* defer framework-level editor extraction until a second prototype proves the shared kernel

---

### 10. Reusable modular board generation is implemented and manually validated

Status:

* Implemented / Owner-Validated In Unity

Why it matters:

* the editor config and gameplay scene now reference the concrete modular cell prefab
* the approved piece profile uses two `0.5` half-walls per exposed edge, a `0.145` convex pillar
  that caps intersecting half-walls, and a concave L with a `0.145` core plus two `0.355` arms
* framework blocked-cell metadata does not generically mean absent geometry, so callers must provide an explicit boundary-participation mask

Risk:

* diagonal-only cell contact creates two coincident convex turns and may not be supported cleanly by the final art set
* generated roots must remain dedicated to board visuals because rebuild clears their children

Owner decision:

* Wall Generation, modular slot planning, the passive cell view, and the narrow visual builder are implemented in the framework package
* deterministic package tests cover single cells, rectangles, internal holes, L shapes, empty masks, duplicate input, and diagonal contact
* Drop The Man blocked coordinates are visually absent board space, but that remains a prototype mapping rather than framework meaning
* Drop The Man editor and gameplay adapters now use the shared pipeline when an optional modular cell prefab is assigned
* concrete meshes, materials, prefab references, and scene wiring remain in DropAwayPrototype
* the concrete prefab has 17 unique assigned slots, optional pieces default inactive, and the owner confirmed the generated result works correctly
* duplicate imported models and materials were removed; retained models share one canonical border, grid, and cell material

Recommended follow-up:

* keep diagonal-touch levels unsupported until a deliberate concrete-art policy is approved
* rerun the board visual playtest after future prefab geometry, material, or importer changes

---

### 11. Framework package consumption must be remote and reproducible

Status:

* Implemented / Unity Package Resolution Validated

Why it matters:

* the original relative `file:` dependency required every collaborator to clone framework and
  prototype repositories into a matching sibling layout
* that assumption does not scale safely across artists and multiple prototype repositories
* a mutable branch reference could silently resolve differently across machines or dates

Owner decision:

* committed prototypes use the framework Git repository plus package subfolder path
* every consumer pins a full 40-character framework commit SHA
* framework changes are verified, committed, and pushed before consumer pins are updated
* each consumer commits its manifest and Unity-resolved lock file when adopting a new revision
* temporary local `file:` overrides are permitted only as uncommitted developer state
* DropAwayPrototype now resolves the framework from Git at a full commit SHA, and Unity regenerated
  the lock entry with `source: git` and the matching hash before compiling successfully

Recommended follow-up:

* validate private-repository credentials on each collaborator machine once
* update each future prototype's `AGENTS.md` and manifest from the canonical workflow at creation
* consider tagged package releases or a private registry only when release cadence justifies them

---

## Immediate Non-Blockers

These are worth remembering but do not require action before the next prototype slice:

* no reusable visual feedback layer exists yet
* framework-level event and feedback generalization remains intentionally deferred; the current prototype still uses direct calls and temporary `OnGUI` HUD/result windows
* framework timer support is still countdown-only in practice, so unsupported `CountUp` content should be rejected before broader content tooling expands
* the prototype movement coordinator is now connected through a narrow drag-session owner plus scene input adapters, but long-term duplicate-sample protection should stay deliberate rather than ad hoc if input complexity grows
* shape-based fill and immediate `Full` interruption are wired; presentation-backed cap-close/shrink
  now finalizes `Closing -> Completed` through a direct callback, with immediate fallback for missing
  or invalid presentation. Single-hole and integrated square-hole checks passed; all-shape visual
  acceptance remains, not implementation of the already delivered sequence.
* the Drop The Man win predicate is now reconciled in docs: the player-facing goal is collecting all required stickmen, while the runtime victory gate is all required holes completed
* prototype-owned outcome routing, timer advancement, JSON-driven runtime spawning and result flow
  remain wired through the dev gameplay scene. Local progression now uses reusable framework
  state/storage plus game-owned replay/loop policy; its OnGUI controls still need player-facing UI.
* the runtime spawning path now uses a config-owned collectable prefab plus a prototype-owned
  shape resolver over the config hole palette; it compares exact footprint sets across four
  quarter-turns, rejects missing/duplicate/rotationally ambiguous mappings before spawning, and
  leaves JSON plus framework runtime construction presentation-agnostic; owner validation confirmed
  distinct runtime visuals for all eight canonical unrotated footprints
* the Drop The Man authoring scene now uses the same config-owned concrete hole prefabs for placed
  hole visuals and a separate click-to-rotate mode for already placed holes; manual validation
  still needs to confirm editor root alignment, tinting, and rotation coverage across every shape
* Drop The Man gameplay loading and editor board refresh now share a prototype-owned perspective
  camera positioner that fits the complete padded logical rectangle without changing rotation or
  lens settings; manually validate odd/even and wide/tall boards at target aspect ratios, plus
  editor resize, restart, and next-level reload behavior
* `GridWorldLayout` now supports explicit board-local axes, and Drop The Man's dev scene bootstrapper defaults to the intended XZ mapping where `GridCoordinate.X -> world.x`, `GridCoordinate.Y -> world.z`, and world `Y` remains visual height only
* first playtest scene adapter fixes now auto-cache child renderers/colliders for placeholder collection/completion hiding and preserve the initial pointer-to-hole drag offset before forwarding candidate positions to runtime movement
* the pointer input adapter may now clamp per-frame hole travel against a configurable max drag speed so blocker release cannot create large single-frame jumps; this remains scene-input feel only and must not become gameplay authority
* runtime-spawned hole views may now apply a visual-only spawned scale multiplier so holes read slightly smaller than their occupied board cells; authored footprint, occupancy, and JSON coordinates remain unchanged
* Drop The Man movement now clamps the freeform candidate against board bounds before swept validation so holes can slide along board edges, while wrong-color, occupied, reserved, blocked, and inactive cells still block normally
* Drop The Man collection timing now splits same-color overlap into reservation, a configurable board-local trigger threshold, asynchronous Animator/DOTween cat presentation, and callback-driven capacity fill; the cat follows a live assigned socket while the hole moves, completion resolves the reserved hole independently of active drag state, immediate fallback prevents presentation deadlock, and reserved targets that have not reached the threshold remain assigned to their hole across release/re-drag
* the Drop The Man JSON level pipeline now supports a prototype-owned readable JSON `TextAsset` source that converts into the existing framework `LevelDefinition`, framework runtime builder, and prototype runtime model path; it deliberately does not add production level-loading UX, required-hole schema, collection timing changes, or framework JSON interpretation
* the framework Resources level catalog now owns opaque `TextAsset` discovery, generic metadata validation, duplicate rejection, gap warnings, and deterministic sequence ordering; Drop The Man owns canonical `Level N` parsing and its existing JSON validation/build path, and owner validation confirmed Level 1 discovery/restart plus deterministic Next loading of Level 2 without serialized scene references
* the Drop The Man editor foundation now includes a concrete design baseline, ten shared color slots with legacy aliases, blocked-cell authored board data, a prototype-owned editor config asset, a first visual authoring shell, a dedicated play-mode authoring scene foundation with runtime hotkeys/HUD, and JSON import/export paths, while still leaving gameplay play/test bridging for a later slice
* reusable board topology, modular activation, cell-view validation, Drop The Man adapters, concrete prefab wiring, and the initial visual playtest are complete
* reusable framework level-editor extraction is still deferred until at least one more prototype proves which authoring mechanics are actually shared
* DropAwayPrototype now uses a prototype-owned URP 17.3 baseline; pipeline assets, shaders, materials, renderer features, and mobile profiling must remain outside the render-pipeline-agnostic framework
* the prototype now has material-only stencil writer/receiver test assets with no layer or renderer-feature dependency; color-matched grid/cell variants use an explicit stencil-enabled ForwardLit pass backed by URP's Lit includes but omit auxiliary passes, so they must not replace production cell materials until Game-view ordering, camera silhouette, depth, lighting, and shadow behavior are evaluated; declaring stencil around `UsePass` was tested and rejected because it did not affect the imported pass reliably
* DOTween Core is a prototype-owned dependency, and the corrected single-hole prefab has manually validated `DropTheManHolePresentation` cap-close, shrink, reset, named blend shape, stencil aperture, visual-root, collection-socket wiring, drag termination, destruction, win routing, restart, and next-level loading; immediate fallback remains implemented for missing or invalid presentation
* full-hole closing alignment is exposed on `DropTheManSceneController` as a scene-wide presentation toggle while visual feel is evaluated; enabled uses a framework grid snap query for footprint-valid view alignment, disabled preserves the final freeform drag position, and neither path routes through non-full release commit, mutates `CurrentCoordinate`, or reoccupies the departing footprint
* `GridWorldLayout.CreateCentered(...)` centers logical rectangular board bounds rather than the centroid of participating cells; Drop The Man now uses it from the gameplay bootstrapper so every runtime consumer shares one conversion, but odd/even board sizes and asymmetric blocked-cell layouts still require Unity validation
* the prototype stencil Lit receiver now supports material-configured compare operations; board materials retain `NotEqual`, while the manually validated `Hole_Inner_Cavity_Stencil` uses `Equal` against the aperture reference and replaces the inner-wall material without adding another FBX material slot

---

## Deferred Drop The Man Return Checklist

Use only when a permitted Drop The Man maintenance or presentation return is explicitly in scope.
This preserved checklist does not gate Color Block Jam architecture/design:

* keep `docs/PROJECT_STATE.md` aligned with actual implementation status
* preserve the validated modular board baseline and keep topology ownership in framework
* validate centered gameplay/editor construction and dynamic camera framing together with odd/even,
  wide/tall, and asymmetric blocked-cell levels at target aspect ratios
* September 2 cat delivery is integrated from the locally replaced `Cat.fbx`: matching mesh,
  Generic Avatar, looping `Idle_1` and six non-looping Jump takes now drive the prefab. The old
  importer named removed takes, and the old `Cat_3D` bind pose is incompatible with the new clips;
  both stale-source paths are removed. Do not mix those old and new exports or regenerate rebased
  `.anim` copies. Setup preserves valid authored trims and removes mappings to nonexistent takes.
* Cat validation passed for six exact active clips, matched early-pose renders, 36 randomized
  assignments, moving socket tracking, shrinking, callbacks, all 20 cats/five square holes through
  delayed capacity/full completion/win, restart mid-fall, and next-level reload. Runtime tint now
  affects only the body slot. Remaining visual checks: all other hole shapes, camera angles, and
  artist acceptance of pacing. Clips play at authored speed from zero; the 1.05s tween intentionally
  does not guarantee showing the entire 0.9-2.583s Jump takes. Distinct mesh hashes alone are never
  proof of readable animation variety; keep the matched early-frame and actual-collection checks.
* concrete hole views now accept multiple pointer-selection colliders for non-rectangular shapes;
  manually verify every authored collider uses the `DropTheManHole` layer and that disabling a hole
  disables every configured hit target
* manually validate all eight config-owned hole prefabs against unrotated and rotated runtime
  footprints, including logical-root alignment, presentation roots, editor click-to-rotate,
  color targets, cap blend shapes, stencil apertures, collection sockets, and completion callbacks
* sharp light-dependent marks inside the deep hole meshes are current inner-cavity self-shadows,
  not footprint or stencil mapping failures; defer the final cavity receive/cast-shadow and gradient
  policy until the owner and artist review the intended look
* the corrected single-hole aperture/cavity proof passed Game-view validation; finish all-shape,
  camera-angle and target-device depth/lighting/shadow checks before changing shader architecture.
  Do not add a renderer feature solely to redo that already validated proof.
* keep the validated prototype-owned begin/finalize callback split direct; do not introduce a framework event bus or let the view mutate gameplay lifecycle state
* keep editor authoring and gameplay runtime scenes separate unless a new approved design intentionally bridges them
* revisit framework-level editor extraction only after a second prototype validates the shared kernel
