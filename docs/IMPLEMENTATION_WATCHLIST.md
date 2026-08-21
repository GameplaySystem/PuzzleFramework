# Implementation Watchlist

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

## Immediate Non-Blockers

These are worth remembering but do not require action before the next prototype slice:

* no reusable visual feedback layer exists yet
* framework-level event and feedback generalization remains intentionally deferred; the current prototype still uses direct calls and temporary `OnGUI` HUD/result windows
* framework timer support is still countdown-only in practice, so unsupported `CountUp` content should be rejected before broader content tooling expands
* the prototype movement coordinator is now connected through a narrow drag-session owner plus scene input adapters, but long-term duplicate-sample protection should stay deliberate rather than ad hoc if input complexity grows
* shape-based fill, immediate `Full` interruption, and the narrow synchronous `Full -> Closing -> Completed` foundation are now wired, but presentation-backed close/disappear sequencing is still deferred
* the Drop The Man win predicate is now reconciled in docs: the player-facing goal is collecting all required stickmen, while the runtime victory gate is all required holes completed
* prototype-owned outcome routing, timer advancement, JSON-driven runtime spawning, and the basic result loop are now wired through the dev gameplay scene, but they remain prototype-owned and dev-scene oriented rather than production progression infrastructure
* the current runtime spawning path still clones scene-local hole/stickman templates in the gameplay test scene rather than using a dedicated prefab pipeline
* `GridWorldLayout` now supports explicit board-local axes, and Drop The Man's dev scene bootstrapper defaults to the intended XZ mapping where `GridCoordinate.X -> world.x`, `GridCoordinate.Y -> world.z`, and world `Y` remains visual height only
* first playtest scene adapter fixes now auto-cache child renderers/colliders for placeholder collection/completion hiding and preserve the initial pointer-to-hole drag offset before forwarding candidate positions to runtime movement
* the pointer input adapter may now clamp per-frame hole travel against a configurable max drag speed so blocker release cannot create large single-frame jumps; this remains scene-input feel only and must not become gameplay authority
* runtime-spawned hole views may now apply a visual-only spawned scale multiplier so holes read slightly smaller than their occupied board cells; authored footprint, occupancy, and JSON coordinates remain unchanged
* Drop The Man movement now clamps the freeform candidate against board bounds before swept validation so holes can slide along board edges, while wrong-color, occupied, reserved, blocked, and inactive cells still block normally
* Drop The Man collection timing now splits same-color overlap into reservation, a configurable board-local trigger threshold, synchronous placeholder completion, and capacity fill; real asynchronous falling presentation remains deferred, and reserved targets that have not reached the threshold remain assigned to their hole across release/re-drag
* the Drop The Man JSON level pipeline now supports a prototype-owned readable JSON `TextAsset` source that converts into the existing framework `LevelDefinition`, framework runtime builder, and prototype runtime model path; it deliberately does not add production level-loading UX, required-hole schema, collection timing changes, or framework JSON interpretation
* the Drop The Man editor foundation now includes a concrete design baseline, ten shared color slots with legacy aliases, blocked-cell authored board data, a prototype-owned editor config asset, a first visual authoring shell, a dedicated play-mode authoring scene foundation with runtime hotkeys/HUD, and JSON import/export paths, while still leaving gameplay play/test bridging for a later slice
* reusable board topology, modular activation, cell-view validation, Drop The Man adapters, concrete prefab wiring, and the initial visual playtest are complete
* reusable framework level-editor extraction is still deferred until at least one more prototype proves which authoring mechanics are actually shared
* DropAwayPrototype now uses a prototype-owned URP 17.3 baseline; pipeline assets, shaders, materials, renderer features, and mobile profiling must remain outside the render-pipeline-agnostic framework

---

## Recommended Next Checkpoint

Before the next placeholder replacement:

* keep `docs/PROJECT_STATE.md` aligned with actual implementation status
* preserve the validated modular board baseline and keep topology ownership in framework
* design concrete hole/cat presentation as separate prototype-owned slices
* validate the future stencil hole effect against real hole meshes before approving shader state or adding a renderer feature
* keep editor authoring and gameplay runtime scenes separate unless a new approved design intentionally bridges them
* revisit framework-level editor extraction only after a second prototype validates the shared kernel
