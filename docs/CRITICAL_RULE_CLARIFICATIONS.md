# Critical Rule Clarifications

## Purpose

This document records high-risk gameplay and interaction clarifications that were easy to misunderstand during planning or implementation discussion.

Use it when:

* a rule assumption changes how system ownership should be implemented
* a discussion reveals that previously updated docs still encode the wrong gameplay behavior
* the implementation could drift badly if the clarification is lost

This document is not a replacement for:

* framework architecture docs
* prototype rule docs
* `docs/IMPLEMENTATION_WATCHLIST.md`

Its job is narrower:

* preserve critical clarified assumptions
* surface unresolved high-risk rule questions
* give future implementation handoffs one place to verify gameplay truth before coding

---

## Status Key

* `Confirmed` means the rule has been discussed and accepted
* `Open` means the rule is still risky or underspecified and should be discussed before implementation crosses that boundary

---

## Clarifications

### Color Block Jam Is A Separate Rule Baseline (2026-09-17)

Status: separation confirmed by owner; detailed gameplay rules open.

Drop The Man is development-complete for the current MVP scope with presentation/asset integration
deferred. Color Block Jam is the active architecture/design prototype. Do not carry over same-color
collectible enterability, shape-derived capacity, reservation behavior or callback-gated hole
completion as Color Block Jam rules. Its movement, collision, gate geometry, admission/removal timing,
win/failure precedence and special-mechanic scope require their own approved specification.

Existing framework editor-only rotation remains the project constraint, not verified evidence about
the reference game's rotation behavior. Publisher descriptions of free sliding do not settle
continuous versus discrete movement, diagonal handling or collision tolerances. No queue, buffer
or gate capacity requirement is established by a matching-color exit alone.

The owner's broad asset description was provisional. Inspection of the supplied `tetra_pack.fbx`
found repeated four-cell silhouettes and no standalone unit model; it does not justify restricting
authored footprints to that mesh catalog. Shape data remains authoritative and visuals derive from it.

Framework Blocked cells remain structural. Drop The Man alone currently maps them to absent board
geometry; Color Block Jam must define its own visual mapping. Framework topology uses Convex and
Concave with explicit diagonal-touch diagnostics. The implemented modular profile's unsupported
diagonal case must not be silently treated as supported.

Affected documentation: [Color Block Jam preflight](ColorBlockJamArchitecturePreflight.md),
[project state](PROJECT_STATE.md), [watchlist](IMPLEMENTATION_WATCHLIST.md). Future module rule specs
must resolve the open questions before implementation. Existing Drop The Man rule docs remain its
maintenance baseline; this clarification does not change its gameplay.

### Replay Navigation Versus Saved Progress (2026-09-03)

Status: Confirmed by owner.

Drop The Man replay is a session-only navigation choice, not a saved campaign rewind. After a
replay win, Next Level selects the next completed shipped entry. Reaching unfinished content
returns to the first unfinished campaign level. After the final shipped replay, return to the
saved campaign/loop selection. Resume Campaign and app relaunch also use that saved selection.
Do not treat a completed replay successor as campaign play merely because its ID equals the
saved loop cursor. No save-schema or framework-policy change is needed.

Related design: `DropAwayPrototype/docs/DropTheManProgressionDesign.md`.

### 1. `Drop The Man` drag, collection, and snap ownership

Status:

* Confirmed

Date:

* 2026-06-23

Clarified rule:

* movement happens during drag
* the important gameplay question is whether the moving hole may enter the next cell during drag
* same-color collectible cells are enterable
* wrong-color collectible cells are not enterable
* collection ownership stays in the drag-time prototype rule path when the hole reaches a same-color collectible cell
* snap happens only on release for final grid alignment or visual settling
* snap does not own collection timing
* snap does not own wrong-color rejection
* updated collection presentation timing may split drag-time collection into reservation, visual trigger, presentation completion, and capacity fill

Architecture consequence:

* framework drag and board systems still answer structural validity
* prototype rule logic answers color-based enterability, drag-time reservation, and collection timing meaning
* release-time snap stays a framework alignment concern rather than the owner of prototype collection behavior

Implementation caution:

* do not build prototype collection around `GridSnapResult`
* do not describe collection as post-snap for `Drop The Man`
* do not use physics collision as gameplay authority even if colliders are later used for feel

Affected docs:

* `PuzzleFramework/docs/FrameworkArchitecture.md`
* `PuzzleFramework/docs/ImplementationRoadmap/FrameworkMVPPlan.md`
* `PuzzleFramework/docs/IMPLEMENTATION_WATCHLIST.md`
* `DropAwayPrototype/docs/DropTheManMVPRules.md`
* `DropAwayPrototype/docs/DropTheManMVPGameModuleRequirements.md`
* `DropAwayPrototype/docs/DropTheManCollectionPresentationTimingDesign.md`

---

### 2. `Drop The Man` MVP uses multi-cell holes and shape-based capacity

Status:

* Confirmed

Date:

* 2026-06-23

Clarified rule:

* the first playable `Drop The Man` slice includes all intended hole shapes, not only single-cell holes
* hole capacity is part of the first playable MVP
* hole capacity is derived from the hole footprint size for the MVP ruleset
* a single-cell hole has capacity `1`
* a double hole has capacity `2`
* a triple hole has capacity `3`
* a short `L` hole has capacity `3`
* a long `L` hole has capacity `4`
* a square hole has capacity `4`
* a `T` hole has capacity `4`
* a plus hole has capacity `5`
* holes of the same color are interchangeable collectors for that color in MVP
* authored levels are expected to match collectible counts to total provided hole capacity per color

Architecture consequence:

* `ShapeSystem` is required for the first playable MVP slice
* prototype capacity behavior is required now even if broad framework `CapacitySystem` generalization stays deferred
* prototype rule coordination must become footprint-aware rather than assuming a single-cell moving collector

Implementation caution:

* do not keep the old `single-cell first` assumption in planning docs
* do not keep the old `capacity deferred for MVP` assumption in planning docs
* do not promote shape-based hole capacity into a reusable framework system until a second game proves the abstraction

Affected docs:

* `PuzzleFramework/docs/ImplementationRoadmap/FrameworkMVPPlan.md`
* `PuzzleFramework/docs/IMPLEMENTATION_WATCHLIST.md`
* `DropAwayPrototype/docs/DropTheManMVPRules.md`
* `DropAwayPrototype/docs/DropTheManMVPGameModuleRequirements.md`

---

### 3. Full hole completion sequence gates level completion timing

Status:

* Gameplay Sequence Confirmed / Closing Alignment Under Visual Evaluation

Date:

* 2026-06-23

Clarified rule:

* when a hole reaches full capacity during drag, collection for that hole stops immediately
* the hole becomes non-draggable immediately
* closing alignment is a scene-level presentation toggle while visual feel is evaluated
* enabled aligns the view to the nearest valid footprint-origin cell before closing
* disabled keeps the final freeform drag position for closing
* neither option changes committed coordinates or occupancy
* collected targets finish their own collection animation first
* after target collection visuals finish, the hole closes and scales down to disappear
* the last collection in the level should not skip directly to `Won` before that full-hole completion sequence is respected

Architecture consequence:

* game-module rule coordination and completion flow cannot treat `all targets collected` as an instant visual end-state
* prototype-owned runtime flow must distinguish between `collection truth reached` and `hole completion sequence finished`

Implementation caution:

* do not request win purely from raw collection count without respecting the completion sequence
* do not let a full hole continue dragging after the capacity threshold is reached
* do not treat the closing-alignment toggle as gameplay snap or structural commit

Affected docs:

* `PuzzleFramework/docs/IMPLEMENTATION_WATCHLIST.md`
* `DropAwayPrototype/docs/DropTheManMVPRules.md`
* `DropAwayPrototype/docs/DropTheManMVPGameModuleRequirements.md`
* `DropAwayPrototype/docs/DropTheManMovementAndCollectionRules.md`

---

### 4. Dedicated movement and collection rules spec now owns the remaining edge cases

Status:

* Confirmed

Date:

* 2026-06-23

Clarified rule:

* same-color collection reservation/acceptance and non-blocking behavior is now explicit
* collectible states and hole states are now explicit
* over-capacity handling is now explicit
* footprint-based movement validation is now explicit
* deterministic swept traversal is now explicit
* deterministic diagonal handling is now explicit
* final win gate is now explicit
* newer collection presentation timing design splits immediate gameplay reservation from visual trigger, presentation completion, and capacity fill
* an accepted reservation remains assigned to its hole across non-full release; release snap does not trigger or cancel it

Architecture consequence:

* movement and collection implementation should no longer infer edge behavior from scattered docs
* the dedicated prototype rules spec should be treated as the detailed gameplay source of truth for this slice

Implementation caution:

* if implementation pressure reveals a new movement or collection edge case, update the dedicated rules spec first

Affected docs:

* `DropAwayPrototype/docs/DropTheManMovementAndCollectionRules.md`
* `DropAwayPrototype/docs/DropTheManCollectionPresentationTimingDesign.md`

---

## Open Discussion Candidates

The preferred full-hole closing presentation remains under visual evaluation: nearest-cell
alignment versus closing from the final freeform drag position.

If a new edge case is discovered, add it only after checking whether the dedicated rules spec already answers it.
