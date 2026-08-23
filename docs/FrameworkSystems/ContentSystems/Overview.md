# Content Systems

## Document Metadata

Category:
- Content Systems

Status:
- Approved

Parent:
- None

Related Documents:
- LevelDataSystem.md
- LevelSaveLoadSystem.md
- LevelEditorFoundation.md
- LevelCatalogSystem.md
- ../RuntimeConstructionSystems/Overview.md
- ../ProgressionSystems/Overview.md

Depends On:
- Core Board Systems
- Runtime Flow Systems where needed

Used By:
- Drop Away
- Color Block Jam
- Sky Rush
- Hole People
- Bus Jam
- Runtime Construction Systems

## Purpose

Content Systems define, persist, and author authored level content.

This category is responsible for:

* defining authored level content data
* saving and loading authored level definitions
* supporting editor-side creation and editing of level definitions

Content Systems are not responsible for runtime object construction, runtime initialization, player progression, or puzzle-specific gameplay rules.

---

# Systems In This Category

This category currently contains:

1. Level Data System
2. Level Save Load System
3. Level Editor Foundation
4. Level Catalog System

Each system has a narrow role:

* `LevelDataSystem` defines the level data model.
* `LevelSaveLoadSystem` persists authored level definitions.
* `LevelEditorFoundation` creates and edits authored level definitions.
* `LevelCatalogSystem` discovers and deterministically orders shipped authored level assets.

Together, they define the authored content side of the framework.

---

# Core Content Pipeline

The approved content pipeline is:

```text
Level Editor Foundation
    ->
LevelDefinition
    ->
Level Save Load System
    ->
JSON
    ->
Level Catalog System
    ->
Level Save Load System
    ->
LevelDefinition
    ->
Runtime Construction Systems
    ->
Playable Level
```

This pipeline makes the handoff explicit:

* the editor creates data
* save/load persists data
* the catalog discovers and selects shipped authored data
* runtime construction later consumes loaded data

That split keeps authored content concerns separate from runtime construction concerns.

---

# Content Systems vs Runtime Construction Systems

Content Systems handle authored level content.

Runtime Construction Systems handle conversion of loaded level definitions into runtime objects and runtime state.

Boundary:

* Content Systems do not instantiate runtime objects.
* Content Systems do not initialize scene objects.
* Content Systems do not spawn playable level content.
* Runtime Construction Systems later consume loaded `LevelDefinition` data and turn it into runtime objects.

This means:

* `LevelDataSystem` defines what the level data is
* `LevelSaveLoadSystem` stores and restores that data
* `LevelEditorFoundation` edits that data
* Runtime Construction Systems build playable runtime state from that data later

---

# Content Systems vs Progression Systems

Content Systems are about authored content.

Progression Systems are about player state.

Content Systems do not own:

* unlocked levels
* stars
* coins
* settings
* player progression save/load

Progression Systems must remain separate because authored level content and player progress are different kinds of data with different lifecycles.

One is shipped or authored content.

The other is player-owned state.

---

# Framework vs Game Module Ownership

Framework ownership:

* shared level definition structure
* shared persistence boundaries
* shared editor shell and generic editor capabilities

Game module ownership:

* puzzle-specific level content payloads
* puzzle-specific editor tools or adapters
* puzzle-specific validation
* puzzle-specific interpretation of authored content

The framework provides generic content infrastructure.

Game modules provide the puzzle-specific meaning.

The framework must not require puzzle-specific nouns as core authored content fields.

---

# Data Flow

Content Systems operate on data, not runtime objects.

Conceptually:

```text
Game Module Authoring Tools
    ->
Level Editor Foundation
    ->
LevelDefinition
    ->
Level Save Load System
    ->
JSON
```

Then later:

```text
JSON
    ->
Level Save Load System
    ->
LevelDefinition
    ->
Runtime Construction Systems
    ->
Runtime scene objects and runtime state
```

This is the cleanest current boundary.

If Content Systems start performing construction directly, the category becomes overloaded.

---

# What Does Not Belong Here

The following do not belong inside Content Systems:

* runtime object construction
* runtime object initialization
* spawning objects
* player progression
* puzzle-specific rules
* win conditions
* lose conditions

More specifically:

* `LevelDataSystem` should not become a runtime scene model.
* `LevelSaveLoadSystem` should remain persistence-only.
* `LevelEditorFoundation` should not become a runtime builder.

---

# Example Usage

## Drop Away

Content Systems may define and persist:

* board layout
* cell state
* generic placed content
* Drop Away-specific content payload data

Runtime Construction Systems later decide how to create playable runtime objects from that content.

## Sky Rush

Content Systems may define and persist:

* board data
* timer data where generic
* Sky Rush-specific content payload data

They do not decide how buses or passengers become runtime objects.

## Hole People

Content Systems may define and persist:

* shared board data
* shared metadata
* Hole People-specific payload content

They do not decide pathfinding execution, collection logic, or runtime scene construction.

Across all target games, the category stays the same:

* author data
* save data
* load data
* edit data

It does not own gameplay meaning or construction.

---

# Future Extensions

Possible future extensions include:

* richer editor tooling
* validation reports
* content import/export utilities
* asset-key reference registries
* version migration support
* stronger integration points with Runtime Construction Systems

These should remain subordinate to the core boundary:

Content Systems still own authored content, not runtime construction or player progression.

---

# Final Design Rule

Content Systems own authored level content data, authored level definition persistence, shipped
level discovery, and editor-side authoring support.

`LevelDataSystem` defines the data model.

`LevelSaveLoadSystem` persists level definitions.

`LevelEditorFoundation` creates and edits level definitions.

`LevelCatalogSystem` discovers and deterministically orders shipped authored level assets.

Runtime Construction Systems later consume loaded level definitions and turn them into runtime objects.

Progression Systems later save and load player progress and must remain separate.
