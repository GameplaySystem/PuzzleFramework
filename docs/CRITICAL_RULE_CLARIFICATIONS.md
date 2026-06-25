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
* collection happens during drag when the hole enters or overlaps a same-color collectible cell
* snap happens only on release for final grid alignment or visual settling
* snap does not own collection timing
* snap does not own wrong-color rejection

Architecture consequence:

* framework drag and board systems still answer structural validity
* prototype rule logic answers color-based enterability and drag-time collection meaning
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

* Confirmed

Date:

* 2026-06-23

Clarified rule:

* when a hole reaches full capacity during drag, collection for that hole stops immediately
* the hole becomes non-draggable immediately
* the hole aligns back to valid cell placement
* collected targets finish their own collection animation first
* after target collection visuals finish, the hole closes and scales down to disappear
* the last collection in the level should not skip directly to `Won` before that full-hole completion sequence is respected

Architecture consequence:

* game-module rule coordination and completion flow cannot treat `all targets collected` as an instant visual end-state
* prototype-owned runtime flow must distinguish between `collection truth reached` and `hole completion sequence finished`

Implementation caution:

* do not request win purely from raw collection count without respecting the completion sequence
* do not let a full hole continue dragging after the capacity threshold is reached

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

* immediate non-blocking collection acceptance is now explicit
* collectible states and hole states are now explicit
* over-capacity handling is now explicit
* footprint-based movement validation is now explicit
* deterministic swept traversal is now explicit
* deterministic diagonal handling is now explicit
* final win gate is now explicit

Architecture consequence:

* movement and collection implementation should no longer infer edge behavior from scattered docs
* the dedicated prototype rules spec should be treated as the detailed gameplay source of truth for this slice

Implementation caution:

* if implementation pressure reveals a new movement or collection edge case, update the dedicated rules spec first

Affected docs:

* `DropAwayPrototype/docs/DropTheManMovementAndCollectionRules.md`

---

## Open Discussion Candidates

No unresolved movement or collection rule questions are currently tracked here.

If a new edge case is discovered, add it only after checking whether the dedicated rules spec already answers it.
