# Drop The Man Remaining Work

Updated: 2026-09-03, after progression commit/push.

Scope: finish and validate the current Drop The Man prototype, not implement every documented
framework system. This is a prioritized backlog, not approval to implement deferred features.

## Delivered Baseline

Campaign persistence, completed-level replay, sequential replay Next, explicit Resume Campaign,
and configurable post-campaign looping are published in prototype commit
`cee412bc27ff5956819cbfcd35dd165c54fd19bb`. Its framework dependency remains the published
`96e9b7751686f2652c0374a40841e74c96c74c9f`. Actual Unity compilation, 27 prototype Edit Mode tests,
and gameplay startup passed. Framework persistence previously passed its 18 focused tests.

The six cat jump variants, matching rig/model, white material details, moving-socket collection,
delayed fill/completion, concrete editor hole visuals, click-to-rotate, and dynamic camera
positioning are already implemented. Remaining items below are acceptance/polish, not requests
to rebuild those systems.

## 1. Acceptance First

- [ ] Verify save/relaunch in the Editor sandbox: win, stop before Next Level, reopen at the next
  unfinished level; loss, restart and quitting mid-level must not advance progress.
- [ ] Verify replay UI end to end: chain completed levels, join unfinished campaign, use Resume
  Campaign, relaunch during replay, and replay the final shipped level into the saved loop.
  Confirm replay never moves saved progress, including when replay ID equals the loop cursor.
- [ ] Exercise Retry Load/Save using a disposable profile, without erasing the owner's save.
  Confirm visible errors, corrupt-save preservation, dirty retry, and loop range/single-level behavior.
- [ ] Validate all eight hole shapes and quarter turns in gameplay/editor: root alignment, tint,
  every selection collider/layer, rotation rejection at blockers/bounds, sockets, caps, apertures,
  completion, restart and next-level reload. Unrotated visuals are already owner-validated.
- [ ] Finish collection visual acceptance on non-square shapes and different camera angles:
  readable six-clip variety, concurrent cats, moving holes, release during flight and restart
  mid-fall. The same-clip/white-detail defects are resolved; investigate only reproduced regressions.
- [ ] Check camera/centering on odd/even, wide/tall and asymmetric blocked-cell boards, in gameplay
  and editor, across intended aspect ratios and resize/reload. Camera rotation must stay authored.

## 2. Finish The Player Experience

- [ ] Agree on final full-hole closing alignment and cavity shadow/gradient appearance with the
  owner/artist, then apply only the chosen prototype presentation settings.
- [ ] Design and approve real gameplay/result/level-select UI to replace temporary OnGUI controls:
  timer, restart, Next Level, completed replays, Resume Campaign and readable save errors.
  Include touch sizing and an explicit decision about whether menus pause the timer (currently not).
- [ ] Finalize the playable level sequence, stable IDs, difficulty and loop interval; play through
  every shipped level, including the structurally validated Level 3 now published in `a743278`.
  Do not renumber existing IDs without an identity/migration decision.
- [ ] Decide the smallest editor usability pass still needed: real controls once UI assets exist,
  reliable import/export feedback and documented authoring workflow. A gameplay Play/Test bridge
  is a separate scope decision, not a reason to merge editor and runtime ownership now.

## 3. Delivery Checks

- [ ] Make and test a player build on the selected target device: input/UI, aspect ratios, stencil
  ordering/depth/shadows, save permissions/replacement, pause/resume and cold launch.
- [ ] Profile representative large levels and repeated reloads on that device for frame time,
  allocations and retained objects/tweens/animators. Optimize measured issues, not hypothetical ones.
- [ ] Fix the existing framework catalog NUnit assertion (`Has.Count` on an array) in a separate
  scoped task, then rerun the broader suite. Confirm unsupported CountUp content is rejected at
  the correct boundary before expanding timer authoring; do not move gameplay validation into storage.
- [ ] Record the acceptance results, prepare a short demo/setup guide, verify a fresh clone resolves
  the pinned framework, and review remaining local changes into intentional commits.

## Not Required For This Finish

Cloud saves, multiple profiles, rewards/stats, mid-level snapshots, adaptive/random endgame,
boosters/monetization and generalized event/feedback infrastructure remain deferred. Audio/haptics
can be a separately approved polish slice. Reusable editor extraction waits for a second game's
evidence. Start Color Block Jam after closing or explicitly accepting the remaining prototype risks.

## References

- [Project state](PROJECT_STATE.md)
- [Implementation watchlist](IMPLEMENTATION_WATCHLIST.md)
- [Progression design](../../DropAwayPrototype/docs/DropTheManProgressionDesign.md)
- [Progression maintainer handoff](../../DropAwayPrototype/docs/DropTheManProgressionImplementationHandoff.md)
- [Cat integration handoff](../../DropAwayPrototype/docs/DropTheManCatAnimationIntegrationHandoff.md)

Recommended immediate next step: the sandbox save/replay acceptance pass, then decide the player UI
slice. Keep the remaining work small and tied to a visible demo outcome.
