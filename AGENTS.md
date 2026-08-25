# AGENTS.md

## 1. Project Overview

This repository is a Unity Puzzle Framework project.

The goal is to build a reusable, scalable, and well-documented framework by deconstructing and recreating multiple successful puzzle games, then extracting the systems they share.

The repository currently prioritizes:

- architecture before implementation
- documentation before coding
- reusable framework systems
- portfolio-quality engineering practices

Primary source-of-truth documents:

- `docs/PROJECT_STATE.md`
- `docs/FrameworkSystems/**`
- `docs/Workflow/FrameworkPackageDependencyWorkflow.md`

Codex must treat those documents as the current design baseline unless the user explicitly changes them.

## 2. Architecture Rules

- Framework code must remain game-agnostic.
- Documentation decisions should happen before implementation.
- Do not implement systems unless their markdown spec is approved.
- A system belongs in the Framework only if it is intended to be shared across at least two games.
- Framework systems should define generic mechanics, data flow, and reusable contracts.
- Game modules should define puzzle-specific meaning, rules, and behaviors.
- Prefer interface-based boundaries and data-driven requests/results over hard-coded object coupling.
- When implementing framework systems, prefer small testable pure C# classes where possible before MonoBehaviour-specific behavior.
- Do not introduce puzzle-specific logic into shared framework systems.

## 3. Framework vs GameModules Separation

Framework code:

- may define reusable systems such as board, occupancy, shape, input, drag, snap, pathfinding, queues, buffers, and capacity
- may define interfaces, requests, results, and shared abstractions
- must not depend on game-specific assemblies, folders, classes, or nouns

GameModules code:

- may depend on Framework
- may implement game-specific rules, entities, and behaviors
- may translate framework capabilities into concrete puzzle logic

Dependency direction:

- `GameModules -> Framework` is allowed
- `Framework -> GameModules` is not allowed

Game-specific nouns such as `Hole`, `Bus`, `Door`, `Stickman`, and `Brick` must not appear inside Framework code.

## 4. Implementation Validation Rules

When implementing framework systems, validation must remain inside the ownership boundary of the system being implemented unless a documented architecture decision explicitly overrides that rule.

Examples:

- Content Systems perform schema and persistence validation.
- Runtime Construction Validation System performs build-safety validation.
- Gameplay systems perform gameplay-rule validation.

Do not move validation responsibilities between categories without an approved architecture change.

### Validation Ownership Rule

Allowed:

- `LevelSaveLoad` validates JSON shape, required fields, serialization integrity, and basic schema correctness.

Not allowed:

- `LevelSaveLoad` validates runtime construction readiness.
- `LevelSaveLoad` validates puzzle solvability.
- `LevelSaveLoad` validates gameplay outcomes.

### Duplicate Data Validation Rule

Whenever authored data contains coordinate-based, identifier-based, or key-based collections, implementation should consider duplicate-entry validation.

Examples:

- duplicate board coordinates
- duplicate ids
- duplicate registration keys

If duplicates are invalid for the owning system, validation should reject them explicitly rather than relying on downstream behavior.

### Opaque Payload Rule

Framework-owned systems must not inspect or interpret game-module payload content unless the architecture explicitly documents that responsibility.

Allowed:

- storing payload json
- loading payload json
- transporting payload data

Not allowed:

- interpreting `Hole` data
- interpreting `Stickman` data
- interpreting `Bus` data
- interpreting `Door` data
- interpreting puzzle-specific rules

Framework transports payloads.

Game modules interpret payloads.

### Minimal Validation Principle

Implement only the validation required by the ownership boundary.

Do not expand validation into neighboring architectural categories simply because the data is available.

Examples:

- persistence validation should not become construction validation
- construction validation should not become gameplay validation
- gameplay validation should not become progression validation

Each system validates only what it owns.

## 5. Documentation Workflow

### Daily Milestone Workflow

- Follow `docs/Workflow/DailyMilestoneWorkflow.md` for project-day planning and reporting across both repositories.
- At the beginning of a project day, establish and discuss one realistic daily milestone before non-trivial implementation.
- Continue later tasks against the established daily plan; re-plan only when scope materially changes or a blocker invalidates it.
- At the user's end-of-day signal, create or update `docs/DailyReports/YYYY-MM-DD.md` using the canonical report template.
- Never invent work-time or Codex-usage values. Record `Not tracked`, `User estimate needed`, or `Unavailable` when appropriate.

- Read the relevant markdown specs before proposing or implementing a system.
- Treat `docs/PROJECT_STATE.md` as the project status reference.
- Treat each system markdown file under `docs/FrameworkSystems/**` as the design spec for that system.
- After each meaningful implementation step, update `docs/PROJECT_STATE.md` so current progress, current focus, and next implementation context stay accurate for future handoff.
- After each implementation step, update `docs/IMPLEMENTATION_WATCHLIST.md` with any newly discovered caveats, open questions, risks, or follow-up checks that should survive into later handoffs.
- When a watchlist item is resolved by owner feedback or documentation updates, update that item directly so the watchlist does not keep stale open concerns.
- When a critical gameplay or interaction rule assumption is corrected, disputed, or discovered to be misunderstood, record it in `docs/CRITICAL_RULE_CLARIFICATIONS.md` and update the affected design documents in the same task.
- If a system is missing documentation, draft or request the missing spec before implementation.
- If implementation pressure reveals a design gap, update or propose the docs first, then code after approval.
- When implementation changes reveal that documentation is outdated, propose a documentation update in the same task summary.
- Keep category references and architectural ownership consistent across docs when reorganizing documentation.
- Do not silently reinterpret an approved spec during coding.

### Documentation Metadata Rule

Every framework system document must contain a `Document Metadata` section immediately after the title.

Required fields:

- Category
- Status
- Parent
- Related Documents
- Depends On
- Used By

When creating new documentation, maintain metadata consistency.

When moving a document, update related metadata references.

### Documentation Relationship Rule

Every framework system document must maintain relationship metadata.

Required fields:

- Category
- Status
- Parent
- Related Documents
- Depends On
- Used By

When creating a new system:

1. Identify what the system depends on.
2. Identify what depends on the system.
3. Update related documents if relationships change.
4. Prefer explicit relationships over implicit assumptions.

Documentation should form a navigable dependency graph.

## Documentation Preflight Before Implementation

Before implementing any non-trivial task, Codex must identify and read the documentation files relevant to that task.

The preflight should include:

1. List the docs that were read.
2. Summarize the rules, constraints, and decisions from those docs that affect the task.
3. Identify any conflicts, stale assumptions, or gaps between the prompt and the docs.
4. Stop for clarification if the requested implementation would contradict approved documentation.
5. Proceed only after the task scope is aligned with the docs.

Do not read every document blindly. Read the smallest sufficient set of task-relevant docs.

Examples:

- For Drop The Man movement, collection, snap, capacity, or completion tasks, read:
  - `DropAwayPrototype/docs/DropTheManMovementAndCollectionRules.md`
  - `DropAwayPrototype/docs/DropTheManMVPRules.md`
  - `DropAwayPrototype/docs/DropTheManRuntimeIntegrationDesign.md`
  - `DropAwayPrototype/docs/DropTheManPlayableSceneAdapterDesign.md`
  - `PuzzleFramework/docs/CRITICAL_RULE_CLARIFICATIONS.md`
  - `PuzzleFramework/docs/IMPLEMENTATION_WATCHLIST.md`
- For collection presentation timing tasks, also read:
  - `DropAwayPrototype/docs/DropTheManCollectionPresentationTimingDesign.md`
- For framework-level system changes, read:
  - the relevant `PuzzleFramework/docs/FrameworkSystems/**` document
  - `PuzzleFramework/docs/FrameworkArchitecture.md`
  - `PuzzleFramework/docs/ImplementationRoadmap/FrameworkMVPPlan.md`
  - `PuzzleFramework/docs/IMPLEMENTATION_WATCHLIST.md`
- For content, JSON, save/load, or editor tasks, read:
  - relevant Content Systems docs
  - relevant Runtime Construction docs
  - Drop The Man level/content docs
  - `PuzzleFramework/docs/IMPLEMENTATION_WATCHLIST.md`

Documentation is the source of truth. If code and docs disagree, report the drift before changing behavior.

## 6. Unity Project Rules

- Do not modify Unity assets or scripts unless the user asks for code or content changes.
- Do not modify Unity-generated folders such as `Library`, `Temp`, `Obj`, `Logs`, or `UserSettings`.
- Do not edit generated project noise unless explicitly required.
- Do not add third-party packages without approval.
- Do not change project-wide Unity settings, package manifests, render pipeline settings, or input actions without approval unless the user explicitly requested that exact change.
- Prefer changes inside intentional source locations, not generated or cached directories.

## 7. Naming Conventions

- Use clear, literal names that match the docs.
- Framework system names should stay generic: `GridSystem`, `CellOccupancySystem`, `ShapeSystem`, `InputSystem`, `DragMovementSystem`, `GridSnapSystem`, `PathfindingSystem`.
- Use `Framework` terminology for shared systems and `GameModules` terminology for puzzle-specific systems.
- Requests/results should use explicit suffixes like `Request`, `Result`, `Data`, `Config`, or `State` when appropriate.
- Interfaces should use `I` prefixes, such as `ISelectable`, `IDraggable`, and `IClickable`.
- Avoid introducing puzzle-specific nouns into framework namespaces, folders, classes, or interfaces.

## 8. Code Documentation Rules

- Code should be documented inside scripts well enough that a reviewer can understand ownership, intent, and data flow without reconstructing everything from scratch.
- Public framework contracts should prefer XML documentation comments where useful, especially for interfaces, services, requests, results, and shared data models.
- Non-obvious implementation logic should include concise intent-focused comments explaining why the code exists or what invariant it protects.
- Comments should explain purpose, boundaries, assumptions, or invariants, not restate obvious line-by-line behavior.
- Documentation inside scripts must remain consistent with the approved markdown architecture docs and must not silently redefine system responsibilities.

## 9. Implementation Handoff Requirement

After every implementation task, provide a maintainer-focused handoff.

The handoff must explain the implementation clearly enough that the project owner can maintain and defend the code later.

Documentation-only tasks may keep the simpler changed-files summary.

Implementation tasks must include the full maintainer handoff.

Every implementation handoff must include:

### 0. Preflight

Report:

- documents read
- rules, constraints, and decisions that affected the implementation
- conflicts, stale assumptions, or gaps discovered before implementation

### 1. What changed

List changed files and summarize what each file now owns.

### 2. Public contracts introduced

List new public types, interfaces, methods, enums, or data structures.

For each public contract explain:

- why it exists
- who is expected to use it
- what should not use it

### 3. Ownership boundaries preserved

Explain how the implementation respects framework boundaries.

Explicitly mention if the task avoided:

- game-specific nouns
- gameplay rules
- presentation logic
- persistence drift
- runtime construction drift
- service locator or singleton shortcuts

### 4. Key implementation decisions

Explain important code decisions and tradeoffs.

Include:

- why this structure was chosen
- alternatives that were intentionally avoided
- assumptions made

### 5. Dependency impact

Explain what other systems can now depend on this slice.

Also explain what would break or become blocked if this slice were removed.

### 6. What is intentionally not implemented

List deferred items and why they were not included.

### 7. Verification

Report:

- compile status
- test status if any
- manual verification performed
- known limitations
- any docs updated
- any unresolved risks
- whether unrelated dirty files were left untouched

### 8. Suggested next step

Recommend the next smallest implementation step.

Do not jump ahead beyond the approved roadmap.

## 10. Git And Commit Expectations

- Do not commit changes automatically.
- Show a summary or diff before any commit.
- Keep commits scoped to one coherent change.
- Avoid mixing documentation restructuring, architecture changes, and implementation work in the same commit unless the user asked for that grouping.
- Do not rewrite history, force-push, or clean unrelated changes without explicit permission.
- Assume the worktree may contain user changes; do not revert unrelated work.
- After the user explicitly approves or requests commit/push work, Codex should handle the remaining git workflow without repeated prompting.
- Codex should choose the correct commit order, commit batches, and commit messages based on architectural boundaries and change scope.
- Codex should push the approved commits when requested, while still excluding unrelated local changes from staging.

### Framework Package Dependency Workflow

- Follow `docs/Workflow/FrameworkPackageDependencyWorkflow.md` whenever a prototype consumes a new framework revision.
- Commit and push framework changes before updating a consuming prototype's package pin.
- Verify the framework commit exists remotely, then use its full 40-character SHA in the prototype manifest.
- Committed prototype manifests must use the Git package URL and package subfolder path; do not commit local `file:` dependencies or mutable branch pins.
- Commit the consumer's `Packages/manifest.json` and resolved `Packages/packages-lock.json` when its framework pin changes.
- Compile and test each consumer after resolving a new framework revision and before committing or pushing it.
- A temporary local package override must remain uncommitted and must be restored to the pinned Git dependency before final verification.

## 11. What Codex Must Ask Approval For

Codex must ask approval before:

- adding third-party Unity packages or external dependencies
- changing `Packages/manifest.json`
- changing project-wide Unity settings
- modifying input mappings or render pipeline setup
- implementing a system whose markdown spec is missing or not approved
- making architectural changes that contradict existing docs
- committing changes
- creating branches, pushing, opening PRs, or performing remote git operations
- deleting files or making destructive repository changes

## 12. What Codex Must Never Do Without Explicit Permission

- Never make Framework depend on GameModules.
- Never place game-specific nouns such as `Hole`, `Bus`, `Door`, `Stickman`, or `Brick` inside Framework code.
- Never implement undocumented or unapproved systems as if the design were settled.
- Never modify `Library`, `Temp`, `Obj`, `Logs`, or `UserSettings`.
- Never add third-party packages automatically.
- Never commit automatically.
- Never move or delete user work unless explicitly instructed.
- Never rewrite or discard unrelated local changes.
- Never commit or push a prototype that references an unpushed framework commit or a machine-specific framework path.
