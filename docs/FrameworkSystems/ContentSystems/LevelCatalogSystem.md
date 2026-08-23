# Level Catalog System

## Document Metadata

Category:
- Content Systems

Status:
- Implemented Foundation

Parent:
- Overview.md

Related Documents:
- LevelDataSystem.md
- LevelSaveLoadSystem.md
- ../RuntimeConstructionSystems/LevelRuntimeBuilderSystem.md
- ../ProgressionSystems/Overview.md

Depends On:
- Level Save Load System
- Game-module catalog metadata adapters

Used By:
- Drop Away
- Future puzzle game level selectors and runtime bootstrap adapters

## Purpose

The Level Catalog System discovers shipped authored level assets, validates generic catalog metadata,
and exposes them in a deterministic order.

It replaces manually maintained scene arrays without deciding how a puzzle parses, constructs,
plays, unlocks, or completes a level.

## Core Boundary

The framework owns:

* discovery from an approved asset source
* deterministic ordering by a positive sequence number
* non-empty stable level identifiers
* duplicate identifier and duplicate sequence rejection
* gap warnings
* index-based access to catalog entries
* structured success, warning, and failure reporting

The game module owns:

* reading its level format
* interpreting its level identifier convention
* converting an authored asset into catalog metadata
* puzzle-specific schema and construction validation
* restart and next-level consequences

The framework must treat the authored asset contents as opaque. It must not inspect holes,
stickmen, buses, doors, colors, or any other puzzle-specific payload.

## MVP Asset Source

The first implementation uses Unity `Resources` and `TextAsset` level files.

The configured path is relative to a `Resources` folder. For example:

```text
Assets/Resources/DropTheMan/Levels/level_1.json
```

is discovered from:

```text
DropTheMan/Levels
```

An empty path is rejected so a catalog cannot accidentally absorb every text asset from every
`Resources` folder in the project.

`Resources.LoadAll` return order is not authoritative. The catalog always orders successful entries
by metadata supplied through the game-module adapter.

## Public Contract Shape

The game module supplies a metadata reader:

```csharp
public interface ILevelCatalogMetadataReader
{
    bool TryReadMetadata(
        TextAsset levelAsset,
        out LevelCatalogMetadata metadata,
        out string failureReason);
}
```

Generic metadata contains:

* `LevelId`: stable authored-content identity
* `SequenceNumber`: positive integer used only for deterministic catalog ordering

The result exposes either:

* a valid immutable catalog and zero or more warnings
* a failure reason and no catalog

## Validation Ownership

Framework catalog validation is intentionally minimal.

The framework rejects:

* missing metadata readers
* empty Resources paths
* folders with no text assets
* null assets
* metadata-reader failures
* empty level identifiers
* non-positive sequence numbers
* duplicate identifiers, compared case-insensitively
* duplicate sequence numbers

The framework reports but does not reject:

* a sequence that starts after `1`
* missing numbers between ordered entries

Game-module readers may reject malformed JSON or unsupported puzzle content before returning
metadata. That remains game-module validation, not framework interpretation.

## Runtime Flow

```text
Unity Resources folder
    ->
Resources Level Catalog Loader
    ->
Game-module metadata reader for each opaque TextAsset
    ->
Framework generic validation and ordering
    ->
Immutable LevelCatalog
    ->
Game-module bootstrapper selects an entry
    ->
Existing game-module load and runtime-construction path
```

The catalog ends at asset selection. It does not construct runtime objects.

## Relationship With Progression

Catalog sequence and player progression are separate.

The catalog says which shipped levels exist and how they are ordered. Progression later says which
levels the player has completed or unlocked. The catalog must not read or write player state.

## Failure Policy

Catalog construction is atomic. If one discovered asset is invalid or creates an ambiguous duplicate,
the entire catalog build fails with the asset name and reason. Silently skipping bad shipped content
would make level availability depend on hidden import errors.

Sequence gaps are non-ambiguous, so they remain warnings. The next entry after `Level 2` may be
`Level 4` when that is the next valid ordered asset.

## Deferred Scope

The MVP does not include:

* Addressables
* remote content
* downloadable level packs
* arbitrary runtime filesystem discovery
* hot reload
* player progression persistence
* level unlock rules
* puzzle-specific JSON interpretation in the framework
* runtime object construction

The catalog contracts should allow a future asset source to replace Resources discovery without
changing metadata ownership or deterministic ordering rules.

## Final Design Rule

The Level Catalog System discovers and orders opaque authored level assets.

Game modules identify and validate their own content. Runtime Construction Systems build selected
content. Progression Systems own player state.
