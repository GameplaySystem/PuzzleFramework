# Project Structure

## Purpose

This document defines the physical organization strategy for the framework and prototype projects.

It does not define gameplay systems.

It does not define framework architecture.

It defines:

* repository layout
* Unity project layout
* framework package strategy
* prototype project strategy
* local package dependency workflow
* portfolio presentation structure
* future release strategy

The goal is to keep the framework clean, reusable, and easy to inspect while allowing each prototype to stand on its own.

---

# Core Structure Rule

The portfolio should present:

```text
Reusable framework
+
separate prototype games built on top of it
```

The project should not become one giant Unity project containing every prototype.

That physical organization would make the framework harder to inspect, harder to package, and harder to present clearly.

---

# Recommended Repository Layout

Recommended top-level layout:

```text
PuzzlePortfolio/
├── PuzzleFramework/
├── DropAwayPrototype/
├── ColorBlockJamPrototype/
├── SkyRushPrototype/
├── HolePeoplePrototype/
└── BusJamPrototype/
```

This structure separates:

* reusable framework source
* prototype-specific Unity projects

Each project can then be inspected, opened, built, and presented independently.

---

# PuzzleFramework Repository Role

`PuzzleFramework` should contain reusable framework code only.

Its role is to act as:

* the reusable source of truth for shared systems
* the central framework documentation repository
* the package-ready framework project

`PuzzleFramework` must not contain:

* prototype-specific scenes
* prototype-specific art assets
* prototype-specific build settings
* prototype-specific app metadata
* game-specific nouns in shared framework source

The framework repository should stay clean enough that a reviewer can inspect it as a reusable engineering artifact, not as a mixed project dump.

---

# Prototype Repository Role

Each prototype should be its own Unity project.

Examples:

* `DropAwayPrototype`
* `ColorBlockJamPrototype`
* `SkyRushPrototype`
* `HolePeoplePrototype`
* `BusJamPrototype`

Each prototype owns:

* scenes
* game-specific assets
* game-module code
* build settings
* product name
* app icon
* screenshots and video links later
* prototype-specific README content

This keeps each prototype self-contained and portfolio-friendly.

It also allows a reviewer to inspect one prototype without wading through every other game.

---

# Unity Project Layout Strategy

## PuzzleFramework

`PuzzleFramework` should be organized so the reusable source can be consumed as a Unity package or package-ready local dependency.

The important structural idea is:

* framework-owned source lives in intentional reusable locations
* framework documentation lives in the framework repository
* framework should be package-oriented rather than prototype-oriented

Recommended direction:

```text
PuzzleFramework/
├── docs/
├── Packages/
├── ProjectSettings/            (only if needed for framework development/testing)
└── reusable framework source
```

The exact source folder layout can be refined later during implementation.

What matters now is that the framework repository is treated as package-ready source, not as the home of prototype scenes and product content.

## Prototype Projects

Each prototype Unity project should follow ordinary project structure for:

* scenes
* assets
* game-specific source
* build configuration
* captured media or prototype docs later

Recommended direction:

```text
DropAwayPrototype/
├── Assets/
├── Packages/
├── ProjectSettings/
└── prototype-specific source and content
```

The prototypes should look like normal standalone Unity projects that happen to consume a shared framework.

---

# Framework Package Strategy

`PuzzleFramework` should be treated as a reusable Unity package or package-ready framework source from the beginning of implementation.

That means the implementation strategy should assume:

* the framework will be referenced from outside its own repository
* prototype projects consume it as a dependency
* framework code should be structured for reuse, not for one project’s internal convenience

Initial strategy:

* use local path-based package consumption during development
* keep the framework package-friendly even if publishing comes later

This is enough for early development without prematurely committing to registry infrastructure.

---

# Prototype Project Strategy

Each prototype should validate framework reuse independently.

The prototype projects are not just demos.

They are proof that:

* the framework can be consumed externally
* the framework can support more than one game
* the framework remains separate from product-specific content

That means each prototype should:

* reference the framework externally
* keep product content local to the prototype
* remain buildable as its own Unity project

This also makes future cleanup much easier because prototype-specific content never needs to be untangled from the framework repository.

---

# Local Package Dependency Workflow

During development, each prototype should reference `PuzzleFramework` through a local package or path dependency.

Conceptually:

```text
Prototype Project
    ->
local package reference
    ->
PuzzleFramework
```

Recommended workflow:

1. Keep `PuzzleFramework` in its own repository or project root.
2. Create a separate Unity project for a prototype.
3. Reference `PuzzleFramework` locally through Unity package or path-based dependency configuration.
4. Make shared-framework changes in `PuzzleFramework`.
5. Validate those changes from the prototype project consuming it.

This workflow is preferred because it proves the real consumption model early.

It avoids the false confidence that comes from developing framework code only inside one giant local Unity project.

---

# Dependency Rule

Allowed:

```text
Prototype Project
    ->
PuzzleFramework
```

Forbidden:

```text
PuzzleFramework
    ->
Prototype Project
```

Practical meaning:

* prototype projects may depend on the framework
* prototype projects may contain product-specific content and rules
* the framework must remain reusable and independent of any single prototype project

This is the physical project-structure version of the broader dependency rule.

---

# Portfolio Presentation Structure

Separate projects are preferred for portfolio presentation because they provide:

* cleaner presentation
* easier inspection for reviewers
* independent builds per prototype
* clearer evidence of framework reuse
* less confusion than one giant Unity project
* better preparation for future packaging or release

A strong portfolio presentation can show:

```text
PuzzleFramework
-> reusable shared framework

DropAwayPrototype
-> first real consumer

ColorBlockJamPrototype
-> second pressure test

additional prototypes
-> proof of reuse breadth
```

This tells a clearer engineering story than one repository containing mixed framework and product content.

---

# Implementation Impact

This structure changes the early implementation sequence.

Instead of building everything inside one Unity project, the practical order becomes:

```text
1. Prepare framework package/project structure
2. Create DropAwayPrototype Unity project
3. Reference PuzzleFramework locally
4. Implement framework MVP through Drop Away needs
5. Build Drop Away vertical slice
6. Create second prototype project to pressure-test reuse
```

Important rule:

Do not build the whole framework in isolation first.

Build the framework incrementally while proving it through separate prototype projects.

---

# Repository Hygiene Rules

To keep the structure sustainable:

* do not place prototype scenes inside `PuzzleFramework`
* do not place prototype art or product branding inside `PuzzleFramework`
* do not let prototype-only code become framework source by convenience
* do not treat one prototype project as the permanent home of the framework

The framework repository should stay usable even if every prototype project is removed or archived.

Likewise, each prototype should stay understandable as a consumer of the framework rather than a hidden monolith.

---

# Future Release Strategy

Future possibilities include:

* publishing the framework as a Unity package
* converting the local dependency to a Git submodule workflow
* converting the framework to a package registry workflow later
* moving prototype projects into separate repositories if portfolio needs change
* asset store packaging later
* CI or build automation later

These are future options, not MVP requirements.

The current strategy should stay simple:

* local package dependency first
* separate prototype projects
* clean framework repository

---

# Final Structure Rule

Physical project organization should make framework reuse obvious.

`PuzzleFramework` stays reusable and package-oriented.

Each prototype stays a separate Unity project.

Prototype projects consume the framework through local dependency workflow during development.

Framework and prototype content must remain physically separated.
