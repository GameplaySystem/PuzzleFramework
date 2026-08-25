# Puzzle Framework

This package contains the reusable, game-agnostic runtime code for the Puzzle Framework project.

It is intended to be consumed by separate prototype Unity projects through a Git dependency pinned
to a full framework commit SHA and the `/Packages/com.gaming.puzzleframework` repository subfolder.

```json
"com.gaming.puzzleframework": "https://github.com/Find-Games/PuzzleFramework.git?path=/Packages/com.gaming.puzzleframework#<full-commit-sha>"
```

The canonical update and framework-first push sequence is documented in
`docs/Workflow/FrameworkPackageDependencyWorkflow.md` at the repository root.
