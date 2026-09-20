# Color Block Escape MVP Requirements

Date: 2026-09-20
Status: Approved by owner on 2026-09-20, including the six approval clarifications.
Product: Color Block Escape, the portfolio prototype. Reference: Rollic Games' Color Block Jam.

## Authority and scope

The owner supplied the product rules below on 2026-09-20. They supersede conflicting proposals in
[the earlier preflight](ColorBlockJamArchitecturePreflight.md). The reference game is evidence, not
the authority for this prototype. An unverified reference detail does not displace a chosen MVP rule.

The delivery target is a playable prototype in at most seven development days. That target is a
constraint on scope and sequencing, not evidence that every requested feature already fits it.
Implementation is authorized within the approved framework/game-module boundaries.

**Architecture rule for that target:** put demonstrably reusable infrastructure in
PuzzleFramework and have both Drop The Man and CBE consume it. Keep block/exit meaning and other
game rules in CBE. Save time by reusing correctly scoped systems and excluding speculative
abstractions, unrelated features and polish. Do not choose a CBE-only duplicate solely because
it seems quicker this week, and do not put a CBE-specific rule into the framework merely because
the framework has a matching document title.

## Reference evidence and open research

Publisher [store description](https://play.google.com/store/apps/details?id=com.GybeGames.ColorBlockJam)
describes freely sliding differently shaped blocks around obstacles toward matching doors. Publisher
[help](https://rollic.helpshift.com/hc/en/24-color-block-jam/faq/1222-how-do-i-play-color-block-jam/)
describes clearing all blocks before the timer expires and also names a “no valid matching doors”
failure. The publisher's [obstacle catalog](https://rollic.helpshift.com/hc/en/24-color-block-jam/faq/1234-obstacles-in-color-block-jam/)
confirms that the live reference has special blocks and changing doors; those are outside this MVP.

| Question | Verified reference behavior | Chosen MVP rule / next evidence needed |
| --- | --- | --- |
| Can a door wider than a block's required span accept it? | Not resolved by the inspected publisher descriptions. | **Yes:** width at least the full bounding span. Keep this deliberate rule unless direct play evidence contradicts it and the owner changes it. |
| Which exact footprints occur? | Different shapes are confirmed; no authoritative shape-by-shape catalog was found. | Ship the owner-requested preset set below. Label any later additions “reference-confirmed” only with a captured level/example. |
| What does “no valid matching doors” mean? | Publisher names it, but does not define the trigger or whether it is missing colors, closed/dynamic doors, or unreachable paths. | Timeout is the only MVP failure. Do not add automatic dead-state detection without a defined predicate and separate approval. |

Do not treat similarly named browser games, solver commentary, or screenshots without a verified
publisher/game identity as proof of these three details. A later research note should record app
version and an actual observed level/action before asserting exact behavior.

## Board, blocks and colors

- The board has rectangular coordinate bounds, with every authored cell explicitly Active,
  Inactive, or Blocked. An irregular playable outline is represented by Inactive cells. Blocked
  cells remain structural obstacles. The editor must expose all three states.
- A block has a unique ID, one of the framework's ten `ColorIdentity` values, an origin, and a
  nonempty unique set of occupied-cell offsets. The authored footprint is the runtime authority.
  Meshes, preset names, and gameplay rotation values are not level-data authorities.
- Blocks can have different fixed shapes and orientations. There is no runtime rotation. Editor
  rotation changes the saved offsets of an already placed block if the new footprint fits.
- Initial editor presets: 1×1, 1×2, 1×3, 2×2, 2×3, optionally 3×3 where useful, plus L, T,
  and S/Z variants. Presets expand into explicit offsets. A 4×4 bounding limit is provisional;
  the data format must not hardcode a shape enum or permanently bake in that limit.
- Any level may use any subset of the ten colors. Multiple exits of one color are allowed.

## Movement and boundaries

- One player-controlled block follows free, continuous board-plane dragging, with footprint-aware
  travel checks between accepted positions. Movement is not cell-by-cell. Other blocks, Blocked
  cells, Inactive space, and closed or invalid wall segments prevent passage.
- The block cannot leave the playable board except through an accepted matching exit. A wrong-color
  exit acts as a solid wall. Exits are entered only from the playable side; movement from outside
  back in is never an admission path.
- A block touching or adjacent to a matching exit at level start does not depart automatically.
  Acceptance requires player-driven outward movement.
- The whole footprint, not just its leading cells, determines fit. For a given wall, required span
  is the width of the footprint's full bounding box projected along that wall. Internal gaps in an
  L, T, or S/Z shape do not reduce the required span. A fixed 2×3 block therefore needs width 2
  or 3 depending on which side faces the exit.

## Exit data and acceptance

- Each exit has a unique ID, a Top/Bottom/Left/Right wall side, a starting board-edge cell, an
  integer width, and a color. Its logical opening and visual position are derived from those
  fields; arbitrary world-space door coordinates are not saved.
- An exit must occupy a contiguous run of exposed, exterior-facing edges on one straight wall.
  Its width must fit that run. Authoring/construction validation rejects incompatible overlapping
  exits and invalid openings. Irregular board outlines remain valid; a hole wholly enclosed by
  board cells is not an exterior exit. An edge bordering an internal Inactive or Blocked region
  does not qualify merely because a local edge is exposed.
- An exit accepts a block only when its color matches, the aperture width is at least the block's
  full projected span, the block is sufficiently aligned, and the player has moved it outward
  from inside the board. Mere corner/tiny-portion contact does not qualify.
- Alignment tolerance is configurable. An initial candidate is 70% overlap of the block's
  projected bounding interval with the opening. Acceptance must still have a collision-free
  alignment path and a final position whose entire projected span fits inside the aperture;
  the tolerance is not permission to pass through a jamb.
- At acceptance, player control over that block ends and it aligns into an exit transition.
  One exit processes one block at a time; it remains busy until the entire block completes its
  exit/shredding sequence. Other blocks may use other eligible exits.
- Occupancy is released progressively as logical portions of the exiting footprint clear board
  cells. The block remains an entity until fully beyond the board. Exit progress is derived from
  logical footprint and direction, not mesh collision. Do not silently replace this with instant
  full-footprint release; report cost/fragility before proposing that change.

## Outcomes and presentation

- Win when every block has been accepted for exit. Acceptance of the final block before timer
  expiry, or exactly at the expiry boundary, locks in a win, even while that or another block is
  still in the exit/chipper animation.
  Result UI may wait for presentation, but an animation callback cannot decide or reverse success.
- Timer expiry is the only MVP loss. A block that has not met acceptance by expiry is not
  counted as exited. Restart returns a fresh level; minimal win/loss UI is sufficient.
- Basic Unity geometry or assembled simple visuals are sufficient. Custom FBX assets must not be
  required for logic, construction, editor previews, or prototype completion.
- The chipper is presentation-only. Pooled fragments may use DOTween, but gameplay does not depend
  on DOTween, fragment completion, mesh intersections, or a result-screen callback. Stop and reset
  each fragment's tween before reuse.
- Expose fragment count/subdivision per block cell, size, exit/travel duration, stagger, scatter,
  rotation, easing, lifetime, pool capacity, result-screen delay, and visibility after results as
  tunable presentation parameters. Their final defaults require Unity playtest tuning.

## Required V1 authoring workflow

The editor supports board resize; Active/Inactive/Blocked painting; block placement; placed-item
selection and move; recolor; rotation of placed blocks; erase; exit create/edit/delete; timer edit;
save; load; and play-test. Undo/redo is explicitly deferred. Loading invalid data must preserve the
current editing session. Structural edits must reject, with a list of affected items, any change
that would delete, relocate, crop or invalidate existing blocks or exits. The play-test uses the
same runtime construction/rules as a saved level
without turning editor preview objects into gameplay authority.

Common board/cell/footprint authoring behavior demonstrated in Drop The Man and needed here is to
be extracted into PuzzleFramework and consumed by both games. CBE block and exit tools remain
game-owned. The working Drop The Man editor has erase and placed-hole rotation, but it does not
already provide true placed-item move or an integrated play-test bridge; those are new work.

## Acceptance examples

1. A 2×3 block with its 2-cell span facing a width-2 exit can pass; facing its 3-cell span it
   cannot. A width-3 matching exit accepts the 2-cell span under the chosen MVP rule.
2. A T footprint with a three-cell projected bounding span cannot use a width-2 exit even if only
   one occupied cell first contacts that opening.
3. A wrong-color, busy, badly aligned, or outward-inaccessible exit does not capture a block.
4. A block authored against a matching exit stays on board until a player-driven outward drag.
5. A second block cannot enter a busy exit, while vacated board cells become available as the
   first block logically clears them.
6. If the final block is accepted with time remaining or exactly at expiry, a later timer tick
   or slow chipper cannot change the win. If time expires first, later visual contact cannot
   create a win.
7. Save/load/play-test preserve cell states, IDs, colors, footprint offsets, exits, and timer data.

## Approval and delivery risks

The requested shared-editor extraction **plus** Drop The Man adoption, true selection/move,
play-test, irregular-edge exits, continuous sweeps, progressive occupancy, and pooled chipper form
a high-risk seven-day scope. None can be called already implemented. The technical designs keep
these requirements and framework boundaries intact. A reliable seven-day promise requires early
Unity integration and explicit checkpoints. If shared infrastructure or progressive release
threatens the target, report measured cost and seek a scope/schedule decision. A game-local copy of
genuinely shared behavior is not an acceptable automatic fallback.

The target checkpoints, counted from the start of approved prototype development, are: by day 2,
framework sweep/clearance primitives are reusable and CBE can load/drag plain blocks; by day 4, CBE
admits exits with progressive release and correct timer outcomes while shared occupancy transfer
is available to both games; by day 6, the framework authoring core is consumed by both games and
the CBE editor round-trips and play-tests levels; by day 7, integrate pooled presentation and
finish Unity regression checks. These are risk checkpoints, not a high-confidence time estimate.
A missed checkpoint prompts a concrete scope/schedule review, not an unannounced rule cut or a
game-local architecture shortcut.
