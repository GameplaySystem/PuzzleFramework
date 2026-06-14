# Level Editor Foundation

## Document Metadata

Category:
- Content Systems

Status:
- Approved

Parent:
- Overview.md

Related Documents:
- LevelDataSystem.md
- LevelSaveLoadSystem.md
- ../RuntimeConstructionSystems/Overview.md

Depends On:
- Level Data System
- Level Save Load System

Used By:
- Drop Away
- Color Block Jam
- Sky Rush
- Hole People
- Bus Jam

## Purpose

The Level Editor Foundation provides generic editor capabilities for creating and editing `LevelDefinition` data.

It exists to give the framework a reusable editor shell and a small set of extension points without embedding puzzle-specific authoring logic inside shared editor code.

The Level Editor Foundation is not responsible for puzzle-specific authoring tools.

---

# Core Design Idea

The approved editor direction is:

* framework provides the editor foundation
* game modules provide editor-specific adapters or extensions

Conceptually:

```text
Framework Editor
    ↓
Editor Extension Contract
    ↓
Game Module Editor Tools
```

Examples of game-module tools:

* Drop Away Editor Tools
* Color Block Jam Editor Tools
* Sky Rush Editor Tools
* Hole People Editor Tools
* Bus Jam Editor Tools

The framework editor must not know about these concrete game tools.

This is the right balance for now because the project needs reuse across five target games, but it does not need a full Unity Asset Store-grade plugin framework.

---

# Responsibilities

The Level Editor Foundation is responsible for:

* Grid visualization
* Cell visualization
* Active cell editing
* Inactive cell editing
* Blocked cell editing
* Generic shape placement support
* Level metadata editing
* Save integration
* Load integration
* Validation hook support
* Tool registration support
* Editor extension support
* Selection support
* Placement preview support

The framework editor owns generic editing mechanics.

Game modules own puzzle-specific interpretation.

---

# Should Not Handle

The Level Editor Foundation should not handle:

* Hole placement logic
* Stickman placement logic
* Bus placement logic
* Door placement logic
* Brick placement logic
* Puzzle-specific validation
* Puzzle-specific win/loss rules
* Runtime object spawning
* Runtime object initialization
* Runtime level construction

The editor foundation may support generic placement workflows.

It must not know the gameplay meaning of the thing being placed.

---

# Editor Extension Model

The approved architecture is:

```text
Framework Editor
    ↓
Editor Extension Contract
    ↓
Game Module Editor Tools
```

The framework provides:

* the shared editor shell
* generic editing workflows
* extension registration points
* shared validation hooks

Game modules provide:

* puzzle-specific editor tools
* puzzle-specific placement logic
* puzzle-specific content inspectors
* puzzle-specific authoring validation where needed

Useful conceptual framing:

* framework provides the socket or contract
* game module provides the adapter

This should remain lightweight.

The current project does not need a deeply layered plugin ecosystem.

It only needs enough extension capability to support the five target games cleanly.

---

# Generic Editing Capabilities

The framework editor may support generic placement and editing workflows.

Examples:

* place object
* move object
* remove object
* rotate shape (editor only)

Other shared generic capabilities may include:

* selecting cells
* selecting placed items
* previewing placement
* editing generic metadata

Important boundary:

The framework editor may support how something is placed generically.

It should not know what the placed thing means.

For example:

* generic shape placement support belongs here
* hole-specific behavior does not
* bus-specific meaning does not
* brick-specific rule interpretation does not

---

# Validation Philosophy

The framework editor may perform:

* structural validation
* missing field validation
* invalid cell coordinate checks
* invalid board bounds checks

These are generic content-structure concerns.

The framework editor must not perform:

* puzzle-specific validation
* game-specific success checks

Those belong to:

* game module editor extensions
* future validation systems where appropriate

This boundary matters because editor validation often expands too easily into gameplay logic.

The framework should validate structure.

Game modules should validate puzzle meaning.

---

# Relationship With Level Data System

The Level Editor Foundation creates and edits `LevelDefinition` data.

Conceptually:

```text
Level Editor Foundation
    ↓ creates/edits
LevelDefinition
```

The Level Data System owns the structure of the level model.

The Level Editor Foundation provides the editor-facing capabilities to author that structure.

Boundary:

* `LevelDataSystem` defines what the data model is
* `LevelEditorFoundation` defines how users and tools edit that data model

The editor should not redefine the structure owned by the Level Data System.

---

# Relationship With Level Save Load System

The Level Save Load System persists authored level data.

Conceptually:

```text
Level Editor Foundation
    ↓ creates/edits
LevelDefinition
    ↓ persisted by
Level Save Load System
    ↓
JSON
```

The editor may integrate with save and load workflows.

It should not absorb persistence responsibility itself.

Boundary:

* editor edits level data
* save/load persists level data

---

# Relationship With Runtime Construction Systems

The editor creates `LevelDefinition` data.

The editor does not create playable runtime levels.

Runtime Construction Systems are responsible for converting loaded level data into runtime objects.

Conceptually:

```text
Level Editor Foundation
    ↓ creates/edits
LevelDefinition

Level Save Load System
    ↓ persists
JSON

Runtime Construction Systems
    ↓ consume
Loaded LevelDefinition

Game Modules
    ↓ interpret
Game-specific content
```

This is a major architecture boundary.

If the editor starts spawning playable runtime content directly as part of its shared responsibility, it will blur into runtime construction and puzzle-specific tooling.

That belongs elsewhere.

---

# Example Workflow

One expected authoring flow is:

```text
Designer opens framework editor shell
    ↓
Designer edits board and generic metadata
    ↓
Game module editor tools expose puzzle-specific authoring controls
    ↓
Editor produces LevelDefinition
    ↓
Level Save Load System saves LevelDefinition as JSON
    ↓
Runtime Construction Systems later load and build runtime objects
```

This flow keeps responsibilities separated:

* editor authors data
* save/load persists data
* runtime construction builds runtime objects
* game modules define puzzle-specific authoring meaning

---

# Edge Cases

## Extensibility Overbuilt Too Early

Risk:

The editor foundation becomes an abstraction-heavy plugin framework before the five target games actually require that complexity.

Rule:

Only introduce extension points that support known shared needs across the target games.

Do not design for hypothetical third-party plugin ecosystems.

---

## Shared Editor Shell Becomes Puzzle-Specific

Risk:

Convenient game-specific tools are added into the framework shell because they are useful for one game right now.

Rule:

If the tool requires puzzle-specific nouns or puzzle-specific rules, it belongs in a game module extension, not in framework editor code.

---

## Validation Drifts Into Game Rules

Risk:

Generic editor validation expands into “is this level good?” instead of “is this structure valid?”

Rule:

Framework editor validates structure.

Game modules validate puzzle-specific meaning.

---

## Preview Flows Start Building Runtime State

Risk:

Placement previews or editor tooling accidentally become runtime construction paths.

Rule:

Editor previews should remain authoring-time support, not runtime level-building ownership.

Runtime Construction Systems remain responsible for converting loaded level data into runtime objects.

---

## Shape Editing Scope Expands Too Far

Risk:

Generic shape editing begins to assume puzzle-specific semantics.

Rule:

Shared shape manipulation is acceptable only while it remains generic authoring behavior.

Game-specific meaning belongs in extensions.

---

# MVP Scope

The first version of the Level Editor Foundation should support:

* A reusable framework editor shell
* Grid visualization
* Cell visualization
* Active, inactive, and blocked cell editing
* Generic shape placement support
* Selection support
* Placement preview support
* Level metadata editing
* Save integration
* Load integration
* Lightweight extension points for game module tools
* Basic structural validation hooks

The MVP should not attempt to become a general-purpose editor framework beyond the needs of the five target games.

---

# Future Extensions

The following can be added later:

* undo and redo
* copy and paste
* bulk editing
* editor shortcuts
* validation reports
* custom editor windows
* import and export tools

These are future extensions only.

They should not drive the initial architecture before concrete reuse needs appear.

---

# Final Design Rule

The Level Editor Foundation provides generic editor capabilities for authoring `LevelDefinition` data.

Framework provides the editor foundation.

Game modules provide editor-specific adapters and extensions.

The framework editor must remain generic.

It must not absorb puzzle-specific tools, runtime construction, or gameplay logic.
