# Implementation Roadmap

## Purpose

This document defines the recommended implementation sequence for the Puzzle Framework after architecture documentation is complete.

The goal is to:

* build a usable framework incrementally
* avoid premature abstraction
* prove shared systems through real game slices
* keep the framework portfolio-quality and technically coherent

This roadmap is not a hard production schedule.

It is a sequencing strategy.

---

# Recommended Approach

The implementation order should optimize for:

* fast proof that the architecture can actually run
* early validation of framework boundaries
* pressure-testing shared systems with a second game before broad generalization
* incremental portfolio value

The main recommendation is:

* do not perform broad framework generalization before the second game creates real reuse pressure

That means the roadmap should not treat “generalize shared systems” as a large standalone abstraction phase too early.

Instead:

* build a small framework MVP
* prove it with one real game slice
* apply second-game pressure
* generalize only where reuse is actually confirmed

---

# Recommended Roadmap

1. Framework MVP Slice
2. Drop Away Vertical Slice
3. Color Block Jam Pressure Test
4. Targeted Shared-System Generalization
5. Sky Rush
6. Hole People
7. Bus Jam
8. Polish and Portfolio Cleanup

This is close to your proposed order, but it makes one deliberate change:

* generalization moves after the second game starts applying real pressure

That reduces the risk of overengineering abstractions that only fit the first game.

---

# Why This Order

## Problem

The framework is fully documented, but unproven in code.

The next risk is not missing architecture.

The next risk is implementing too much abstraction before runtime behavior is proven.

## Assumptions

This roadmap assumes:

* Drop Away is the simplest first proof target
* Color Block Jam is the best second-game pressure test for shared board and interaction systems
* pathfinding-heavy games should come later because they introduce more complexity
* portfolio value improves when the framework is shown evolving through real slices rather than being abstractly “complete” first

## Risks

Main risks:

* overgeneralizing after only one game
* building too much framework before anything is playable
* delaying the first real prototype too long
* mixing framework stabilization with puzzle-specific rule work

## Simpler Alternative

A simpler but weaker option would be:

```text
Build full framework first
    ->
Then build games
```

That looks clean on paper, but it usually produces speculative abstractions and slower feedback.

## Recommended Approach

Build the minimum shared slice needed for one game.

Prove it.

Apply second-game pressure.

Then generalize only the parts that actually need to become broader framework systems.

---

# Phase 1: Framework MVP Slice

## Goal

Implement the minimum framework backbone needed to support one playable vertical slice.

## Scope

Expected first-pass implementation focus:

* Content Systems needed to load a simple level definition
* Runtime Construction Systems needed to build runtime state
* Core Board Systems needed for a grid-based board
* Interaction Systems needed for basic drag and snap
* Runtime Flow Systems needed for game state and event wiring
* Presentation Systems only where needed for readability

Keep this slice narrow.

Do not try to fully implement every documented framework feature in phase 1.

## Exit Criteria

Phase 1 is complete when:

* a simple authored level can load
* runtime state can be built from that level
* board interaction works at a basic level
* one game can plausibly be layered on top without architecture changes

---

# Phase 2: Drop Away Vertical Slice

## Goal

Build the first fully playable end-to-end slice using the framework MVP.

## Why Drop Away First

Drop Away is a strong first slice because it stresses:

* grid movement
* drag and snap
* occupancy
* capacity-style collection
* runtime flow
* presentation feedback

without needing the higher complexity of pathfinding-heavy behavior.

## Scope

Focus on:

* gameplay loop completeness
* one shippable-feeling slice
* identifying framework gaps
* documenting mismatches between approved docs and implementation reality

## Exit Criteria

Phase 2 is complete when:

* Drop Away is playable from level load to outcome
* the framework MVP has survived one real game
* implementation-driven documentation adjustments are clearly identified

---

# Phase 3: Color Block Jam Pressure Test

## Goal

Use a second game to test whether the framework boundaries actually generalize.

## Why This Comes Before Broad Generalization

One game proves usability.

Two games prove whether a system is truly shared.

Color Block Jam is a better test of generality than trying to abstract after Drop Away alone.

It pressures:

* shape support
* drag behavior
* snap validation
* board constraints
* exit-style interactions

## Exit Criteria

Phase 3 is complete when:

* Color Block Jam runs on the framework with only targeted game-module code where expected
* repeated framework friction points are clearly visible
* real candidates for generalization are known

---

# Phase 4: Targeted Shared-System Generalization

## Goal

Stabilize and generalize only the framework systems that proved reusable under at least two games.

## Rules

Generalize:

* when two games require the same behavior through the same abstraction
* when duplication is real
* when framework ownership is clearly justified

Do not generalize:

* speculative future needs
* one-off game rules
* puzzle-specific meaning

## Likely Focus Areas

Potential areas to refine after phases 2 and 3:

* runtime construction adapters
* shared drag and snap behaviors
* board placement utilities
* content payload wiring
* presentation hooks

## Exit Criteria

Phase 4 is complete when:

* the shared systems that truly belong in the framework have been tightened
* duplicated first-game and second-game glue has been reduced
* framework boundaries are cleaner than after the first vertical slice

---

# Phase 5: Sky Rush

## Goal

Add a queue and capacity heavy game that stresses Resource Processing Systems and runtime flow more deeply.

## Why Here

Sky Rush introduces meaningful pressure on:

* queues
* capacity
* timer flow
* transfer-oriented gameplay

This is a good next test after grid and shape-oriented slices are already proven.

## Exit Criteria

Phase 5 is complete when:

* queue and capacity systems are proven in real gameplay
* timer and runtime flow interactions are production-credible
* framework seams are still holding under multi-system coordination

---

# Phase 6: Hole People

## Goal

Add the first pathfinding-heavy slice.

## Why After Sky Rush

Pathfinding adds a different complexity class.

It should come after the more basic board, interaction, runtime construction, and resource flow patterns are already proven.

Hole People is a good first pathfinding stress test because it combines:

* board traversal
* buffers
* queue behavior
* pathfinding-dependent decisions

## Exit Criteria

Phase 6 is complete when:

* pathfinding integrates cleanly with existing board systems
* resource flow and movement rules coexist without breaking boundaries

---

# Phase 7: Bus Jam

## Goal

Pressure-test the framework against a more complex combination of:

* pathfinding
* selection
* buffer usage
* puzzle-specific movement meaning

## Why Later

Bus Jam is better used as a maturity test than as an early architecture proof.

By this point, the framework should already be stable enough that Bus Jam reveals real edge-case issues rather than first-principles instability.

## Exit Criteria

Phase 7 is complete when:

* Bus Jam can be built mostly through game-module logic on top of established framework systems
* any remaining architecture gaps are narrow and actionable

---

# Phase 8: Polish and Portfolio Cleanup

## Goal

Turn the working framework and prototypes into a coherent portfolio artifact.

## Scope

Focus on:

* cleanup of technical debt that actually matters
* documentation updates from implementation reality
* repository organization
* demo quality
* readme and portfolio-facing clarity
* visual and UX polish where needed

Do not treat this as a vague “fix everything” phase.

Keep cleanup tied to presentation quality, maintainability, and portfolio value.

## Exit Criteria

Phase 8 is complete when:

* the repository tells a coherent engineering story
* the prototypes demonstrate framework reuse clearly
* docs and implementation match closely enough to be credible

---

# Implementation Principles During This Roadmap

## Principle 1: Small Framework First

Implement the thinnest viable shared slice before expanding horizontally.

## Principle 2: Prove With Real Games

A framework system is only truly proven when a real game uses it cleanly.

## Principle 3: Generalize Under Pressure

Do not abstract because a future game might need something.

Abstract when a second real use case forces a cleaner shared shape.

## Principle 4: Update Docs When Reality Changes

If implementation reveals documentation drift, propose doc updates in the same task summary.

## Principle 5: Keep Game Modules Honest

If puzzle-specific nouns or rules start creeping into framework code, stop and correct the boundary.

---

# Roadmap Summary

Recommended sequence:

```text
1. Framework MVP Slice
2. Drop Away Vertical Slice
3. Color Block Jam Pressure Test
4. Targeted Shared-System Generalization
5. Sky Rush
6. Hole People
7. Bus Jam
8. Polish and Portfolio Cleanup
```

This order balances:

* early proof of architecture
* low-risk first implementation
* second-game validation before overgeneralization
* later pressure from pathfinding-heavy and resource-heavy games

---

# Final Design Rule

The implementation roadmap should favor proof over speculation.

Build the minimum framework slice.

Prove it with a real game.

Use the second game to force honest generalization.

Then expand the framework through increasingly demanding prototypes.
