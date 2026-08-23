# Level Data System

## Document Metadata

Category:
- Content Systems

Status:
- Approved

Parent:
- Overview.md

Related Documents:
- LevelSaveLoadSystem.md
- LevelEditorFoundation.md
- LevelCatalogSystem.md
- ../CoreBoardSystems/GridSystem.md
- ../CoreBoardSystems/ShapeSystem.md

Depends On:
- Grid System
- Shape System
- Timer System

Used By:
- Drop Away
- Color Block Jam
- Sky Rush
- Hole People
- Bus Jam
- Level Catalog System

## Purpose

The Level Data System defines how authored level content is structured, organized, and handed to gameplay systems.

It exists to give the framework a reusable level definition model without forcing every game into one giant shared data object.

The Level Data System should support:

* shared framework-level level data
* modular game-specific content
* clean separation between framework ownership and game module ownership
* separation between authored level content and player progression data

---

# Core Design Idea

The Level Data System should use a hybrid modular level data approach.

It should not use:

* one giant level data god object
* hundreds of disconnected tiny fragments with no shared root

The core structure is:

```text
LevelDefinition
    -> Metadata
    -> Shared framework data
    -> Game-specific content payload
```

This gives the project:

* one common entry point for loading a level
* one place for shared framework structures
* one modular payload for game-specific data

---

# Problem Being Solved

Multiple target games need level content data, but they do not need the same puzzle-specific fields.

If the framework uses one massive shared level object, it will eventually accumulate game-specific nouns and optional fields such as:

* hole data
* bus data
* passenger data
* brick data
* door data

That would violate framework boundaries and make the model harder to maintain.

At the other extreme, if every part of a level becomes its own disconnected fragment, the system loses coherence.

That would create:

* unclear loading flow
* weak validation boundaries
* fragile editor integration
* scattered serialization logic

The Level Data System exists to solve that middle problem:

* one shared root
* shared framework structures where appropriate
* modular game-specific payloads where meaning differs

---

# Responsibilities

The Level Data System is responsible for:

* Defining the common root level definition structure
* Defining which level data belongs to the framework
* Providing a place for shared board-related content
* Providing a place for generic level metadata
* Supporting modular game-specific content payloads
* Keeping framework-owned and game-owned data clearly separated
* Supporting future loading, save, and editor workflows
* Defining authored level content structure, not player progression structure

---

# Should Not Handle

The Level Data System should not handle:

* Runtime gameplay logic
* Animation
* Input
* Pathfinding execution
* Resource processing execution
* Win conditions
* Lose conditions
* Puzzle-specific rules
* Game-specific content interpretation inside framework-owned fields
* Player progression data
* Player progression save/load

The Level Data System defines structure.

Other systems decide behavior.

---

# Level Data Structure

The approved direction is a common root definition with modular ownership.

Conceptually:

```text
LevelDefinition
    -> Metadata
    -> FrameworkData
    -> ContentPayload
```

Example conceptual shape:

```text
LevelDefinition
    Metadata
        LevelId
        DisplayName
        Version
    FrameworkData
        BoardData
        CellData
        ShapePlacements
        TimerData
        SharedConfigReferences
    ContentPayload
        Game-specific content
```

This is not a final implementation type.

It is the approved architecture shape.

---

# Shared Framework Data

Framework owns shared level structures that are generic across multiple games.

Examples of framework-owned level data:

* Level metadata
* Board data
* Cell data
* Shape placement data where generic
* Timer data where generic
* Shared configuration references

These structures belong in the framework because they describe reusable level concerns rather than puzzle-specific meaning.

Examples:

* board width and height
* which cells are active or blocked
* generic placed shapes
* level timer configuration when the timer is a shared framework concept

Important boundary:

Framework-owned data should describe structure, layout, and shared configuration.

It should not require puzzle-specific nouns as mandatory fields.

---

# Game-Specific Content Payloads

Game modules own game-specific content payloads.

Examples:

* `DropAwayLevelContent`
* `ColorBlockJamLevelContent`
* `SkyRushLevelContent`
* `HolePeopleLevelContent`
* `BusJamLevelContent`

These payloads contain puzzle-specific level content that the framework should not promote into shared required fields.

Examples of likely game-owned content:

* hole behavior content
* bus route or bus occupancy content
* passenger grouping content
* door rule content
* brick exit rule content

The framework should allow these payloads to exist.

It should not attempt to normalize all puzzle-specific meaning into one shared schema.

---

# Framework vs Game Module Ownership

Framework ownership:

* root level definition shape
* shared level metadata
* shared board-related structures
* generic timer-related structures where reused
* shared configuration references

Game module ownership:

* puzzle-specific content payload models
* puzzle-specific fields
* puzzle-specific validation rules
* puzzle-specific interpretation of loaded content

Ownership rule:

Framework provides the common container and shared reusable data structures.

Game modules provide the content meaning.

Framework must not require game-specific nouns as core level fields.

---

# Serialization Considerations

There is a real tradeoff here.

A conceptual content contract is useful at the architecture level, but pure interfaces like `ILevelContent` may be awkward for Unity and JSON serialization.

This means the design should separate:

* conceptual contract

from:

* concrete serialization strategy

Possible implementation directions later include:

* `LevelDefinition<TContent>`
* type or id-based payloads
* `ScriptableObject` wrappers
* serializable DTOs

This document does not finalize which of those should be used.

Reason:

Choosing too early would turn an architectural doc into an implementation constraint before the serialization and editor workflow tradeoffs are fully understood.

Design caution:

If the project pushes too hard toward interface purity, Unity serialization may become awkward.

If the project pushes too hard toward convenience, the data model may collapse into a giant inspector-driven god object.

The architecture should stay modular enough to avoid both extremes.

---

# Example Level Content Models

These are conceptual examples, not final implementation types.

## Shared Root

```text
LevelDefinition
    -> Metadata
    -> FrameworkData
    -> ContentPayload
```

## Drop Away

```text
LevelDefinition
    -> Metadata
    -> FrameworkData
        -> BoardData
        -> CellData
        -> ShapePlacements
    -> DropAwayLevelContent
```

## Color Block Jam

```text
LevelDefinition
    -> Metadata
    -> FrameworkData
        -> BoardData
        -> CellData
        -> ShapePlacements
    -> ColorBlockJamLevelContent
```

## Sky Rush

```text
LevelDefinition
    -> Metadata
    -> FrameworkData
        -> BoardData
        -> CellData
        -> TimerData
    -> SkyRushLevelContent
```

## Hole People

```text
LevelDefinition
    -> Metadata
    -> FrameworkData
        -> BoardData
        -> CellData
        -> ShapePlacements
    -> HolePeopleLevelContent
```

## Bus Jam

```text
LevelDefinition
    -> Metadata
    -> FrameworkData
        -> BoardData
        -> CellData
        -> TimerData
    -> BusJamLevelContent
```

These examples show the intended ownership split:

* framework data stays generic
* payload data carries puzzle-specific meaning

---

# Edge Cases

## Shared Data Expands Too Far

Risk:

Framework data slowly accumulates game-specific fields because multiple games seem similar at first glance.

Rule:

A field belongs in shared framework data only if it represents a reusable generic structure, not a puzzle-specific interpretation.

---

## Payload Becomes Opaque

Risk:

The root definition becomes too generic, and all meaningful content is dumped into one opaque payload with weak validation.

Rule:

Keep shared framework structures explicit where they are genuinely shared across games.

Do not hide reusable board-level structure inside game payloads just to make the root simpler.

---

## Serialization Strategy Forces Architecture

Risk:

Unity serializer constraints push the design toward awkward wrappers or duplicated DTOs.

Rule:

Let the architecture describe ownership and structure first.

Choose a serialization implementation later that preserves those boundaries as closely as possible.

---

## Generic Shape Placement Is Misclassified

Risk:

Some placed content may look generic but actually carries game-specific meaning.

Rule:

Only generic placement data belongs in framework-owned shape placement structures.

Puzzle-specific semantics still belong in the game payload.

---

## Timer Data Is Not Shared Everywhere

Risk:

Timer support may be generic in some games but irrelevant in others.

Rule:

Timer data should be optional and only included where the framework timer concept is actually reused.

---

# MVP Scope

The first version of the Level Data System should support:

* One common root level definition concept
* Shared framework-level data grouping
* Modular game-specific content payloads
* Generic level metadata
* Board-related shared level data
* Optional generic timer data
* Clear framework versus game module ownership boundaries
* Clear separation between authored level content and player progression
* Serialization strategy deferred until implementation design

---

# Future Extensions

The following can be added later:

* Versioned migration support
* Validation pipelines
* Editor-facing authoring wrappers
* Data import and export formats
* Payload type registries
* Save-game compatible runtime DTOs
* Partial level overrides
* Addressable content references
* Localization-ready metadata

---

# Final Design Rule

The Level Data System should use one shared root level definition with modular ownership.

Framework owns shared level structures.

Game modules own puzzle-specific content payloads.

The framework must not require game-specific nouns as core level fields.

The Level Data System is about authored level content, not player progression.

The architecture should remain modular enough to avoid a god object, but coherent enough to avoid fragmented disconnected data.
