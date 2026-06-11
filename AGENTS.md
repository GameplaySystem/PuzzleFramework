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

## 4. Documentation Workflow

- Read the relevant markdown specs before proposing or implementing a system.
- Treat `docs/PROJECT_STATE.md` as the project status reference.
- Treat each system markdown file under `docs/FrameworkSystems/**` as the design spec for that system.
- If a system is missing documentation, draft or request the missing spec before implementation.
- If implementation pressure reveals a design gap, update or propose the docs first, then code after approval.
- When implementation changes reveal that documentation is outdated, propose a documentation update in the same task summary.
- Keep category references and architectural ownership consistent across docs when reorganizing documentation.
- Do not silently reinterpret an approved spec during coding.

## 5. Unity Project Rules

- Do not modify Unity assets or scripts unless the user asks for code or content changes.
- Do not modify Unity-generated folders such as `Library`, `Temp`, `Obj`, `Logs`, or `UserSettings`.
- Do not edit generated project noise unless explicitly required.
- Do not add third-party packages without approval.
- Do not change project-wide Unity settings, package manifests, render pipeline settings, or input actions without approval unless the user explicitly requested that exact change.
- Prefer changes inside intentional source locations, not generated or cached directories.

## 6. Naming Conventions

- Use clear, literal names that match the docs.
- Framework system names should stay generic: `GridSystem`, `CellOccupancySystem`, `ShapeSystem`, `InputSystem`, `DragMovementSystem`, `GridSnapSystem`, `PathfindingSystem`.
- Use `Framework` terminology for shared systems and `GameModules` terminology for puzzle-specific systems.
- Requests/results should use explicit suffixes like `Request`, `Result`, `Data`, `Config`, or `State` when appropriate.
- Interfaces should use `I` prefixes, such as `ISelectable`, `IDraggable`, and `IClickable`.
- Avoid introducing puzzle-specific nouns into framework namespaces, folders, classes, or interfaces.

## 7. Git And Commit Expectations

- Do not commit changes automatically.
- Show a summary or diff before any commit.
- Keep commits scoped to one coherent change.
- Avoid mixing documentation restructuring, architecture changes, and implementation work in the same commit unless the user asked for that grouping.
- Do not rewrite history, force-push, or clean unrelated changes without explicit permission.
- Assume the worktree may contain user changes; do not revert unrelated work.

## 8. What Codex Must Ask Approval For

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

## 9. What Codex Must Never Do Without Explicit Permission

- Never make Framework depend on GameModules.
- Never place game-specific nouns such as `Hole`, `Bus`, `Door`, `Stickman`, or `Brick` inside Framework code.
- Never implement undocumented or unapproved systems as if the design were settled.
- Never modify `Library`, `Temp`, `Obj`, `Logs`, or `UserSettings`.
- Never add third-party packages automatically.
- Never commit automatically.
- Never move or delete user work unless explicitly instructed.
- Never rewrite or discard unrelated local changes.
