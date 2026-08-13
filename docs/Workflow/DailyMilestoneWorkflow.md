# Daily Milestone And Reporting Workflow

## Purpose

This workflow coordinates daily work across the `PuzzleFramework` and `DropAwayPrototype` repositories.

It exists to:

- set a realistic direction before implementation begins
- compare planned work with actual progress
- record time and Codex usage without inventing data
- identify workflow problems and improve delivery efficiency over time

Daily planning and reporting are project-level activities. Daily reports belong in:

```text
PuzzleFramework/docs/DailyReports/YYYY-MM-DD.md
```

Do not create an empty report merely because a task starts. Create the report when the user asks to record the day or end the project day.

## Start-Of-Day Milestone Planning

At the beginning of each project day, before non-trivial implementation, Codex must help establish one daily milestone plan.

The plan must be discussed with the user before implementation begins. A detailed user-authored task may supply most of the plan, but Codex must still summarize the proposed day scope, identify conflicts or unrealistic expectations, and confirm that the direction is aligned.

Once the daily plan is established, later tasks that day may continue against it without repeating the full planning ceremony. Re-plan only when scope materially changes, a blocker invalidates the plan, or the user explicitly changes direction.

The milestone must be achievable in one project day and should prefer one coherent deliverable over a long list of unrelated tasks.

Record:

- date
- project area
- main milestone theme
- primary goal
- secondary or stretch goals
- explicitly out-of-scope work
- relevant documentation to read
- expected Codex task sequence
- expected manual Unity or user checks
- definition of done
- risk level
- estimated scope

Do not begin non-trivial file changes when the day's milestone is still ambiguous or conflicts with approved documentation. Read-only investigation may be used to clarify the plan.

## Daily Milestone Template

```markdown
# Daily Milestone Plan - YYYY-MM-DD

## Project Area

<!-- Framework, Drop The Man prototype, content pipeline, workflow, etc. -->

## Main Theme

<!-- One sentence describing the day's coherent milestone. -->

## Primary Goal

<!-- The outcome that must be complete today. -->

## Secondary / Stretch Goals

- <!-- Optional goal that may be attempted only after the primary goal. -->

## Out Of Scope

- <!-- Work deliberately excluded from today. -->

## Relevant Docs

- <!-- Smallest sufficient documentation set. -->

## Planned Codex Task Sequence

1. <!-- First scoped task. -->
2. <!-- Next scoped task. -->

## Manual Checks Needed

- <!-- Unity playtest or owner verification. -->

## Definition Of Done

- <!-- Observable completion criteria. -->

## Risk Level

<!-- Low / Medium / High, with a short reason. -->

## Estimated Scope

<!-- Small / Medium / Large, with assumptions. -->
```

## Task Execution During The Day

Existing project rules remain in force:

- perform the documentation preflight before non-trivial implementation
- keep implementation tasks small and scoped
- provide clear maintainer handoffs
- do not modify unrelated files
- do not broaden architecture without explicit approval
- preserve unrelated dirty files across both repositories

Each task handoff must continue to report:

- documents read
- files changed
- root cause or design reason
- verification performed
- documentation updates
- unresolved risks
- final repository state
- whether unrelated dirty files were left untouched

When a task changes the daily plan, record the change rather than silently redefining the milestone.

## End-Of-Day Report

When the user indicates that the project day is ending, Codex should help create or update that day's report in `PuzzleFramework/docs/DailyReports/YYYY-MM-DD.md`.

The report should compare the plan with actual results and include:

- date
- total work time
- approximate active implementation time
- approximate planning and discussion time
- Codex limit or usage consumed, if available or supplied by the user
- milestones planned, completed, partially completed, not started
- commits created
- repositories touched
- files or systems changed
- Unity and manual tests performed
- bugs found and fixed
- open blockers
- design decisions made
- workflow problems noticed
- efficiency notes
- recommended next-day starting point

Do not infer precise time or Codex usage from chat length, commit timestamps, file timestamps, or token guesses. Use one of these explicit values when the data is unavailable:

```text
Not tracked
User estimate needed
Unavailable
```

If Codex can access a product-provided usage value, label its source and whether it is exact or approximate.

## Daily Report Template

```markdown
# Daily Report - YYYY-MM-DD

## Daily Milestone Plan

- Project area:
- Main theme:
- Primary goal:
- Secondary / stretch goals:
- Out of scope:
- Definition of done:
- Risk level:
- Estimated scope:

## Work Sessions

- Session 1:

## Completed Work

- <!-- Completed milestone or task. -->

## Partial / Unfinished Work

- <!-- Partial milestone and exact remaining work. -->

## Not Started

- <!-- Planned work not started. -->

## Commits

- Repository:
- Commit:

## Repositories And Systems Touched

- <!-- Repo, files, and systems changed. -->

## Verification

- Unity compile:
- Automated tests:
- Manual playtest:

## Bugs Found And Fixed

- Found:
- Fixed:

## Design Decisions

- <!-- Decisions that should survive into future handoffs. -->

## Codex Usage / Limits

- Usage consumed: Unavailable
- Source: Unavailable

## Time Tracking

- Total work time: User estimate needed
- Active implementation time: Not tracked
- Planning / discussion time: Not tracked

## Problems / Blockers

- <!-- Open blockers or unresolved risks. -->

## Workflow Efficiency Notes

- <!-- What accelerated or slowed delivery. -->

## Next-Day Recommended Start

- <!-- Smallest safe first task for the next project day. -->
```

## Efficiency Review

Daily data should be used to improve planning quality, not to reward raw output volume.

Review trends such as:

- planned milestones completed versus deferred
- time lost to unclear requirements or stale documentation
- compile and playtest feedback latency
- frequency of unplanned fixes
- commit scope quality
- repeated manual steps that may deserve later automation
- whether Codex usage produced a proportional verified outcome

Do not optimize by skipping documentation, verification, ownership boundaries, or user review.
