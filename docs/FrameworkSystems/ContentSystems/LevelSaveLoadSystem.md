# Level Save Load System

## Document Metadata

Category:
- Content Systems

Status:
- Approved

Parent:
- Overview.md

Related Documents:
- LevelDataSystem.md
- LevelEditorFoundation.md
- ../RuntimeConstructionSystems/Overview.md
- ../ProgressionSystems/Overview.md

Depends On:
- Level Data System

Used By:
- Drop Away
- Color Block Jam
- Sky Rush
- Hole People
- Bus Jam
- Level Editor Foundation
- Runtime Construction Systems

## Purpose

The Level Save Load System is responsible only for persistence of authored level definitions.

It saves and loads level content data.

It does not instantiate runtime objects.

It does not initialize scene objects.

It does not build playable levels.

It does not save or load player progression.

---

# Core Design Idea

The Level Save Load System persists `LevelDefinition` data.

Its responsibility is narrow by design:

```text
Editor or tool produces LevelDefinition
    ->
Level Save Load System persists it
    ->
Stored level file can later be loaded back into LevelDefinition
```

This keeps persistence separate from:

* level structure definition
* runtime construction
* player progression save/load

That separation is necessary to avoid turning save/load into a god system that also tries to define data, build scenes, and own game rules.

---

# Responsibilities

The Level Save Load System is responsible for:

* Saving authored level definitions
* Loading authored level definitions
* Reading and writing the chosen persistence format
* Returning structured success or failure results
* Performing basic post-load data integrity checks
* Preserving the framework and game-module level content structure defined by `LevelDefinition`

---

# Should Not Handle

The Level Save Load System should not handle:

* Defining level structure
* Spawning objects
* Instantiating runtime scene objects
* Calling initialization methods on scene objects
* Building playable runtime levels
* Player progression data
* Player progression save/load
* Unlocked levels
* Stars
* Coins
* Settings
* Puzzle-specific deep validation
* Game-specific rules
* Knowledge of holes, buses, stickmen, doors, bricks, colors, or puzzle-specific nouns

Persistence belongs here.

Construction belongs to Runtime Construction Systems.

Player progress belongs to Progression Systems.

---

# JSON As MVP Format

JSON should be the MVP storage format.

Reason:

* human-readable
* Git-friendly
* easy to diff
* easy to inspect and debug
* good for portfolio documentation
* useful for editor tooling and external level generation

JSON fits the current project phase because documentation clarity and inspectability are more important than early optimization.

This also supports:

* hand-authored sample levels
* automated tooling later
* easier review of level changes in version control

---

# ScriptableObject Relationship

`ScriptableObject` is not the MVP level format.

However, `ScriptableObject` may still be useful later for:

* Unity asset references
* prefab registries
* visual configuration
* editor palettes
* reusable configuration assets

The current direction is:

* JSON level files store authored level definition data
* Unity assets may later be referenced by ids or keys
* direct object references should not be the primary MVP level persistence model

This boundary matters because direct Unity object references are convenient inside the editor but are weaker for:

* diffability
* external tooling
* portability
* clean separation between data and runtime objects

---

# Editor Save Flow

The expected editor save flow is:

```text
Level Editor
    ->
LevelDefinition
    ->
LevelSaveLoadSystem.Save
    ->
JSON file
```

The Level Editor or related tooling prepares the authored level data.

The Level Save Load System persists that data.

It should not reinterpret level meaning during save.

It should preserve the structure already defined by `LevelDefinition`.

---

# Runtime Load Flow

The expected runtime load flow is:

```text
JSON file
    ->
LevelSaveLoadSystem.Load
    ->
LevelDefinition
    ->
Runtime Construction Systems later build playable objects
```

The Level Save Load System ends at the loaded data model.

A later system is responsible for:

* turning data into runtime objects
* building runtime state
* calling initialization flows where needed

This separation is one of the key architecture boundaries.

---

# Validation Scope

The Level Save Load System may perform basic data integrity validation after load.

Examples:

* file exists
* JSON is readable
* version exists
* level id exists
* required root fields exist
* deserialization succeeds

This is persistence-level validation.

It is not puzzle-specific validation.

The system should not deeply decide whether a level is playable or whether game rules are satisfied.

Game-specific validation belongs to:

* game modules
* future runtime construction validation

This boundary keeps persistence concerns from absorbing gameplay logic.

---

# Versioning

Level JSON should include a version field.

Reason:

Even early in the project, level data formats are likely to evolve.

The MVP does not need full migration support.

However, the design should reserve space for future migration by ensuring each saved level definition can declare its own format version.

Current recommendation:

* include a version field in the root data
* reject or flag incompatible versions gracefully
* defer migration pipelines until later

This keeps the MVP simple while avoiding a dead-end format.

---

# Relationship With Level Data System

The Level Data System owns level structure.

The Level Save Load System persists that structure.

Boundary:

* `LevelDataSystem` defines what a level is
* `LevelSaveLoadSystem` defines how that level is saved and loaded

The Level Save Load System should not redefine or reshape the architecture owned by the Level Data System.

---

# Relationship With Runtime Construction Systems

Runtime Construction Systems convert loaded level data into runtime scene objects and runtime state.

The Level Save Load System does not do that.

Boundary:

* `LevelSaveLoadSystem.Load(...)` returns authored level data
* Runtime Construction Systems decide how to build runtime objects from that data

A useful conceptual split is:

* Framework persistence systems provide the saved data socket
* Framework construction systems may later provide the construction contract
* Game modules may provide adapters or builders for game-specific payloads

That adapter idea should remain conceptual for now.

This document does not finalize any construction implementation.

---

# Relationship With Progression Systems

Progression Systems handle player progression concerns.

Examples:

* unlocked levels
* stars
* coins
* settings
* profile progress

The Level Save Load System must remain separate from those concerns.

A level definition file is authored content.

A progression save is player state.

Treating them as the same kind of data would create hidden coupling between content authoring and player progress tracking.

---

# API Shape

This is architecture documentation only.

One acceptable conceptual API shape is:

```csharp
public interface ILevelSaveLoadService
{
    SaveResult Save(LevelDefinition levelDefinition, string path);
    LoadResult<LevelDefinition> Load(string path);
}
```

```csharp
public struct SaveResult
{
    public bool Success;
    public string FailureReason;
}
```

```csharp
public struct LoadResult<T>
{
    public bool Success;
    public T Data;
    public string FailureReason;
}
```

This shape fits the current design because:

* persistence may fail during normal operation
* save/load should report success explicitly
* failure should be inspectable without exceptions as the only control path

This is not a finalized implementation commitment.

It is a reasonable architecture-level contract shape.

---

# Edge Cases

## File Missing

If the requested file does not exist:

* load should fail safely
* a failure reason should be returned

The system should not pretend a missing level file is valid data.

---

## Invalid JSON

If the JSON content is unreadable or malformed:

* load should fail safely
* a failure reason should be returned

This is a persistence concern, not a gameplay concern.

---

## Missing Required Root Fields

If required fields such as version or level id are missing:

* load should fail or return invalid data status

The system should reject incomplete root-level content early.

---

## Unknown Future Version

If a level file uses a newer unsupported version:

* the system should fail clearly
* the design should leave room for future migration support

The MVP does not need migration logic, but it should not ignore version mismatch.

---

## Asset Reference Resolution

If JSON later references Unity assets by ids or keys:

* unresolved references should be reported clearly

However, direct runtime asset resolution should not be the central responsibility of persistence itself.

That concern may later involve shared config registries or runtime construction support.

---

## Overloaded Save Scope

A common failure mode is letting level save/load absorb unrelated concerns because it already reads and writes files.

Examples of incorrect expansion:

* saving player progression here
* spawning objects during load
* validating puzzle solvability during load
* calling runtime initialization directly

The document boundary should block that expansion.

---

# MVP Scope

The first version of the Level Save Load System should support:

* Saving authored `LevelDefinition` data
* Loading authored `LevelDefinition` data
* JSON as the MVP persistence format
* Basic success and failure result reporting
* Basic post-load data integrity validation
* Root-level version field support
* Clear separation from runtime construction
* Clear separation from progression save/load

---

# Future Extensions

The following can be added later:

* Version migration support
* Batch import and export tools
* Pretty-print and minified format options
* External level generation pipelines
* Asset key registries
* Compression or alternative file formats
* Schema validation tools
* Editor validation integrations
* Remote or cloud-backed level repositories

---

# Final Design Rule

The Level Save Load System is a persistence system for authored level definitions.

It saves and loads `LevelDefinition` data.

It does not define the level model.

It does not build runtime objects.

It does not initialize scene objects.

It does not save player progression.
