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

* 2026-06-22

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

### 1. `PROJECT_STATE.md` overstates category completion

Status:

* Accepted Follow-Up

Why it matters:

* `docs/PROJECT_STATE.md` currently marks multiple system categories as `[done]` even though only a subset has been implemented in code so far
* this can mislead future context checks, handoffs, and milestone decisions

Examples:

* `Shape System [done]`
* `Wall Generation System [done]`
* `Pathfinding System [done]`
* `Event System [done]`
* `Visual Feedback System [done]`
* `Progress Save Load System [done]`

Interpretation risk:

* readers may assume those systems are implemented rather than merely documented or approved

Owner decision:

* approved for follow-up
* future progress tracking should be more explicit and more detailed at the step level

Recommended follow-up:

* clarify whether category completion sections mean `documented`, `approved`, or `implemented`
* if they are meant to reflect implementation, correct the current overstated entries before later milestone reviews

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

### 7. Color identity breadth may need expansion after the second game

Status:

* Low Priority

Why it matters:

* current framework `ColorIdentity` is intentionally small and fixed
* current `ColorSystem` maps only to `UnityEngine.Color`

Risk:

* second-game pressure may reveal the need for more identities or richer visual mapping than a raw color value

Owner decision:

* simple equality-based color matching is not currently the concern
* the likely future pressure point is shared identity breadth and richer visual mapping, not whether game-module matching can compare ids

Current interpretation:

* keep game-rule color matching inside the game module
* leave the framework color slice narrow until a second real game forces broader reuse pressure

Recommended follow-up:

* revisit only after `ColorBlockJamPrototype` creates real reuse pressure

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

## Immediate Non-Blockers

These are worth remembering but do not require action before the next prototype slice:

* no reusable visual feedback layer exists yet
* timer start and stop hookup to actual gameplay state is not implemented yet
* the swept footprint helper foundation now exists, but it is not wired into a gameplay rule coordinator yet
* wrong-color entry blocking is documented but not yet wired into a prototype rule coordinator
* drag-time matching collection is documented but not yet wired into a prototype rule coordinator
* shape-based capacity completion is documented but not yet wired into a prototype rule coordinator
* the dedicated movement and collection rules spec now exists, but implementation has not been reconciled against it yet
* win and lose evaluation are not yet wired into `GameStateSystem`

---

## Recommended Next Checkpoint

Before implementing scene objects or presentation reactions, complete the smallest prototype rule coordinator that:

* follows `DropAwayPrototype/docs/DropTheManMovementAndCollectionRules.md` as the detailed gameplay source of truth
* consumes the swept footprint helper instead of final-position-only validation
* applies prototype-owned entry blocking for wrong-color target cells during interaction validation
* evaluates drag-time cell entry or overlap against target color identity
* evaluates multi-cell footprint behavior against shape-based capacity assumptions
* applies matching collection during drag
* stops and locks a hole immediately when it reaches full capacity
* coordinates hole completion sequencing before requesting win when appropriate
* requests `Won` or `Lost` through `GameStateSystem`
* connects timer expiry to the prototype lose rule

After that slice, run another context check and update this watchlist.
