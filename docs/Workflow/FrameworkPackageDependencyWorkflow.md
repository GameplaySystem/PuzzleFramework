# Framework Package Dependency Workflow

## Purpose

This workflow defines how Unity prototype repositories consume `PuzzleFramework` without depending
on a developer's filesystem layout.

It exists to ensure that:

* prototypes can be cloned and opened without placing repositories in matching parent folders
* every committed prototype revision resolves the same framework revision
* framework changes are available remotely before a prototype references them
* package updates remain reviewable and reversible

---

## Approved Dependency Form

Committed prototype manifests must use a Git dependency pinned to a full 40-character commit SHA:

```json
"com.gaming.puzzleframework": "https://github.com/GameplaySystem/PuzzleFramework.git?path=/Packages/com.gaming.puzzleframework#<full-commit-sha>"
```

The `path` query points Unity Package Manager to the package manifest inside the framework
repository. The revision fragment comes after the query.

Do not commit:

* relative or absolute `file:` dependencies
* machine-specific paths
* an unpinned Git URL
* mutable branch pins such as `#main`
* shortened commit hashes

A full immutable commit pin makes a prototype checkout reproducible even after framework `main`
moves forward.

---

## Consumer Prerequisites

Every machine opening a prototype must have:

* Git installed and available to Unity
* access to the `GameplaySystem/PuzzleFramework` repository
* working GitHub credentials for HTTPS if the repository is private

The framework repository does not need to be cloned locally for normal prototype consumption.

Validate access outside Unity with `git ls-remote` if Package Manager reports an authentication
failure. Never put a personal access token, password, or other credential inside a committed
package URL.

---

## Framework-First Update Sequence

When a prototype needs unpushed framework changes, use this order:

1. Verify framework and consumer worktrees before editing.
2. Implement and verify the framework change in `PuzzleFramework`.
3. Commit the framework change in a coherent framework commit.
4. Push the framework commit to its remote.
5. Verify the commit exists on the remote.
6. Capture the full pushed SHA with `git rev-parse HEAD`.
7. Update the prototype's `Packages/manifest.json` to that SHA.
8. Let Unity Package Manager resolve the dependency and update `Packages/packages-lock.json`.
9. Verify the prototype compiles and run the task-relevant tests or Play Mode checks.
10. Commit `manifest.json`, `packages-lock.json`, and the dependent prototype changes together or
    in intentionally ordered consumer commits.
11. Push the prototype only after its pinned framework commit is remotely available.

Never commit or push a prototype revision that references a framework commit which exists only in
a local clone.

---

## Prototype-Only Change Sequence

If a task does not change framework code or require a newer framework revision:

* keep the existing framework SHA unchanged
* do not refresh the pin merely because framework `main` advanced
* commit and push the prototype normally after its own verification

Each prototype may intentionally remain on a different known-good framework commit.

---

## Lock File Rule

`Packages/manifest.json` declares the requested framework revision.

`Packages/packages-lock.json` records Unity's resolved Git dependency.

Rules:

* commit both files when the framework pin changes
* prefer Unity Package Manager to regenerate the lock entry
* verify the resolved lock revision matches the manifest pin
* do not delete or hand-edit the entire lock file to hide unrelated package changes
* review lock-file changes for unexpected dependency updates before committing

---

## Temporary Local Development Override

A framework developer may temporarily replace the Git dependency with a local `file:` dependency
for rapid iteration.

That override is local-only:

* do not commit it
* do not push it
* restore the approved Git URL and pinned SHA before final prototype verification
* regenerate and review `packages-lock.json` after restoring the Git dependency

If a local override repeatedly leaks into commits, stop using it and work through pushed framework
commits instead.

---

## Updating Multiple Prototypes

Framework changes do not automatically require every prototype to update.

For each consumer that adopts a new framework revision:

1. update its manifest pin explicitly
2. resolve its lock file independently
3. compile and test that prototype independently
4. commit and push that prototype independently

This prevents one framework push from silently changing every game.

---

## Rollback

If a framework update breaks a prototype:

1. restore the previous known-good full SHA in `manifest.json`
2. let Unity re-resolve `packages-lock.json`
3. verify the prototype
4. commit the rollback normally

Do not rewrite framework or prototype history to roll back a package pin.

---

## Final Rule

Framework first, consumer second.

The framework commit must be pushed before any prototype is committed or pushed with a dependency
on that commit. Every committed consumer must use a remote, immutable, full-SHA package reference.
