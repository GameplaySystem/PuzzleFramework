# Puzzle Framework Interview Preparation

This guide is a study companion for the current repositories, not a replacement for reading the code. It was written against these repository states:

- `PuzzleFramework` at `dcb4d38`
- `DropTheMan` at `d2ef2e9`, consuming framework commit `c485272d5e0b9764d67fe1b07a5e9af6429623f7`
- `PuzzleFramework-ColorBlockEscape` at `57314c2`, consuming framework commit `fe9f9bc6635554b81f327844207cd1c36c33f9e8`

The distinction between the current framework checkout and the commits consumed by each prototype matters. The prototype manifests use immutable Git package pins, so a prototype sees the framework revision named in its own `Packages/manifest.json`, not automatically the latest framework checkout.

## Prioritized study roadmap

### Tier 1 — absolutely understand before an interview

1. **The ownership boundary:** the framework owns reusable mechanics and data contracts; each game owns the meaning and policy built on those mechanics.
2. **Board, footprint, and occupancy:** `GridBoard`, `GridCoordinate`, `ShapeFootprint`, `CellOccupancySystem`, and atomic footprint transfer.
3. **Continuous movement:** `GridWorldLayout`, `ShapeAwareDragFootprint`, `SweptFootprintHelper`, `FootprintClearanceQuery`, and how DTM and CBE compose them differently.
4. **Authored data to runtime state:** `LevelDefinition` → persistence validation → runtime construction validation → `RuntimeLevelContext` → game-specific model builder.
5. **The shared authoring core:** why `LevelAuthoringCore` was extracted only after two concrete editors existed, and how DTM and CBE apply different structural-edit policies.
6. **Game-owned state machines:** DTM reservation/collection/completion and CBE capture/exiting/outcome precedence.
7. **Implemented versus planned:** be able to say clearly that pathfinding, the generic event system, resource-processing systems, CBE chipper/pooling, and a final standalone CBE gameplay scene are not implemented runtime features.

### Tier 2 — understand well

1. Input intent versus platform polling and view hit testing.
2. `TimerSystem` and `GameStateSystem`, including why they return facts/results instead of deciding game outcomes.
3. Level catalogs and progression persistence.
4. Wall topology, modular board visuals, and shared color identity.
5. DTM presentation callbacks and why visual completion affects DTM capacity while CBE victory is accepted before presentation.
6. Assembly definitions, Git package pins, and dependency direction.
7. The focused test suites and what they do and do not prove.

### Tier 3 — difficult follow-up material

1. Sweep event generation and deterministic candidate ordering.
2. Exterior flood fill for irregular-board exits.
3. Exact timer/acceptance boundary handling in CBE.
4. Allocation and complexity hot spots.
5. Transactional weaknesses in runtime construction and final-slice release.
6. Why several scene and editor controllers are too large, and how to improve them without pushing game rules into the framework.
7. The evolution visible in Git history: minimal framework foundations, DTM proving game-specific needs, then CBE justifying shared movement and editor extraction.

## How to study this document

Use three passes rather than trying to memorize everything.

1. Read the Tier 1 explanations and draw the data-flow diagrams from memory.
2. Open the files in the code-walkthrough order and verify every claim yourself.
3. Answer each knowledge check aloud. Only then rehearse the supplied interview answers. The answers are examples of sound reasoning, not scripts.

---

# 1. The architecture and ownership boundary

## What it does

In simple terms, PuzzleFramework supplies reusable puzzle-building parts. Drop The Man and Color Block Escape assemble those parts and decide what they mean in their own games.

Technically, the framework package contains generic authored-data contracts, board topology, occupancy storage, interaction geometry, runtime construction, runtime flow, presentation helpers, progression persistence, and a live authoring session. The game assemblies reference the framework assembly. The framework assembly does not reference either game.

The repository currently uses one runtime assembly, `PuzzleFramework.Runtime`, with namespaces and folders separating categories. DTM has `DropAwayPrototype.Runtime`; CBE has `ColorBlockEscape.Runtime`. Assembly definition references enforce the primary dependency direction:

```text
DropAwayPrototype.Runtime ─┐
                          ├──> PuzzleFramework.Runtime
ColorBlockEscape.Runtime ─┘
```

## Why it exists

The project is testing whether multiple puzzle prototypes can reuse mechanics without forcing their game rules into a universal controller. The second prototype is essential evidence: a class is not reusable merely because it has a generic name. DTM and CBE now both consume shared board, footprint, sweep, occupancy, layout, and authoring primitives while retaining different rule coordinators.

This also keeps framework changes safer. `CellOccupancySystem` can answer whether coordinates are in use and transfer a footprint atomically. It never needs to know whether the occupant is a hole, block, cat, or exit.

## How it works

The common runtime path is:

```text
JSON / authored session
        │
        ▼
LevelDefinition
  ├─ Metadata
  ├─ FrameworkData: board + timer
  └─ opaque ContentPayload: type id + JSON string
        │
        ├─ framework persistence/schema checks
        ├─ framework runtime construction checks
        ▼
RuntimeLevelContext
  ├─ GridBoard
  ├─ CellOccupancySystem
  └─ construction board data
        │
        ▼
game-specific payload parser/builder
        │
        ▼
game-specific runtime state and scene adapters
```

The framework transports `SerializedLevelContentPayload` without interpreting it. DTM parses it with `DropTheManContentPayloadParser` and builds `DropTheManRuntimeModel`. CBE parses it with `ColorBlockEscapeLevelCodec` and builds `ColorBlockEscapeRuntimeLevel`.

Important files:

- [LevelDefinition.cs](../Packages/com.gaming.puzzleframework/Runtime/Content/LevelDefinition.cs)
- [LevelRuntimeBuilder.cs](../Packages/com.gaming.puzzleframework/Runtime/RuntimeConstruction/LevelRuntimeBuilder.cs)
- [PuzzleFramework.Runtime.asmdef](../Packages/com.gaming.puzzleframework/Runtime/PuzzleFramework.Runtime.asmdef)
- [DTM runtime model builder](https://github.com/GameplaySystem/DropTheMan/blob/main/Assets/GameModules/Runtime/DropTheManRuntimeModelBuilder.cs)
- [CBE runtime builder](https://github.com/GameplaySystem/PuzzleFramework-ColorBlockEscape/blob/main/Assets/GameModules/Runtime/ColorBlockEscapeRuntimeBuilder.cs)

## Architectural decisions

### Framework mechanics, game policy

The most important decision is that the framework exposes capabilities and facts while game modules decide consequences.

| Framework capability | DTM meaning | CBE meaning |
|---|---|---|
| `ShapeFootprint` | hole capacity and drag geometry | block geometry and projected exit span |
| `CellOccupancySystem` | committed hole placement | committed block placement and retained exit corridor |
| `SweptFootprintHelper` | blocks tunneling and orders collectible contacts | blocks tunneling and validates alignment/outward travel |
| `TimerSystem` | produces expiry fact for `DropTheManOutcomeRouter` | produces expiry fact for tie-aware `ColorBlockEscapeOutcomeSession` |
| `LevelAuthoringCore` | DTM entities with warned prune-on-resize policy | blocks with reject-invalidating-edit policy |
| `ColorIdentity` | hole/stickman matching | block/exit matching |

The framework therefore contains no `Hole`, `Stickman`, `Block`, or `Exit` types.

### Pure C# core with Unity adapters at the edge

Many core services are ordinary C# classes. MonoBehaviours poll devices, raycast/project pointers, instantiate views, draw HUDs, and advance frame time. This makes the rule code directly testable in Edit Mode and avoids requiring a scene for every logic test.

The separation is useful but incomplete. The framework runtime assembly still references Unity because contracts use `Vector2`, `Vector3`, `Ray`, `Mathf`, `JsonUtility`, `GameObject`, and `MonoBehaviour`. It is “plain-class heavy,” not engine-independent.

### One framework assembly

All framework categories currently compile into one assembly. This keeps package setup simple, but namespace boundaries are conventional rather than assembly-enforced inside the framework. A mature package might split runtime core, Unity presentation, editor tooling, and tests into separate assemblies. Doing that now would add package complexity without changing current behavior.

## Alternatives and tradeoffs

- **One base game class with virtual hooks:** quicker at first, but DTM and CBE have different state machines and would inherit irrelevant concepts.
- **A service locator or global singleton:** easier scene access, but hides dependencies, makes tests order-dependent, and weakens lifecycle control. The current code uses constructor injection and explicit composition instead.
- **A fully generic entity-component puzzle engine:** potentially flexible, but speculative for two prototypes. The current abstraction stops at demonstrated shared mechanics.
- **Duplicate utilities in each game:** locally simple, but would leave coordinate, sweep, and editor semantics inconsistent. CBE was the evidence needed to extract these.

The current approach introduces translation work at every boundary. Each game must map its payload into framework structures and compose several services. That is intentional, but the mapping code can be repetitive.

## Failure cases and assumptions

- A prototype can pin an older framework commit and therefore behave differently from the latest framework checkout.
- Namespace separation does not prevent accidental cross-category dependencies inside `PuzzleFramework.Runtime`.
- The opaque payload boundary prevents framework/game coupling, but it also means framework validators cannot catch game-specific errors.
- Core services can be used incorrectly if a caller skips the intended validation sequence.
- The architecture assumes a single-threaded Unity gameplay context. Collections and mutable runtime states are not thread-safe.

## Performance

There is no reflection-heavy dependency injection container, service locator, or entity framework. Composition is explicit and small collections are used for mobile-scale puzzle boards. That is a reasonable baseline.

The cost is not uniformly optimized. Movement queries allocate temporary lists and sets, editor operations use linear scans, and board views are rebuilt with instantiate/destroy. Those choices are acceptable for current board sizes and authoring workflows, but they should be profiled before claiming production mobile performance.

## C# and Unity knowledge demonstrated

- **Assembly definitions:** game assemblies reference the framework; tests reference both the game and framework assemblies.
- **Interfaces and dependency injection:** `IGridSnapSystem`, `ILevelRuntimeBuilder`, `IRuntimeConstructionValidator`, save/load interfaces, and DTM view interfaces allow substitution in tests and composition roots.
- **Composition:** game coordinators own and combine narrow framework services rather than inheriting from a framework gameplay superclass.
- **Immutable result values:** many operations return `readonly struct` results with success/failure details.
- **Mutable domain state:** game runtime entities expose controlled transitions rather than public field mutation.
- **Unity serialization:** serializable DTO classes use public fields because `JsonUtility` serializes fields rather than general C# properties.

## Interview questions

### Why is the game payload serialized as an opaque JSON string inside another level JSON structure?

**Strong answer:** The outer `LevelDefinition` gives every game the same metadata, board, and timer schema. The framework can save, load, catalog, and construct those shared pieces without referencing a game assembly. The payload carries a content type ID and game-owned JSON. Each game validates and interprets that payload after the shared validation/build step. The cost is nested serialization and duplicated mapping, but it preserves dependency direction.

**Understand, do not memorize:** Which validator owns which errors, and why the framework is deliberately unable to understand a hole or exit.

**Likely follow-ups:** How would you migrate payload versions? Why not use `SerializeReference`? How do you reject the wrong content type?

### How do you know the framework abstractions are actually reusable?

**Strong answer:** I look for dual adoption, not generic naming. DTM and CBE both consume `GridBoard`, `CellOccupancySystem`, `GridWorldLayout`, sweep/clearance primitives, and `LevelAuthoringCore`. Their game-specific policies stay separate: DTM reserves collectibles and fills capacity through presentation callbacks; CBE admits blocks through exits and progressively releases occupancy. The different consumers are evidence that the shared layer captures mechanics rather than one game's vocabulary.

**Understand, do not memorize:** Name one shared primitive and trace two meaningfully different uses.

**Likely follow-ups:** Which abstraction would you remove? What has only one consumer? When would you promote another system?

### Why not split every framework category into its own assembly?

**Strong answer:** The current package is small and one assembly minimizes setup and cyclic-reference risk. Folder and namespace boundaries are enough for this stage. The drawback is that internal category dependencies are not compiler-enforced. I would split pure runtime core, Unity presentation, and editor-only tooling when compile times, platform constraints, or team ownership make that enforcement valuable.

**Understand, do not memorize:** Assembly boundaries have maintenance and compilation costs; more assemblies are not automatically better architecture.

**Likely follow-ups:** What would the dependency graph be? Where would `GridWorldLayout` live? Would tests need separate assemblies?

## Knowledge check — answer before continuing

1. Why can `PuzzleFramework` build a board from CBE data without knowing what a CBE block is?
2. Give three concrete examples of policy that remains game-owned.
3. What does an immutable Git package pin protect against, and what maintenance problem can it create?
4. Which parts of the core still depend on Unity types?
5. What evidence would justify moving CBE exits into the framework later?

---

# 2. Board, coordinates, footprints, and occupancy

## What it does

In simple terms, this layer describes where cells exist, which cells are blocked, what shape an object occupies, and which cells are currently in use.

Technically, `GridBoard` stores sparse structural topology inside rectangular bounds. `ShapeFootprint` stores relative integer offsets. `CellOccupancySystem` stores occupied and reserved coordinate sets over one exact `GridBoard`. `GridWorldLayout` translates between board-local coordinates and Unity world space.

## Why it exists

Unity transforms and colliders are poor authoritative puzzle state. They can drift, overlap because of floating-point movement, and entangle logic with presentation. Integer coordinates and explicit occupancy make placement and collision deterministic and testable. Sparse structure also supports irregular boards: a coordinate can be inside width/height bounds but absent from the playable topology.

## How it works

### Structural board

`GridCoordinate` is a serializable readonly value type with value equality and a stable hash. It is safe as a dictionary or hash-set key.

`GridBoard` receives dimensions, structural coordinates, and optional blocked coordinates. Its constructor rejects:

- non-positive dimensions;
- null collections;
- structural coordinates outside the rectangle;
- duplicate structural coordinates;
- duplicate blocked coordinates;
- blocked coordinates that are not structural cells.

The important distinction is:

- `IsWithinBounds(c)` asks whether `0 <= x < Width` and `0 <= y < Height`.
- `ContainsCell(c)` asks whether a playable structural cell exists there.
- `IsBlocked(c)` asks whether that structural cell is marked blocked.

`GetNeighbors` returns orthogonally adjacent structural cells. It does not promise that gameplay may traverse them.

### Footprints

`ShapeFootprint` owns a nonempty, unique list of relative `GridCoordinate` offsets. `ResolveCoordinates(origin)` adds each offset to an origin. It does not normalize offsets, require `(0,0)`, or require edge connectivity. That broader policy belongs to a game or authoring layer. CBE's codec and authoring session add connectivity and size rules; the generic footprint does not.

### Occupancy

`CellOccupancySystem` uses two `HashSet<GridCoordinate>` collections: occupied and reserved. Individual operations validate that the coordinate exists on its board. `CanAcceptFootprint` validates the entire destination before mutation.

`TransferFootprint(source, destination)` is the key operation:

1. Collect unique source and destination coordinates; reject duplicates or emptiness.
2. Verify every source cell is currently occupied.
3. Verify every destination is structural and not reserved.
4. Reject destination occupancy unless that coordinate is also part of the source footprint.
5. Only after all checks pass, remove source-only cells and add destination cells.

This makes a move atomic from the caller's point of view. A failed transfer leaves the original occupancy unchanged.

Important files:

- [GridCoordinate.cs](../Packages/com.gaming.puzzleframework/Runtime/CoreBoard/GridCoordinate.cs)
- [GridBoard.cs](../Packages/com.gaming.puzzleframework/Runtime/CoreBoard/GridBoard.cs)
- [ShapeFootprint.cs](../Packages/com.gaming.puzzleframework/Runtime/CoreBoard/ShapeFootprint.cs)
- [CellOccupancySystem.cs](../Packages/com.gaming.puzzleframework/Runtime/CoreBoard/CellOccupancySystem.cs)
- [GridWorldLayout.cs](../Packages/com.gaming.puzzleframework/Runtime/CoreBoard/GridWorldLayout.cs)

## Architectural decisions

### Structure and occupancy are separate

Blocked cells are structural metadata. Occupied and reserved cells are runtime state. This prevents a moving object from rewriting board topology and permits multiple independent systems to query the same board.

### Occupancy has no entity IDs

The occupancy service knows that a coordinate is used, not who uses it. This keeps it small and game-agnostic. Callers pass their current footprint as an exemption during clearance checks and maintain entity-to-footprint state themselves. The tradeoff is weaker diagnostics and no built-in protection against one caller releasing another caller's cells if higher-level invariants are broken.

### Explicit anchor contract

`GridWorldLayout` supports `GridCellAnchor.Center` and `GridCellAnchor.Corner`, arbitrary orthogonal board axes, and nonuniform positive cell size. DTM uses center anchoring with an XZ board plane. CBE uses corner anchoring with an XY board plane. The same picking API can serve both because the anchor is explicit rather than an undocumented offset hidden in each scene.

## Alternatives and tradeoffs

- **Unity physics as truth:** gives collision callbacks and broad-phase acceleration, but introduces collider setup, discrete/continuous physics differences, and presentation coupling. This project uses logical queries for puzzle truth.
- **Dense 2D arrays:** faster indexed access and fewer hashes for rectangular boards, but sparse/inactive topology becomes sentinel-heavy and resizing/mapping logic changes. The current dictionary suits small irregular boards.
- **Occupancy owner map:** `Dictionary<GridCoordinate, EntityId>` would improve debugging and ownership safety, but adds an identity contract the current framework has not needed.
- **Normalized polyominoes:** simplifies shape comparison and rotation, but would silently change authored origin semantics. The current code preserves explicit offsets.

## Failure cases and edge cases

- Inside bounds does not imply playable; callers must check `ContainsCell`.
- `CellOccupancySystem` does not reject blocked cells by itself. Structural/gameplay clearance queries must do so. This is a deliberate narrow boundary but easy to misuse.
- `TransferFootprint` requires a nonempty destination. CBE's final exit slice therefore releases remaining cells individually.
- A malformed caller can release any occupied coordinate because occupancy does not track ownership.
- `ShapeFootprint` accepts disconnected shapes. CBE rejects those in its own schema; another game may allow them.
- Arbitrary axes must be nonzero and orthogonal. Floating-point conversions still require epsilon-aware boundary handling.

## Performance

Hash-set and dictionary membership are expected O(1). A footprint operation is generally O(number of footprint cells). For puzzle boards with tens of cells, this is appropriate and clearer than physics.

The collections allocate when constructed, and footprint resolution often creates lists. There is no pooling of query collections. This is acceptable at current scale, but a profiler should guide any production optimization. Converting the entire design to arrays without evidence would trade flexibility for an unmeasured gain.

## C# and Unity knowledge demonstrated

- Value-type equality and `GetHashCode` for collection keys.
- `IReadOnlyList`/`IReadOnlyCollection` exposure to protect collection ownership.
- Constructor validation and fail-fast invariants.
- Hash sets for uniqueness and membership.
- `readonly struct` for compact operation results.
- Coordinate-space transforms using Unity vectors while retaining logical integer state.

## Interview questions

### Why are bounds, structural cells, blocked cells, and occupied cells separate concepts?

**Strong answer:** Bounds define the addressable rectangle. Structural cells define the actual irregular board inside it. Blocked cells are permanent topology that exists but cannot be entered. Occupancy is mutable runtime usage by entities. Combining these would make holes in a board, obstacles, and moving objects indistinguishable and would complicate validation and rendering.

**Understand, do not memorize:** Be able to classify an inactive cell, a blocked obstacle, and a cell beneath a moving block.

**Likely follow-ups:** Could an inactive coordinate be occupied? Where is that prevented? How are walls generated around inactive space?

### What makes `TransferFootprint` atomic?

**Strong answer:** It performs all duplicate, source-presence, structural, reservation, and destination-collision checks before mutating either set. Only after validation succeeds does it remove source-only cells and add destinations. The atomicity is logical within the single-threaded call; it is not a database transaction or thread synchronization primitive.

**Understand, do not memorize:** State what remains unchanged after a failed transfer.

**Likely follow-ups:** How are overlapping source/destination cells handled? Why does it reject empty destinations? Is it thread-safe?

### Why does occupancy not store entity ownership?

**Strong answer:** Both prototypes only require binary occupied/reserved checks at the reusable layer, while game state already owns entity IDs and footprints. Omitting identity keeps the API generic. The weakness is that caller bugs can release another entity's cells and diagnostics cannot name the owner. If that becomes a real recurring problem, an owner token or typed occupancy layer would be justified.

**Understand, do not memorize:** This is a conscious scope decision, not a claim that ownerless occupancy is universally ideal.

**Likely follow-ups:** How would you migrate to owner-aware occupancy? Would reservations use the same owner type?

## Knowledge check — answer before continuing

1. Explain a coordinate that is within bounds but fails `ContainsCell`.
2. Why can `ShapeFootprint` remain less strict than CBE's footprint codec?
3. Walk through a transfer where old and new footprints overlap by two cells.
4. What bug becomes possible because occupancy stores no owner ID?
5. Why does DTM use center anchoring while CBE uses corner anchoring, and why can both still use `GridWorldLayout`?

---

# 3. Input, continuous dragging, collision, and snapping

## What it does

In simple terms, this layer turns pointer motion into safe block or hole movement without letting fast input jump through walls.

Technically, there are three separate concerns:

1. `InputSystem` coordinates a single pointer interaction through `ISelectable`, `IClickable`, and `IDraggable` capabilities.
2. `BoardPointerProjection` and `GridWorldLayout` translate pointer rays/world positions into board space.
3. Sweep and clearance primitives enumerate every footprint contact introduced along a continuous segment and validate those cells against structure and occupancy.

## Why it exists

Checking only the final pointer position allows tunneling: a fast frame can begin before a blocker and end after it. Moving one whole cell per input sample feels stepped. Physics could solve parts of this but would make colliders, rigidbodies, and frame timing part of gameplay authority. The project instead uses continuous logical geometry over grid cells.

## How the shared geometry works

`ShapeAwareDragFootprint` converts footprint offsets into unit rectangles. It may inset exposed outer edges while preserving shared internal edges. DTM uses a small configurable inset for drag feel; CBE uses zero because any visual penetration into inactive or occupied cells would violate its rules.

`SweptFootprintHelper.EnumerateContactGroups`:

1. Converts the previous and candidate world positions to continuous board-local origins.
2. Calculates normalized event times where any rectangle edge crosses an integer grid line on X or Y.
3. Sorts and epsilon-deduplicates those times, always including the endpoint.
4. Samples each interval just after crossings and resolves cells with positive geometric overlap.
5. Emits deterministic groups containing overlapped cells, newly entered cells, and ordered candidates.

This is event-based traversal, not fixed-distance stepping. A very long pointer segment generates more crossing events, so it cannot skip intermediate cells merely because the frame delta was large.

`FootprintClearanceQuery` checks ordered contacted cells for out-of-bounds, missing structural cell, blocked, reserved, or occupied status. The caller supplies self cells that may be ignored for occupancy collision.

`GridSnapSystem` evaluates a nearest integer origin and full destination footprint. It returns a result; it does not mutate occupancy.

Important files:

- [InputSystem.cs](../Packages/com.gaming.puzzleframework/Runtime/Interaction/InputSystem.cs)
- [BoardPointerProjection.cs](../Packages/com.gaming.puzzleframework/Runtime/Interaction/BoardPointerProjection.cs)
- [ShapeAwareDragFootprint and sweep request types](../Packages/com.gaming.puzzleframework/Runtime/Interaction/SweptFootprintTypes.cs)
- [SweptFootprintHelper.cs](../Packages/com.gaming.puzzleframework/Runtime/Interaction/SweptFootprintHelper.cs)
- [FootprintClearanceQuery.cs](../Packages/com.gaming.puzzleframework/Runtime/Interaction/FootprintClearanceQuery.cs)
- [GridSnapSystem.cs](../Packages/com.gaming.puzzleframework/Runtime/Interaction/GridSnapSystem.cs)

## DTM runtime flow

The scene-side pointer adapter polls Unity's Input System and projects the pointer. It calls `DropTheManRuntimeController`, which owns orchestration but not Unity polling.

```text
DropTheManPointerInputAdapter
  → DropTheManRuntimeController.Begin/Update/Release
    → DropTheManDragSessionOwner
      → DropTheManMovementCoordinator
        → ShapeAwareDragFootprint
        → SweptFootprintHelper
        → FootprintClearanceQuery
        → StickmanCoordinateIndex and hole capacity reservation
```

The movement coordinator clamps the candidate to board bounds. If the full diagonal path is blocked, it tries horizontal-only and vertical-only slide candidates and chooses the one with greater progress. During drag, the framework occupancy remains at the hole's last committed cell footprint. The coordinator exempts only that committed footprint.

For each swept contact group:

- structural or occupied blockage stops movement;
- an available wrong-color stickman blocks movement;
- a same-color stickman may reserve one hole-capacity slot and is immediately removed from `StickmanCoordinateIndex`;
- when a reserved stickman enters the collection radius of any hole footprint cell, it changes to `Collecting`.

The view then runs collection presentation. Its callback converts reserved capacity into filled capacity. When the hole becomes full, normal dragging stops, its committed footprint is released, closing presentation runs, and only the completion callback changes it to `Completed` and can produce a win.

On non-full pointer release, `DropTheManReleaseCommitService` evaluates snap and then calls `TransferFootprint`. This separates the freeform visual pose from `HoleRuntimeState.CurrentCoordinate`, which remains the last committed board origin during drag.

Key DTM files:

- [DropTheManMovementCoordinator.cs](https://github.com/GameplaySystem/DropTheMan/blob/main/Assets/GameModules/Runtime/DropTheManMovementCoordinator.cs)
- [DropTheManDragSessionOwner.cs](https://github.com/GameplaySystem/DropTheMan/blob/main/Assets/GameModules/Runtime/DropTheManDragSessionOwner.cs)
- [DropTheManReleaseCommitService.cs](https://github.com/GameplaySystem/DropTheMan/blob/main/Assets/GameModules/Runtime/DropTheManReleaseCommitService.cs)
- [DropTheManRuntimeController.cs](https://github.com/GameplaySystem/DropTheMan/blob/main/Assets/GameModules/Runtime/DropTheManRuntimeController.cs)
- [DropTheManRuntimeState.cs](https://github.com/GameplaySystem/DropTheMan/blob/main/Assets/GameModules/Runtime/DropTheManRuntimeState.cs)

## CBE runtime flow

`PlainBlockDragAdapter.Update` advances time, advances accepted exit travel, polls touch before mouse, projects the pointer to the board plane, and routes a `BlockDrag` through the shared `InputSystem`.

`PlainBlockMovement.Move`:

1. Converts the requested world position to board-local space.
2. Clamps it so the whole shape remains within rectangular board bounds.
3. Sweeps from the previous accepted pose to the target with zero footprint inset.
4. If clear, accepts the continuous position.
5. If blocked, performs 18 binary-search iterations to find the last clear pose along the segment.
6. After normal movement, optionally asks `ColorBlockEscapeExitCapture` whether the outward sample qualifies for capture.

On release, it uses `GridSnapSystem`, sweeps the short path to the snapped pose, validates the complete destination, and atomically transfers occupancy. If any step fails, it restores `ContinuousOrigin` to `CommittedOrigin`.

Key CBE files:

- [PlainBlockDragAdapter.cs](https://github.com/GameplaySystem/PuzzleFramework-ColorBlockEscape/blob/main/Assets/GameModules/Runtime/PlainBlockDragAdapter.cs)
- [PlainBlockMovement.cs](https://github.com/GameplaySystem/PuzzleFramework-ColorBlockEscape/blob/main/Assets/GameModules/Runtime/PlainBlockMovement.cs)

## Architectural decisions

### Shared geometry, game-owned movement policy

The framework enumerates contacts and reports clearance. It does not decide that a wrong-color cat blocks a hole, that a block can enter a matching exit, or that a blocked diagonal should slide along one axis. Those are game policies.

### Committed and continuous positions are distinct

Both games preserve an integer committed origin and a floating-point continuous drag pose. Occupancy follows committed state during ordinary dragging and changes only through an approved commit. This avoids rewriting hash-set occupancy on every pointer sample.

CBE exit capture is the exception because an exiting block logically spans a changing corridor. It uses explicit retained-cell state and safe transfers/releases.

### The older movement services still exist

`DragMovementSystem` and `GridSnapSystem` predate the swept primitives. `DragMovementSystem` validates a destination footprint but does not sweep intermediate space, so it is not the authority for current continuous DTM/CBE movement. `GridSnapSystem` remains useful for release-time snapping. An interviewer may notice this overlap; describe it as an evolved API with a cleanup opportunity, not as one unified movement implementation.

## Alternatives and tradeoffs

- **Continuous physics/Rigidbody sweep:** established collision tools, but harder to make grid topology and occupancy authoritative and deterministic.
- **Fixed substeps:** easier to implement, but correctness depends on step size and can become expensive for large deltas.
- **AABB sweep for the entire shape:** cheaper, but fills gaps in irregular footprints and produces false collisions. This code sweeps per-footprint rectangles.
- **Update occupancy continuously:** makes current blocking exact, but causes many mutations and raises ambiguous fractional-cell ownership. Current regular drag keeps occupancy committed and uses geometric clearance.
- **Project to the last valid position analytically:** potentially faster than CBE's 18-step binary search, but more complex for arbitrary polyomino contacts. The fixed iteration count is simple and deterministic enough for small boards.

## Failure cases and edge cases

- DTM's coordinator is mutating: calling it twice for one pointer sample can reserve twice. `DropTheManDragSessionOwner` centralizes that call but cannot identify duplicate samples by timestamp or sequence number.
- DTM holds reservations across release/re-drag. That is intended gameplay, but cancellation semantics require care.
- A DTM hole's committed occupancy remains at its pre-drag footprint while its view moves. Clearance exemptions and release commit must stay consistent with that invariant.
- CBE's binary search assumes collision along the tested straight segment is monotonic: after the first blocking interval, later positions are not treated as reachable through the blocker.
- The sweep uses floating-point epsilon checks. Boundary poses need tests because exact grid-line contact is sensitive.
- `InputSystem` supports one active pointer target. Multi-touch gameplay is intentionally unsupported.
- Hit testing in CBE loops through blocks and footprint cells rather than using a spatial index.

## Performance

The current sweep allocates event-time lists, overlap hash sets, result lists, and sorted candidate lists per query. DTM may sweep multiple candidate paths when testing diagonal and axis slides. CBE may repeat a full sweep up to 18 times when blocked. These are the clearest runtime allocation/CPU hot spots.

For current puzzle sizes this is likely acceptable, but no benchmark in the repository proves mobile headroom. A production optimization path would profile first, then reuse buffers, reduce LINQ/allocation, cache shape rectangles, and replace repeated binary-search sweeps only if measurements justify it.

Physics is intentionally absent from logical movement. That avoids Rigidbody/Collider overhead and nondeterministic callback timing, but all collision correctness now depends on the custom geometry tests.

## C# and Unity knowledge demonstrated

- Capability interfaces (`IDraggable`, `ISelectable`, `IClickable`) and composition.
- Nested adapter objects (`BlockDrag`) closing over a MonoBehaviour owner.
- Unity Input System device polling for mouse and primary touch.
- Ray-plane projection and coordinate-space conversion.
- State ownership and separation of view transforms from logical coordinates.
- Local callback functions in DTM presentation orchestration.
- Dependency injection through `IGridSnapSystem` for focused testing.

## Interview questions

### How does the sweep prevent tunneling without simulating tiny time steps?

**Strong answer:** It computes event times at which any footprint rectangle edge crosses an integer grid boundary. Between consecutive events, the set of overlapped grid cells is stable. Sampling those intervals enumerates every relevant contact transition regardless of pointer speed. Clearance is evaluated for each emitted group, so an intermediate blocker cannot be skipped just because the final pose is clear.

**Understand, do not memorize:** Be able to draw one square moving from x=0.2 to x=3.2 and mark the grid-line crossing times.

**Likely follow-ups:** Why sample after the crossing? How do you handle simultaneous X/Y crossings? What is the complexity?

### Why does DTM not update occupancy during every drag sample?

**Strong answer:** DTM separates a freeform view pose from the committed board origin. During drag, geometric sweeps validate movement while the original committed footprint remains occupied and is exempted as self. On valid release, snap and atomic transfer commit the new origin. This avoids fractional occupancy semantics and frequent mutations. It requires strict session ownership so the committed and freeform states do not become confused.

**Understand, do not memorize:** Know which value is authoritative for rendering during drag and which is authoritative for board occupancy.

**Likely follow-ups:** Can another hole enter the visually vacated cells? How would simultaneous drags change the design? What happens when a hole becomes full mid-drag?

### Why is CBE's movement policy not a framework class?

**Strong answer:** The reusable part is continuous footprint contact and structural/occupancy clearance. CBE additionally chooses zero inset, clamps blocks inside the board, binary-searches the last clear point, attempts exit capture after an outward sample, and snaps/commits on release. Those choices are not shared wholesale by DTM, so putting them in the framework would encode one game's feel and rules.

**Understand, do not memorize:** Identify the exact boundary between a geometric fact and a gameplay consequence.

**Likely follow-ups:** Which part might be promoted after a third game? Could the last-clear-point solver be generic?

### What would you profile first on mobile?

**Strong answer:** I would profile drag frames around `SweptFootprintHelper`, especially CBE's blocked-path binary search and DTM's diagonal plus two slide candidates. I would inspect GC allocations from temporary lists/hash sets and CPU time as travel distance and footprint size grow. I would not claim a pooling or zero-allocation solution already exists because it does not.

**Understand, do not memorize:** Tie every optimization proposal to a measured call path.

**Likely follow-ups:** How would you remove allocations? Would Burst/Jobs help? When would physics be cheaper?

## Knowledge check — answer before continuing

1. Trace one CBE pointer sample from `Mouse.current` to a view transform change.
2. Why is checking only the destination footprint insufficient?
3. What does DTM do when diagonal motion is blocked?
4. Which DTM operation reserves capacity, and which later operation fills it?
5. Why does CBE use a zero drag inset?
6. What incorrect behavior could result from calling DTM's movement coordinator twice for one sample?

---

# 4. CBE exit capture and progressive occupancy

## What it does

In simple terms, this system decides whether a block is close enough, aligned enough, correctly colored, and physically able to leave through an authored opening. After acceptance it locks player control, moves the block outward, and frees board cells as the block leaves.

Technically, `ColorBlockEscapeExitCapture` is a CBE-owned state machine built from shared sweep, clearance, layout, and occupancy primitives.

## Why it exists

An exit is more than a missing wall. It has game meaning: color matching, aperture width, an outward direction, a capture threshold, busy state, and win acceptance timing. Generalizing that into PuzzleFramework would be premature because DTM has no corresponding edge entity or rule.

## How it works

`TryCapture(block, previousPose, requestedPose)` examines exits and rejects until one satisfies all conditions:

1. The block is still `OnBoard`.
2. The pointer sample has positive motion along the exit's outward normal. Starting near an exit does not auto-capture.
3. Exit color matches and the exit is not busy.
4. The block's projected bounding span along the aperture fits the exit width.
5. The leading edge is inside the configurable capture distance.
6. Lateral overlap reaches `requiredSpan * OverlapFraction`; the default tuning is 0.70, not a hard game constant.
7. A discrete fully aligned origin is chosen within the aperture.
8. `OnBoardAlignmentClear` sweeps from the current pose to that alignment and checks the final footprint.
9. `ExitSweepClear` verifies the entire outward corridor. Cells beyond the chosen exit are allowed only when they are not structural cells; in-board contacts must be active and free of other occupancy.
10. The service computes all remaining in-board corridor cells and atomically transfers occupancy from the old retained set to that set.

The pre-reserved corridor is important for irregular shapes. Alignment may acquire new cells before strictly outward travel begins. After acceptance, `Advance` moves only along the exit normal. It computes the remaining corridor subset, verifies that it never grows, transfers to the smaller set, and releases the final slice when the set becomes empty. At full travel the block becomes `Removed` and the exit becomes free.

The block's `AcceptedExitId` is set at capture, before outward presentation finishes. Outcome code uses that accepted fact rather than waiting for removal.

Important files:

- [ColorBlockEscapeExitCapture.cs](https://github.com/GameplaySystem/PuzzleFramework-ColorBlockEscape/blob/main/Assets/GameModules/Runtime/ColorBlockEscapeExitCapture.cs)
- [ColorBlockEscapeRuntimeLevel.cs](https://github.com/GameplaySystem/PuzzleFramework-ColorBlockEscape/blob/main/Assets/GameModules/Runtime/ColorBlockEscapeRuntimeLevel.cs)
- [ExitBoundaryValidator.cs](https://github.com/GameplaySystem/PuzzleFramework-ColorBlockEscape/blob/main/Assets/GameModules/Runtime/ExitBoundaryValidator.cs)

## Architectural decisions

### Bounding span, not cell count

A rotated 2×3 rectangle needs an aperture wide enough for its projection perpendicular to travel. Total cell count does not answer that question. An irregular shape may contain gaps, but the approved rule uses the bounding span, producing predictable doors rather than allowing narrow interlocking passage tricks.

### Acceptance is independent of presentation

Logical success occurs when admission commits. The view can later animate outward movement or a chipper, but a missing or slow effect cannot invalidate success. This is essential for deterministic outcome precedence.

### Exit validation separates authoring structure from runtime admission

`ExitBoundaryValidator` checks that every exit segment lies on an active cell and faces genuine exterior space. It flood-fills inactive cells connected to the board perimeter; a sealed internal inactive hole does not count as exterior. Runtime capture then checks the moving block, color, span, alignment, occupancy, and corridor.

## Alternatives and tradeoffs

- **Release the entire footprint immediately on capture:** much simpler, but another block could enter cells still visually and logically covered by the exiting block. It violates the approved progressive-release rule.
- **Move cell by cell after capture:** simpler occupancy, but loses continuous outward motion and may look stepped.
- **Model an exit as shared framework edge entity:** premature because only CBE currently needs side/start/width/color and admission semantics.
- **Use actual mesh/collider opening width:** couples gameplay to presentation assets and scale. Current logic uses board-cell units.

The current corridor reservation can be conservative: it reserves every in-board cell the irregular footprint will need along the remaining route. That guarantees monotonic release after alignment but may block another piece from entering a cell before the exiting shape physically reaches it.

## Failure cases and edge cases

- Wrong-color exits remain solid because ordinary movement clamps to the board and capture rejects the color.
- Inward or tangential samples cannot capture.
- An exit narrower than the projected span rejects even if the block has fewer occupied cells because of gaps.
- A structurally blocked or occupied alignment/corridor rejects without changing lifecycle or occupancy.
- A busy exit rejects a second block until the first is completely removed.
- If `AdvanceBlock` sees a corridor set that would grow after outward motion, it records `LastAdvanceFailure` and stops. This guards the monotonic invariant.
- Final release is a cell-by-cell loop because generic transfer rejects empty destinations. The code throws if a supposedly owned retained cell cannot be released. It has no rollback for a partial failure, relying on prior invariants.

## Performance

Capture is not a per-block global search every frame; it runs for the actively dragged block after movement and iterates authored exits. It still constructs sets and performs multiple sweeps during an accepted attempt. `Advance` iterates all level blocks each frame and only acts on `Exiting` blocks. Current levels are small, but a large number of simultaneous exits would justify indexing active exit transitions.

No chipper fragment pool or DOTween presentation is implemented in CBE yet. DOTween binaries are installed, but the game module contains no `DG.Tweening` use and no fragment pool. Do not claim that presentation optimization exists.

## C# and Unity knowledge demonstrated

- A game-specific state machine using `BlockLifecycle` and `ExitRuntimeState.IsBusy`.
- Vector dot products for direction tests.
- Projection bounds and axis-dependent transforms.
- Hash-set subset checks for monotonic occupancy.
- Separating tunable immutable settings from mutable session state.
- Explicit error reporting through result structs plus `LastAdvanceFailure` for frame-driven advancement.

## Interview questions

### Why does capture reserve the remaining corridor instead of only the aligned footprint?

**Strong answer:** An irregular footprint can acquire a different in-board cell as it translates outward. The approved invariant says alignment may transfer occupancy safely, but once strictly outward travel starts occupancy must only shrink. Reserving the complete remaining in-board corridor at acceptance guarantees that later updates never overwrite another block and can only release cells. The tradeoff is conservative blocking.

**Understand, do not memorize:** Draw an L shape moving outward and show a cell it can acquire during translation.

**Likely follow-ups:** Could you reserve slices just in time? Would reservations be better than occupancy? How would simultaneous exits interact?

### How do you distinguish an exterior notch from an internal hole?

**Strong answer:** `ExitBoundaryValidator` flood-fills absent cells starting from absent coordinates on the board perimeter. Only absent cells reached by that flood are exterior inactive space. An absent region fully enclosed by structural cells is never reached, so an edge facing it is rejected.

**Understand, do not memorize:** This is graph connectivity over inactive coordinates, not a visual wall check.

**Likely follow-ups:** Complexity? What about a one-cell tunnel to the perimeter? Why are blocked cells not exterior?

### What is one weakness in final occupancy release?

**Strong answer:** When no cells remain, the generic transfer cannot accept an empty destination, so CBE releases retained cells one by one and throws on failure. The code assumes all retained cells are owned and validated; if that invariant is already broken, a partial release has no rollback. A generic `ReleaseFootprintAtomically` operation would make this safer if a second use case justifies it.

**Understand, do not memorize:** Know the difference between a currently safe invariant and a generally transactional API.

**Likely follow-ups:** Would you change `TransferFootprint` to allow empty destination? What tests would you add?

## Knowledge check — answer before continuing

1. List every admission condition in the order it is evaluated.
2. Why is 70% overlap not permission to ignore geometry?
3. When does the exit become busy and when does it become free?
4. What fact determines victory: capture, full outward travel, or presentation completion?
5. Explain why an internal inactive region cannot host an exit.

---

# 5. Content, validation, runtime construction, and catalogs

## What it does

In simple terms, this pipeline turns a saved level file into safe shared runtime board state, then lets the correct game construct its own entities.

Technically, it deliberately splits validation into three ownership layers:

1. persistence/schema validation;
2. shared runtime construction safety;
3. game-specific construction and gameplay validation.

## Why it exists

A single “validate everything” method would couple file I/O to scene construction and game rules. It would also tempt PuzzleFramework to inspect opaque payloads. The current separation lets invalid JSON fail before runtime work, invalid boards fail before partially built shared state is published, and game-specific errors remain in their owning assembly.

## How it works

### Persistence

`JsonLevelSaveLoadService` uses `JsonUtility` and `System.IO`. It validates root objects, metadata, IDs and versions, board dimensions/cells, coordinate bounds and duplicates, basic timer fields, and the payload type/JSON pairing. It does not test block overlap, exit geometry, DTM solvability, or runtime prefab availability.

### Runtime construction

`LevelRuntimeConstructionValidator` reads only `FrameworkData.Board`. It excludes `Inactive` entries, collects structural and blocked coordinates, and constructs a temporary `GridBoard` as a build-safety probe. It returns issue codes and `RuntimeConstructionBoardData`.

`LevelRuntimeBuilder` only publishes a `RuntimeLevelContext` if validation succeeds. That context holds the structural data, `GridBoard`, and a fresh `CellOccupancySystem`. It intentionally does not spawn GameObjects.

### Game construction

DTM's `DropTheManRuntimeModelBuilder` parses the DTM payload, validates prototype placement, constructs hole and stickman states, occupies hole footprints, and builds a `StickmanCoordinateIndex`.

CBE's `ColorBlockEscapeRuntimeBuilder`:

- requires an explicit state for every coordinate in the rectangular board;
- requires a valid enabled countdown timer;
- validates exits with `ExitBoundaryValidator`;
- constructs connected block footprints;
- rejects non-active placement and overlap;
- occupies all block cells;
- builds exit runtime states.

### Catalogs

`ResourcesLevelCatalogLoader` discovers `TextAsset`s under a Resources path. `LevelCatalogBuilder` delegates game-specific metadata extraction to `ILevelCatalogMetadataReader`, then rejects duplicate IDs case-insensitively and duplicate sequence numbers, sorts entries, and reports sequence gaps as warnings rather than failures.

Important files:

- [JsonLevelSaveLoadService.cs](../Packages/com.gaming.puzzleframework/Runtime/Content/JsonLevelSaveLoadService.cs)
- [LevelCatalogBuilder.cs](../Packages/com.gaming.puzzleframework/Runtime/Content/LevelCatalogBuilder.cs)
- [LevelRuntimeConstructionValidator.cs](../Packages/com.gaming.puzzleframework/Runtime/RuntimeConstruction/LevelRuntimeConstructionValidator.cs)
- [LevelRuntimeBuilder.cs](../Packages/com.gaming.puzzleframework/Runtime/RuntimeConstruction/LevelRuntimeBuilder.cs)
- [CBE codec](https://github.com/GameplaySystem/PuzzleFramework-ColorBlockEscape/blob/main/Assets/GameModules/Runtime/ColorBlockEscapeLevelCodec.cs)
- [DTM payload parser](https://github.com/GameplaySystem/DropTheMan/blob/main/Assets/GameModules/Runtime/DropTheManContentPayloadParser.cs)

## Architectural decisions

### Validation ownership is explicit

The framework persistence service owns file/schema integrity. Runtime construction owns only the ability to build shared structures. Game builders own payload meaning and legal game placement. This is one of the strongest decisions in the codebase because it prevents convenient but damaging dependency leakage.

### Runtime construction does not instantiate views

The shared builder returns logical state. DTM's composition layer registers/spawns game views separately; CBE's authoring play-test builds simple views separately. This keeps construction tests fast and prevents a missing prefab from contaminating board validation.

### DTOs are separate from runtime state

`LevelDefinition` and game payload classes are mutable serializable DTOs. Runtime classes expose controlled operations and read-only collections. Loading creates fresh runtime state rather than playing directly against serialized objects.

## Alternatives and tradeoffs

- **ScriptableObject levels:** excellent Unity authoring references and inspector integration, but less convenient for external JSON, versioned text diffs, and game-agnostic opaque payload transport.
- **Polymorphic serialized base classes:** removes nested JSON but tends to require type registration and makes the framework aware of derived game types.
- **One validator:** fewer calls, but blurred ownership and difficult unit tests.
- **Addressables instead of Resources:** better scalable content delivery, but more setup than the current prototype needs.

## Failure cases and edge cases

- `JsonUtility` has limited support for dictionaries and polymorphism, so DTOs use lists and fields.
- Level save writes directly to the destination; unlike progress persistence, it is not an atomic temp-file replacement. A process interruption can corrupt the level file.
- DTM model building mutates a newly built occupancy system while it constructs holes. It rolls back an individual hole's already occupied cells on failure, but the broader build is designed as disposable rather than a reusable transactional context.
- The framework builder catches exceptions and returns a failure string, which is friendly for setup flow but can hide exception type/stack unless the caller logs enough context.
- CBE deliberately rejects incomplete explicit board data even though the generic construction validator can build a sparse list. This is game-owned strictness.
- Catalog gap warnings do not imply missing content is invalid; sequence ordering remains deterministic.

## Performance

JSON parsing and file access occur at load/save boundaries, not per frame. Board validation is linear in authored cells plus hash-based duplicate checks. Catalog construction is linear plus sort, approximately O(n log n). `Resources.LoadAll` loads all matching level assets at once, which is simple for a small prototype catalog but not a large live game's streaming strategy.

## C# and Unity knowledge demonstrated

- Serializable DTOs and Unity `JsonUtility` constraints.
- Interface-driven metadata readers and builders.
- Result objects instead of exceptions for expected invalid-content flow.
- Exception boundaries around genuinely exceptional construction faults.
- Read-only runtime collection wrappers.
- Generic `LoadResult<T>` style persistence responses.

## Interview questions

### Why are there multiple validation passes?

**Strong answer:** Each pass protects a different boundary. Save/load checks whether the shared document is structurally serializable. Runtime construction checks whether generic board services can be built. The game parser/builder checks payload semantics and game placement. Combining them would either make persistence know gameplay rules or leave late failures after runtime mutation.

**Understand, do not memorize:** Given an error, identify its owner: duplicate board coordinate, disconnected CBE footprint, or unsolvable puzzle.

**Likely follow-ups:** Is any validation duplicated? How do you keep messages consistent? Where would prefab validation belong?

### Why use an opaque payload rather than a generic list of entities?

**Strong answer:** A generic entity schema would need to anticipate holes, stickmen, exits, buses, portals, and their evolving metadata. The opaque payload makes the stable shared envelope reusable without pretending those meanings are already common. The cost is that shared tooling cannot inspect game content unless the game supplies an adapter.

**Understand, do not memorize:** The abstraction deliberately gives up introspection to preserve ownership.

**Likely follow-ups:** How does the editor still manipulate generic footprints? How are payload versions handled?

### What would you improve about level saving?

**Strong answer:** Progress saving already writes to a unique temporary file, flushes, and replaces/moves the destination. Level saving currently writes directly. I would consider the same atomic replacement strategy for authored levels, preserving the old file on failure and cleaning temporary files. I would add tests for failed replacement and invalid save input.

**Understand, do not memorize:** Know why an atomic file replacement matters independently of JSON validation.

**Likely follow-ups:** What differs across platforms? Would cloud saves change the design?

## Knowledge check — answer before continuing

1. Where should a duplicate board coordinate be rejected?
2. Where should a disconnected CBE block footprint be rejected?
3. Why does `LevelRuntimeBuilder` stop before prefab creation?
4. What happens to `Inactive` cells during shared construction?
5. What is the weakness of direct level-file writes?

---

# 6. Shared level authoring and dual adoption

## What it does

In simple terms, `LevelAuthoringCore` is the live in-memory editing model shared by both game editors. It handles board cells, generic footprint entities, selection, placement, movement, rotation, deletion, snapshots, and structural-impact reporting. Each game adds its own tools, metadata, rules, visuals, and save payload.

## Why it exists

The framework editor was intentionally deferred while only DTM existed. The DTM editor proved a workflow, but extracting it wholesale would have copied hole/cat behavior into the framework. CBE became the second concrete use case and exposed the true common core: board structure, generic footprints, atomic edits, picking, and extensible tool dispatch.

The extraction is validated by adoption, not location. DTM was migrated to a live `LevelAuthoringCore`, then CBE built on the same core with different policy.

## How the framework authoring core works

`LevelAuthoringCore` owns:

- a full rectangular dictionary of authored cell states;
- generic `AuthoredFootprint` items keyed by ordinal string ID;
- metadata and timer DTOs;
- selected cell/item and selected tool ID;
- placement, move, clockwise rotation, erase, resize, cell-state changes, restore, and snapshots.

Placement is evaluated before mutation. Every resolved footprint cell must be inside dimensions, exactly `Active`, and not overlap another item. Move and rotate construct candidate footprints and only replace the stored item after validation succeeds.

Structural changes use a two-stage contract:

1. `InspectResize` or `InspectCellStateChange` reports affected items/cells and can include game-owned affected IDs through a callback.
2. `TryResize` or `TrySetCellState` rejects when the report says content would be affected.

This keeps consequence detection shared while allowing game-specific response policy outside the core.

`TryRestore` builds a staged candidate session, requires exactly one valid state for every rectangular cell, and validates all items before publishing the new session. Invalid import therefore leaves the current live session untouched.

`AuthoringToolHost` registers small `IAuthoringTool` implementations by ID, selects one, and routes preview/apply calls with either a cell target or boundary-edge target. It knows nothing about game modes.

`BoardAuthoringPicker` projects a ray to the board and uses the explicit layout anchor. It can pick generated boundary edges, but it does not decide whether an edge represents a legal CBE exit.

Important files:

- [LevelAuthoringCore.cs](../Packages/com.gaming.puzzleframework/Runtime/Content/LevelAuthoringCore.cs)
- [AuthoringToolHost.cs](../Packages/com.gaming.puzzleframework/Runtime/Content/AuthoringToolHost.cs)
- [BoardAuthoringPicker.cs](../Packages/com.gaming.puzzleframework/Runtime/Content/BoardAuthoringPicker.cs)
- [Level Editor Foundation design](FrameworkSystems/ContentSystems/LevelEditorFoundation.md)

## DTM adoption

`DropTheManEditorBoardController` now owns one live shared core with `GridCellAnchor.Center`. Generic fit, selection, move, rotation, erase, picking, board state, and snapshots route through it. DTM retains:

- man/hole payload records and colors;
- hole palette/prefab preview behavior;
- DTM-specific hotkeys and HUD;
- JSON mapping;
- the warned prune-on-resize policy.

For resize, DTM first inspects the shared impact report, warns the author, then explicitly removes affected game records before applying the resize. That policy is not embedded in the framework.

The main weakness is that `DropTheManEditorBoardController` is about 1,500 lines and still coordinates UI, view rebuilding, game payload, hotkeys, selection behavior, and prefab previews. The shared core removed duplicated validation but did not make the scene controller small.

DTM file:

- [DropTheManEditorBoardController.cs](https://github.com/GameplaySystem/DropTheMan/blob/main/Assets/GameModules/Runtime/Authoring/DropTheManEditorBoardController.cs)

## CBE adoption

`ColorBlockEscapeAuthoringSession` wraps one live `LevelAuthoringCore` and owns block colors and CBE exit records. It uses `GridCellAnchor.Corner`, matching CBE runtime's footprint-square convention.

CBE adds:

- connected footprint presets/custom offsets and a configurable new-shape dimension guard;
- block color, selection, move, recolor, and rotation;
- exit side/start/width/color and genuine exterior validation;
- timer editing;
- save/load through `ColorBlockEscapeLevelCodec`;
- play-test construction into a detached `ColorBlockEscapeRuntimeLevel`.

CBE rejects any resize or cell edit that would invalidate a block, exit, or nondefault cropped cell. It reports affected IDs/cells and requires the author to move or delete them first.

The controller maps B/M/O/E/S to modes, 0–9 to colors, mouse wheel to block shapes, left click to apply/select, and right click contextually to erase. `BeginPlayTest` creates a fresh runtime level and views while hiding the authoring root; returning destroys the play-test root and reveals the unchanged authoring session.

Key CBE files:

- [ColorBlockEscapeAuthoringSession.cs](https://github.com/GameplaySystem/PuzzleFramework-ColorBlockEscape/blob/main/Assets/GameModules/Runtime/Authoring/ColorBlockEscapeAuthoringSession.cs)
- [ColorBlockEscapeAuthoringTools.cs](https://github.com/GameplaySystem/PuzzleFramework-ColorBlockEscape/blob/main/Assets/GameModules/Runtime/Authoring/ColorBlockEscapeAuthoringTools.cs)
- [ColorBlockEscapeEditorController.cs](https://github.com/GameplaySystem/PuzzleFramework-ColorBlockEscape/blob/main/Assets/GameModules/Runtime/Authoring/ColorBlockEscapeEditorController.cs)

## Architectural decisions

### Live session is authoritative

Earlier DTM code reconstructed temporary authoring cores for checks. The migration made one session authoritative so selection, board states, and items cannot silently diverge from the object used for validation.

### Consequence detection is shared; response policy is game-owned

Both games need to know what a structural edit affects. They do not agree on what to do. The framework reports consequences; DTM deliberately prunes after warning, while CBE rejects. This is a clean example of shared fact versus game policy.

### Exits are not generic authored footprint entities

The shared picker understands a boundary edge. CBE owns exit records and their structural meaning. There is no evidence yet that side/start/width/color is a reusable edge-entity schema.

## Alternatives and tradeoffs

- **Copy the DTM editor into CBE:** fastest initially, but duplicates fit, picking, selection, and structural logic and preserves DTM nouns.
- **A giant universal editor MonoBehaviour:** centralizes everything but accumulates every game's toolbar and payload rules. The current model uses a generic core plus game scene controllers/tools.
- **Unity custom EditorWindow:** gives native inspector tooling, but the chosen Play Mode authoring scene supports runtime-like visuals and input and keeps the workflow usable without editor-only runtime dependencies.
- **Command pattern with undo/redo:** a good future direction, but currently deferred because neither game required history. Adding it would change edit APIs and mutation ownership.

## Failure cases and edge cases

- Rotation applies `(x,y) → (y,-x)` and preserves the item origin. It does not normalize offsets; a rotated shape can extend into negative relative coordinates and be rejected at its current origin.
- `TryFindItemAtCell` scans every item and every offset. This is fine for small authoring boards but not large maps.
- Full view rebuilds destroy and recreate authoring objects after edits, producing editor-time allocations and possible transient references.
- Game-specific dictionaries, such as CBE's block color map, must stay synchronized with core item deletion/load. The wrapper methods maintain that; bypassing the session would break it.
- `AuthoringToolHost` prevents duplicate tool IDs but offers no undo, tool lifecycle, shortcut conflict resolution, or dependency declaration.
- CBE cannot save a partial draft with no exits because `TryCreateLevel` runs the production codec/builder rules. That is a deliberate current content policy, but it can frustrate incremental authoring.

## Performance

Editor operations prioritize clarity and atomicity over runtime optimization. Placement overlap is approximately O(number of items × average footprint size). Structural checks may scan all items and CBE exits. View refreshes instantiate/destroy objects. Since this occurs during authoring on small boards, it is reasonable; a large-level editor would need a cell-to-item index, incremental rendering, and probably an undoable command model.

## C# and Unity knowledge demonstrated

- Composition of a generic session by game-specific facades.
- Delegates/callbacks for structural consequence extension.
- Staged construction for atomic restore.
- Strategy-like tool interface and dictionary registration.
- Play Mode authoring with MonoBehaviour `Update` and `OnGUI`.
- Separation between serializable DTO snapshots and live state.
- Input System `Key` handling and contextual mouse actions.

## Interview questions

### Why was the editor core extracted after DTM rather than before it?

**Strong answer:** With one editor, generic-looking APIs would have been guesses based on DTM. CBE provided a second concrete workflow and revealed the actual common behavior: board cells, footprint edits, selection, picking, structural impacts, and tool dispatch. It also revealed a policy difference—DTM prunes on resize after warning while CBE rejects—which belongs outside the shared core.

**Understand, do not memorize:** Give one capability that became shared and one tempting capability that remained game-owned.

**Likely follow-ups:** Was any extraction too late? What duplication remains? How would a third game register tools?

### How does invalid import avoid corrupting the current editor session?

**Strong answer:** The game first decodes its payload, builds generic `AuthoredFootprint` candidates, and calls `LevelAuthoringCore.TryRestore`, which constructs and validates a separate candidate session. The game runtime builder also validates the complete level. Only after both succeed does the wrapper replace `Core` and rebuild its game-specific color/exit collections. Failure leaves the old references and selection intact.

**Understand, do not memorize:** Identify the publication point where the live session reference changes.

**Likely follow-ups:** Could color-map replacement still fail? How would you make the whole wrapper transaction explicit?

### Why is structural edit response not inside `LevelAuthoringCore`?

**Strong answer:** The common need is to identify affected content. The desired response differs by product: DTM intentionally warns and prunes, CBE forbids silent loss and rejects. A universal response would encode one game's authoring policy. The core therefore reports IDs/cells and lets the game decide whether and how to proceed.

**Understand, do not memorize:** This is the same fact-versus-policy principle used in runtime systems.

**Likely follow-ups:** How does DTM safely prune? Could a policy interface be useful? When would that be overengineering?

### What is the biggest maintainability problem in the editor code today?

**Strong answer:** The framework core is focused, but DTM's scene controller remains very large and CBE's controller still mixes immediate-mode HUD, input, preview lifecycle, camera framing, save/load, and play-test setup. I would split scene presentation, input routing, view rebuilding, and file workflow behind the existing session without moving game concepts into the framework.

**Understand, do not memorize:** Refactoring a large game controller does not imply generalizing its rules.

**Likely follow-ups:** Which class would you extract first? How would you test the refactor? Would you replace `OnGUI`?

## Knowledge check — answer before continuing

1. What makes the authoring session “live” and authoritative?
2. How do DTM and CBE respond differently to the same resize impact report?
3. Why does shared edge picking not imply shared exits?
4. What happens if a selected CBE block cannot rotate at its current origin?
5. Why can CBE play-test without mutating the authoring session?

---

# 7. Runtime flow, outcomes, presentation, and progression

## What it does

This group coordinates high-level lifecycle, countdown time, game-specific win/loss decisions, presentation callbacks, and persistent campaign progress.

## Shared runtime flow

`GameStateSystem` owns `NotStarted`, `Playing`, `Paused`, `Won`, and `Lost` and an explicit transition table. Duplicate transitions succeed as no-ops. It does not publish events.

`TimerSystem` is a countdown timer despite the authored `TimerMode` also containing `CountUp`. It owns start/stop/pause/resume/reset, elapsed/remaining time, a one-shot warning threshold, and a one-shot expiry fact returned by `Advance`. It does not decide loss.

This design favors explicit return data over a generic event bus. The event-system documentation is approved, but runtime event infrastructure is intentionally absent until a real multi-listener need appears.

Important files:

- [GameStateSystem.cs](../Packages/com.gaming.puzzleframework/Runtime/RuntimeFlow/GameStateSystem.cs)
- [TimerSystem.cs](../Packages/com.gaming.puzzleframework/Runtime/RuntimeFlow/TimerSystem.cs)

## DTM outcomes and presentation

`DropTheManOutcomeRouter` accepts completed-hole facts and timer-expired facts. A win occurs only after every required hole reaches `Completed`. First terminal outcome wins; later requests are ignored.

DTM deliberately makes collection presentation part of its capacity sequence:

```text
Available stickman
  → Reserved (immediately removed from blocking index)
  → Collecting (trigger radius reached)
  → view plays Animator/DOTween sequence
  → callback marks stickman Collected and fills reserved capacity
  → hole Full
  → footprint released; hole Closing
  → hole presentation callback
  → hole Completed
  → outcome router may accept Won
```

The controller supplies immediate fallback when a view cannot start presentation, preventing a missing visual from deadlocking state. DOTween sequences are linked to GameObjects and killed/reset on teardown. DTM does instantiate and destroy level views; it does not implement a general object pool for holes or cats.

The controller exposes `TerminalOutcomeAcceptedNow` as a narrow C# event for its progression adapter. This is a concrete local event, distinct from the unimplemented generic framework Event System.

## CBE outcome precedence

`ColorBlockEscapeOutcomeSession` creates a fresh `TimerSystem` and `GameStateSystem` for one runtime level. Completion means every block has an `AcceptedExitId` and is no longer `OnBoard`; it does not wait for `Removed`.

The frame order in `PlainBlockDragAdapter.Update` is:

1. `AdvanceTime(deltaTime)`;
2. stop active drag if terminal;
3. advance already accepted exits and update views;
4. process current input if still playing;
5. `ResolveBoundary()`.

For an exact expiry boundary, `AdvanceTime` marks expiry pending rather than losing immediately. Input gets one boundary opportunity. `ResolveBoundary` checks all accepted blocks before applying loss, so final acceptance wins the tie. If `deltaTime` overshoots the remaining time, loss occurs before input because the sample is considered late. Once won, later timer updates return early and cannot reverse the result.

## Progression

`PlayerProgressData` stores completed level IDs and a resume level ID. It restores atomically, rejects unsupported versions and duplicate/blank IDs, and returns sorted snapshot IDs for deterministic saves.

`JsonProgressSaveLoadService` uses a unique temporary file, flushes it, and replaces or moves it into place. It distinguishes not found, invalid data, unsupported version, and I/O failure.

DTM's `DropTheManProgressionSession` and `DropTheManLevelProgression` own campaign policy: unlock order, replay behavior, configured loop range, next-level selection, and when accepted wins are persisted. That policy correctly remains outside the framework.

Important files:

- [PlayerProgressData.cs](../Packages/com.gaming.puzzleframework/Runtime/Progression/PlayerProgressData.cs)
- [JsonProgressSaveLoadService.cs](../Packages/com.gaming.puzzleframework/Runtime/Progression/JsonProgressSaveLoadService.cs)
- [DTM outcome router](https://github.com/GameplaySystem/DropTheMan/blob/main/Assets/GameModules/Runtime/DropTheManOutcomeRouter.cs)
- [DTM progression session](https://github.com/GameplaySystem/DropTheMan/blob/main/Assets/GameModules/Runtime/DropTheManProgressionSession.cs)
- [CBE outcome session](https://github.com/GameplaySystem/PuzzleFramework-ColorBlockEscape/blob/main/Assets/GameModules/Runtime/ColorBlockEscapeOutcomeSession.cs)

## Architectural decisions

### Timer produces facts; game decides consequences

Both games reuse the same countdown service but have different completion timing. DTM waits for full-hole presentation completion; CBE wins at exit acceptance. Therefore loss/win precedence cannot live in `TimerSystem`.

### Presentation timing differs intentionally

DTM capacity fills when its collection presentation callback completes. That is an approved game rule, with fallback to avoid deadlock. CBE success explicitly does not depend on later exit/chipper animation. “Presentation never affects gameplay” would be an inaccurate blanket statement; the precise rule differs by game.

### Progress data is generic, progression policy is not

The framework persists a set and cursor. DTM decides what “next,” “replay,” and “loop range” mean. This prevents a reusable save model from dictating campaign design.

## Alternatives and tradeoffs

- **C# events for every timer/state change:** convenient for listeners, but can hide ordering and subscription lifetime. Explicit result facts keep current flows easy to trace.
- **Coroutines for timers/outcomes:** concise Unity code, but harder to unit test and reset deterministically.
- **Make DTM fill immediately:** simpler and less presentation-coupled, but contradicts the approved feel and capacity timing.
- **Make CBE wait for block removal:** visually intuitive, but a slow animation could turn a logically successful final move into a loss.

## Failure cases and edge cases

- `TimerSystem` does not explicitly reject NaN or positive infinity in its constructor/advance path. Game builders add stronger finite checks. This is a framework hardening opportunity.
- Authored `CountUp` exists in the schema, but the runtime timer is countdown-only. Callers must reject unsupported mode, as CBE does.
- DTM callback guards prevent duplicate presentation completion from filling capacity twice, but asynchronous callbacks make teardown and terminal-state handling complex.
- Progress replacement semantics depend on filesystem behavior; tests cover failed replacement preservation, but platform-specific behavior still deserves device validation.
- CBE's tie rule relies on the adapter calling `AdvanceTime` before input and `ResolveBoundary` after input. A different scene adapter order could violate the rule.

## Performance

Timer and game-state operations are constant time and allocation-light. DTM DOTween/Animator presentation has runtime object and tween overhead, but sequences are bounded to active collections and linked to objects for cleanup. CBE has no chipper/pooling implementation yet, so there is nothing to measure or claim there.

Progress and catalog operations occur outside the per-frame path. Saving flushes to disk and should not be placed in a hot gameplay loop.

## C# and Unity knowledge demonstrated

- Explicit finite-state transition tables.
- Events/delegates used narrowly for accepted terminal outcomes and presentation callbacks.
- Local functions and idempotence guards for asynchronous completion.
- DOTween sequence lifecycle and GameObject-linked cleanup in DTM.
- File-system transaction pattern using temp/replace.
- Set-based idempotent progress and deterministic snapshots.

## Interview questions

### Why doesn't `TimerSystem` transition the game to Lost?

**Strong answer:** Expiry is a generic fact, while its meaning and precedence are game rules. CBE has a final-acceptance tie rule; DTM victory waits for completed holes. If the timer owned loss, it would either know those games or make ordering bugs unavoidable. It returns `ExpiredRaised`, and a game-owned coordinator decides the transition.

**Understand, do not memorize:** Explain the exact CBE tie that proves the need for separation.

**Likely follow-ups:** Why not publish an event? How is warning handled? What if two systems request terminal states?

### Isn't DTM presentation coupled to gameplay?

**Strong answer:** Yes, deliberately and narrowly. Reservation immediately removes a cat from collision and prevents over-capacity. Actual fill occurs when the collection sequence completes because that timing is part of DTM's rule/feel. The controller provides an immediate completion fallback if presentation is missing, so logic cannot deadlock. CBE makes a different choice and accepts success before presentation.

**Understand, do not memorize:** Do not claim all presentation is cosmetic; describe the actual boundary.

**Likely follow-ups:** How would you test delayed callbacks? What happens on scene destruction? Would you keep this in a production version?

### How is the exact timer tie resolved?

**Strong answer:** If the delta exactly reaches remaining time, `AdvanceTime` records a pending expiry. The frame then processes input. `ResolveBoundary` checks final acceptance first and wins if all blocks were accepted; otherwise it loses. If the delta overshoots remaining time, the session loses before input because that input arrived after the boundary.

**Understand, do not memorize:** Be able to explain why update order is part of the contract.

**Likely follow-ups:** What about floating-point epsilon? How would FixedUpdate change it? Could timestamps make it more rigorous?

## Knowledge check — answer before continuing

1. Why does DTM win later than CBE for an apparently successful final interaction?
2. What is the difference between a generic event bus and DTM's narrow terminal event?
3. How does CBE distinguish an exact expiry from an overshoot?
4. Why is campaign ordering not stored in `PlayerProgressData`?
5. What runtime support is missing for authored `TimerMode.CountUp`?

---

# 8. Presentation: colors, walls, and board views

## What it does

The framework supplies stable color identities and turns structural board topology into a modular visual plan. Game projects provide actual materials, meshes, prefabs, and presentation timing.

## How it works

`ColorIdentity` defines `None` and ten stable slots, with legacy aliases for four original names. `ColorSystem` maps identities to Unity colors and rejects `None` or duplicate mappings. It is a visual lookup, not match-rule authority.

`WallGenerationSystem` examines a supplied set of participating coordinates. It emits exposed edges and classifies grid vertices as straight, convex, concave, or diagonal-touch based on the four adjacent quadrants.

`ModularBoardVisualPlanner` converts topology into per-cell flags for floors, half-walls, convex corners, and concave corners. It rejects diagonal-touch topology because the current modular art profile cannot represent it correctly.

`ModularBoardVisualBuilder.TryRebuild` validates the plan, prefab, root, and layout; destroys old children; instantiates one `ModularBoardCellView` per cell state; and positions/scales it through `GridWorldLayout`. CBE calls `SetBoundaryEdgeVisible` to hide wall halves where authored exits appear.

Important files:

- [ColorIdentity.cs](../Packages/com.gaming.puzzleframework/Runtime/Presentation/ColorIdentity.cs)
- [WallGenerationSystem.cs](../Packages/com.gaming.puzzleframework/Runtime/CoreBoard/WallGenerationSystem.cs)
- [ModularBoardVisualPlanner.cs](../Packages/com.gaming.puzzleframework/Runtime/Presentation/ModularBoardVisualPlanner.cs)
- [ModularBoardVisualBuilder.cs](../Packages/com.gaming.puzzleframework/Runtime/Presentation/ModularBoardVisualBuilder.cs)
- [ModularBoardCellView.cs](../Packages/com.gaming.puzzleframework/Runtime/Presentation/ModularBoardCellView.cs)

## Decisions, alternatives, and weaknesses

The planner is separate from the MonoBehaviour view so topology is testable without instantiating a scene. Half-wall slots let adjacent cells jointly form a wall and allow CBE to suppress an opening. The builder is intentionally narrow and rebuild-oriented rather than a pooled incremental renderer.

Alternatives include a generated combined mesh, Tilemap, sprites, or spline walls. A combined mesh would reduce GameObject count but complicate material sections, selection, and incremental authoring. The modular prefab approach was practical because DTM already had configured art pieces and CBE could reuse them.

Current limitations:

- diagonal-touch participation is rejected;
- one cell can create many child renderers/GameObjects;
- rebuild destroys and recreates all cell views;
- color slots are identities, not proof that ten polished materials exist;
- board view performance has not been benchmarked on target mobile hardware.

## Interview questions

### Why separate wall generation, visual planning, and view building?

**Strong answer:** Wall generation derives topology from coordinates. Planning converts topology into the vocabulary supported by a particular modular piece profile. Building performs Unity object work. This lets topology and slot decisions be tested as pure data and allows a future renderer to consume the same topology without changing board rules.

**Understand, do not memorize:** Name one responsibility and one failure each layer owns.

**Likely follow-ups:** Why reject diagonal touch? Could a combined mesh consume the same plan?

### Is the modular board optimized for mobile?

**Strong answer:** It avoids per-cell gameplay `Update`, but it instantiates a modular hierarchy per cell and rebuilds by destroy/recreate. The repository has no device measurement proving production performance. For current prototype boards it is acceptable. I would profile renderer count, batches, hierarchy cost, and rebuild spikes before considering mesh combination or pooling.

**Understand, do not memorize:** Honest evidence is stronger than invented optimization claims.

**Likely follow-ups:** Static batching? GPU instancing? Tilemap? When does authoring performance matter?

## Knowledge check — answer before continuing

1. Why is `ColorIdentity` not responsible for match rules?
2. What does a concave vertex mean for modular wall pieces?
3. Why does the planner reject diagonal touch?
4. What would you measure before replacing modular cells with a combined mesh?

---

# 9. Implemented, documented, and missing

An interviewer with the repository open can distinguish code from plans. Use precise language.

## Implemented now

- Generic level DTOs, JSON save/load validation, Resources catalog construction.
- Sparse board structure, blocked cells, coordinates, footprints, occupancy/reservation, atomic nonempty transfer.
- World/grid layout with explicit center/corner anchors and configurable axes.
- Input intent coordination, pointer projection, snapping, continuous sweep, clearance queries.
- Game state and countdown timer foundations.
- Color identity/mapping, wall topology, modular board planning/building.
- Runtime construction validation and shared context.
- Generic progress data and atomic local progress persistence.
- Live authoring core, tool host, cell/edge picking, structural impact reports.
- DTM playable runtime, collection/completion presentation, JSON levels, catalog, campaign progression, and shared-core editor adoption.
- CBE authored level construction, editor/play-test, continuous movement, exit capture/progressive occupancy, and timer/outcome logic.

## Documented or approved but not implemented as runtime systems

- Generic Event System.
- Generic resource-processing queue/buffer/capacity systems.
- Pathfinding runtime.
- Broad visual feedback system.
- Runtime object factory/prefab factory in shared construction.
- Count-up timer runtime support.
- Undo/redo in the shared editor.

## CBE work still missing

- Chipper presentation.
- Pooled fragment implementation.
- DOTween-driven scatter/break animation.
- Final presentation/UI polish and result flow.
- A finalized standalone gameplay scene beyond the authored-level play-test and movement checkpoint paths.
- Final block prefabs/assets supplied by the owner.

## Documentation drift and stale wording

- Some older framework documents describe complete architecture categories and can read like implementation inventories. `PROJECT_STATE.md` and the watchlist later introduced status language to distinguish “documented” from “implemented.” Code remains the authority for interview claims.
- `TimerMode.CountUp` exists in authored data and timer documentation discusses both modes, but `TimerSystem` is countdown-only.
- The approved Event System document exists, while `GameStateSystem` explicitly says event publication is deferred.
- `DragMovementSystem` exists, but current prototype continuous movement depends on later sweep/clearance primitives and game-owned coordinators.
- Older DTM editor documentation contains historical statements that generic editor extraction and gameplay play-test bridging were deferred. Later approved sections at the top and current code supersede those statements.
- The watchlist retains historical notes for handoff context. Read current status lines and code before treating every older bullet as active.

## Architectural weaknesses to discuss honestly

1. **Large scene controllers.** `DropTheManDevSceneBootstrapper`, `DropTheManRuntimeController`, and `DropTheManEditorBoardController` have broad orchestration responsibilities. They are understandable composition points, but some should be split for maintainability.
2. **Allocation-heavy collision traversal.** Correctness was prioritized over zero-allocation drag loops. This needs profiling and buffer reuse before production scale.
3. **Ownerless occupancy.** Small and reusable, but susceptible to caller mistakes and weaker diagnostics.
4. **Single framework assembly.** Simple package distribution, weaker compile-time category enforcement.
5. **Asymmetric persistence safety.** Progress saves are atomic; level saves are direct writes.
6. **Presentation dependency in DTM capacity.** Approved and guarded, but asynchronous callback orchestration is complex.
7. **No generic event layer.** This is deliberate restraint today; if multiple independent listeners appear, direct wiring may become cumbersome.
8. **Editor UI is functional rather than polished.** Play Mode `OnGUI`, full rebuilds, and large controllers are suitable for a prototype tool but not a finished commercial content pipeline.

## How to answer “what would you change?”

A strong answer should preserve working boundaries and prioritize evidence:

1. Profile the sweep and view hierarchy on target hardware.
2. Split large scene controllers by existing responsibilities without moving game policy into the framework.
3. Add atomic level-file replacement using the proven progress-save pattern.
4. Consider reusable query buffers/cached footprint geometry if profiling confirms drag allocations matter.
5. Add a safe atomic full-footprint release if CBE presentation or another game proves a second use.
6. Introduce a generic event layer only when multiple listeners create concrete coupling pressure.
7. Add editor commands/undo only when daily level-authoring use justifies the complexity.

Avoid answering with a fashionable rewrite such as ECS, Jobs, a dependency-injection framework, or a universal entity schema unless you can tie it to measured or repeated problems.

## Knowledge check — answer before continuing

1. Name five documented systems that you must not present as implemented.
2. Which current limitation would you fix first, and what evidence would you gather?
3. Which large class is a composition root, and when does a large composition root become a problem?
4. Why is the absence of a generic Event System currently defensible?
5. What historical document statements have been superseded by current implementation?

---

# 10. Test strategy and debugging

## What the tests actually cover

The framework Edit Mode suite focuses on invariants:

- atomic footprint transfer and failed-transfer non-mutation;
- intermediate swept collision and pointer offset;
- anchor-aware layouts and picking;
- atomic authoring edits/restore, tool dispatch, structural impact;
- wall topology and unsupported diagonal touch;
- catalogs, gaps, duplicates;
- progress idempotence, invalid restore, and file replacement failures.

DTM's focused tests cover the shared editor migration and extensive progression policy. Much of DTM gameplay was also manually validated in its development scene; the repository does not have the same broad automated runtime movement suite as CBE.

CBE's tests cover:

- continuous subcell movement, board limits, blocked/inactive cells, occupied blocks, tunneling, and irregular gaps;
- exit color/direction/span/threshold/alignment/busy/corridor behavior;
- progressive monotonic release;
- construction, duplicate IDs, disconnected footprints, explicit board/timer validation;
- exact timer ties, overshoots, completion freeze, and fresh reset;
- shared-core editor operations, structural rejection, exterior exits, save/load, anchor picking;
- Play Mode keyboard/mouse authoring and authored-level capture.

Tests prove rule behavior for their fixtures. They do not prove input feel, art alignment, touch behavior on real devices, frame-time budget, or final presentation quality.

## Debugging maps

### A block tunnels through an obstacle

1. Confirm the adapter sends previous accepted pose and candidate pose, not two raw unrelated positions.
2. Inspect `ShapeAwareDragFootprint.Rectangles` for the actual offsets and inset.
3. Log sweep contact groups and ensure the intermediate obstacle coordinate appears.
4. Verify `FootprintClearanceQuery` uses the correct board and only the active object's real self cells.
5. Check whether a caller accidentally used `DragMovementSystem` destination validation instead of the sweep path.

### CBE refuses a visually valid exit

1. Check outward dot product and current leading-edge distance.
2. Calculate projected bounding span for the current orientation.
3. Compare lateral overlap with the tunable threshold.
4. Inspect the chosen aligned origin.
5. Trace alignment sweep, final clearance, and outbound corridor separately.
6. Confirm the exit was authored on a genuine exterior-facing edge and is not busy.

### A DTM hole fills twice or never completes

1. Inspect stickman lifecycle and `ReservedHoleId`.
2. Confirm it was removed from `StickmanCoordinateIndex` exactly once.
3. Check that the presentation callback is invoked once; the controller has a local idempotence guard.
4. Confirm reserved capacity is converted to fill only in the completion callback/fallback.
5. Trace `Full → Closing → Completed` and whether the hole presentation callback arrives.

### An editor load corrupts current work

1. Verify the code stages a new `LevelAuthoringCore` with `TryRestore`.
2. Ensure game payload and runtime builder validation finish before assigning the live core.
3. Check game-owned side collections—colors, exits, DTM records—are replaced only after success.
4. Add a failing fixture for the exact partial state if a mutation occurred early.

## Interview questions

### Why are most framework tests Edit Mode tests?

**Strong answer:** Most framework services are plain deterministic classes operating on data. Edit Mode tests are faster and do not require scene setup. Play Mode tests are reserved for lifecycle and adapter behavior that depends on GameObjects, `Update`, device input, or view state. That split keeps logic feedback fast while still exercising critical integration paths.

**Understand, do not memorize:** Choose the lowest test layer that can prove the behavior.

**Likely follow-ups:** What should be a Play Mode test? What needs a device/manual test? How would you test DOTween callbacks?

### A test passes but dragging feels bad. What do you do?

**Strong answer:** The tests prove collision and state invariants, not feel. I would reproduce in the scene, record pointer and accepted trajectories, inspect input smoothing/max-speed settings, drag inset, binary-search stop behavior, camera projection, and frame rate. Then I would add a regression test only for a measurable rule discovered during that investigation, not for subjective feel itself.

**Understand, do not memorize:** Automated correctness and interaction feel are different evidence.

**Likely follow-ups:** What telemetry would you record? How would you compare touch and mouse?

## Knowledge check — answer before continuing

1. Which CBE test category proves tunneling prevention?
2. What important claims still require scene/device testing?
3. How would you isolate whether an exit failure comes from authoring validation or runtime admission?
4. Why is a failed-edit non-mutation assertion valuable?

---

# 11. Recommended code walkthrough

Read these in order. Do not move on until you can answer the checkpoint for each group.

## Tier 1 reading order

1. [GridCoordinate.cs](../Packages/com.gaming.puzzleframework/Runtime/CoreBoard/GridCoordinate.cs) and [GridBoard.cs](../Packages/com.gaming.puzzleframework/Runtime/CoreBoard/GridBoard.cs)  
   Understand value equality, sparse structure, bounds versus presence, and blocked cells.

2. [ShapeFootprint.cs](../Packages/com.gaming.puzzleframework/Runtime/CoreBoard/ShapeFootprint.cs) and [CellOccupancySystem.cs](../Packages/com.gaming.puzzleframework/Runtime/CoreBoard/CellOccupancySystem.cs)  
   Be able to simulate `TransferFootprint` by hand and state what it deliberately does not validate.

3. [GridWorldLayout.cs](../Packages/com.gaming.puzzleframework/Runtime/CoreBoard/GridWorldLayout.cs)  
   Draw center and corner anchors and explain DTM's XZ versus CBE's XY plane.

4. [SweptFootprintTypes.cs](../Packages/com.gaming.puzzleframework/Runtime/Interaction/SweptFootprintTypes.cs), [SweptFootprintHelper.cs](../Packages/com.gaming.puzzleframework/Runtime/Interaction/SweptFootprintHelper.cs), and [FootprintClearanceQuery.cs](../Packages/com.gaming.puzzleframework/Runtime/Interaction/FootprintClearanceQuery.cs)  
   Explain crossing events, positive overlap, candidate order, and self-cell exemption.

5. [LevelDefinition.cs](../Packages/com.gaming.puzzleframework/Runtime/Content/LevelDefinition.cs), [JsonLevelSaveLoadService.cs](../Packages/com.gaming.puzzleframework/Runtime/Content/JsonLevelSaveLoadService.cs), and [LevelRuntimeConstructionValidator.cs](../Packages/com.gaming.puzzleframework/Runtime/RuntimeConstruction/LevelRuntimeConstructionValidator.cs)  
   Classify validation responsibilities without mixing schema, build safety, and game rules.

6. [LevelAuthoringCore.cs](../Packages/com.gaming.puzzleframework/Runtime/Content/LevelAuthoringCore.cs)  
   Trace failed move, failed structural edit, restore, rotation, and snapshot behavior.

7. [DTM runtime state](https://github.com/GameplaySystem/DropTheMan/blob/main/Assets/GameModules/Runtime/DropTheManRuntimeState.cs), [movement coordinator](https://github.com/GameplaySystem/DropTheMan/blob/main/Assets/GameModules/Runtime/DropTheManMovementCoordinator.cs), and [drag session owner](https://github.com/GameplaySystem/DropTheMan/blob/main/Assets/GameModules/Runtime/DropTheManDragSessionOwner.cs)  
   Explain committed versus freeform position and reservation versus fill.

8. [CBE runtime level](https://github.com/GameplaySystem/PuzzleFramework-ColorBlockEscape/blob/main/Assets/GameModules/Runtime/ColorBlockEscapeRuntimeLevel.cs), [plain movement](https://github.com/GameplaySystem/PuzzleFramework-ColorBlockEscape/blob/main/Assets/GameModules/Runtime/PlainBlockMovement.cs), and [exit capture](https://github.com/GameplaySystem/PuzzleFramework-ColorBlockEscape/blob/main/Assets/GameModules/Runtime/ColorBlockEscapeExitCapture.cs)  
   Explain normal drag commit, capture admission, retained corridor, and monotonic release.

9. [TimerSystem.cs](../Packages/com.gaming.puzzleframework/Runtime/RuntimeFlow/TimerSystem.cs), [GameStateSystem.cs](../Packages/com.gaming.puzzleframework/Runtime/RuntimeFlow/GameStateSystem.cs), and [CBE outcome session](https://github.com/GameplaySystem/PuzzleFramework-ColorBlockEscape/blob/main/Assets/GameModules/Runtime/ColorBlockEscapeOutcomeSession.cs)  
   Reconstruct exact tie/overshoot behavior without reading comments.

10. [CBE authoring session](https://github.com/GameplaySystem/PuzzleFramework-ColorBlockEscape/blob/main/Assets/GameModules/Runtime/Authoring/ColorBlockEscapeAuthoringSession.cs) and the DTM editor's shared-core sections  
    Explain dual adoption and the two structural edit policies.

## Tier 2 reading order

11. [InputSystem.cs](../Packages/com.gaming.puzzleframework/Runtime/Interaction/InputSystem.cs) and both scene input adapters.  
    Separate device polling, hit testing, lifecycle coordination, and movement rules.

12. [WallGenerationSystem.cs](../Packages/com.gaming.puzzleframework/Runtime/CoreBoard/WallGenerationSystem.cs) and [ModularBoardVisualPlanner.cs](../Packages/com.gaming.puzzleframework/Runtime/Presentation/ModularBoardVisualPlanner.cs).  
    Understand topology data before reading prefab-building code.

13. [PlayerProgressData.cs](../Packages/com.gaming.puzzleframework/Runtime/Progression/PlayerProgressData.cs), [JsonProgressSaveLoadService.cs](../Packages/com.gaming.puzzleframework/Runtime/Progression/JsonProgressSaveLoadService.cs), and DTM progression tests.  
    Explain generic persistence versus campaign policy.

14. DTM's `DropTheManRuntimeController`, `DropTheManViewRegistry`, cat presentation, and hole presentation.  
    Follow callback ownership and terminal-state cleanup.

15. CBE and DTM editor controllers.  
    Identify which responsibilities are legitimately scene-owned and which make the controllers too large.

## Tier 3 reading order

16. All focused tests, starting with names and then reading arrangements/assertions.  
    For each major invariant, identify the test that proves it and the manual behavior it cannot prove.

17. Git history around `9de0d8e`, `c485272`, `722c5fa`, `e789caf`, `24291ef`, `6f398a5`, and `d7c57a3`.  
    Explain how reuse pressure changed the architecture in coherent checkpoints.

18. Approved docs and the implementation watchlist.  
    Practice spotting historical/deferred statements that current code supersedes.

---

# 12. Whiteboard explanations

## A. Framework boundary in five minutes

**Problem:** Multiple grid puzzle games need the same board, movement geometry, content pipeline, and editor mechanics, but their win rules and entities differ.

**Components:** Draw an outer `LevelDefinition`, a shared `RuntimeLevelContext`, then two game modules. Inside the framework box place board, occupancy, layout, sweep/clearance, timer/state, authoring core, persistence, and presentation helpers. Inside DTM place holes, stickmen, collection, capacity, and completion. Inside CBE place blocks, exits, capture, progressive release, and outcome precedence.

**Data/control flow:** Saved DTO → shared validation/build → game payload build → scene adapter → framework geometric facts → game rule decision → view update.

**Decision:** Shared services answer generic questions; games assign meaning and consequences. Dependencies point from games to framework.

**Tradeoff:** More adapters/mapping code, but no universal gameplay controller or framework knowledge of game nouns.

## B. Continuous movement in five minutes

**Problem:** Free dragging should feel continuous, but destination-only grid checks tunnel through obstacles.

**Components:** pointer projection, continuous board-local origin, shape rectangles, sweep event generator, clearance query, game movement coordinator, snap/occupancy commit.

**Flow:** Previous accepted pose and candidate pose → generate every grid-boundary crossing → resolve overlapped cells in order → query structure/occupancy → game policy chooses accept/block/slide/capture → render accepted pose → snap and transfer occupancy on release.

**Decision:** Logical geometry is authoritative; Unity physics is presentation/input infrastructure only.

**Tradeoff:** Deterministic and testable, but custom geometry allocates and must handle numerical boundaries carefully.

## C. Shared editor in five minutes

**Problem:** DTM had a useful but game-specific editor; CBE needed the same generic board operations with different entities and destructive-edit rules.

**Components:** live `LevelAuthoringCore`, structural impact report, picker, tool host, game wrapper, game scene controller.

**Flow:** Pointer selects cell/edge → selected game tool calls shared preview/apply → core validates atomically → game wrapper updates payload metadata → view rebuild → snapshot/codec/save or detached runtime play-test.

**Decision:** Extract only board/cell/footprint/edit mechanics proven by two games. Keep DTM holes and CBE exits in their modules.

**Tradeoff:** The shared model is reusable, while current scene controllers remain larger and less polished than a mature editor suite.

## D. CBE exit capture in five minutes

**Problem:** A block should leave only through a matching opening it can fully align with, without occupancy gaps or timer/presentation races.

**Components:** authored exterior exit, capture thresholds, span calculation, alignment sweep, corridor validation, retained occupancy, exit busy flag, game outcome session.

**Flow:** Outward drag → color/span/distance/overlap checks → collision-free alignment/outbound corridor → atomic corridor reservation → lock input and mark accepted → outward-only movement → progressively shrink occupancy → mark removed/free exit. Outcome checks accepted state, not animation completion.

**Decision:** Capture rules remain CBE-owned; only geometry and occupancy operations are shared.

**Tradeoff:** Corridor reservation is safe but conservative and creates temporary set allocations.

---

# 13. Practical modification drills

These are useful interview exercises because they test understanding rather than recall.

1. **Add an owner-aware diagnostic without changing occupancy semantics.** Decide whether to wrap occupancy in the game layer or extend the framework. Explain migration and tests before coding.
2. **Add counter-clockwise authoring rotation.** Reuse atomic candidate validation, preserve origin semantics, and test a rotation that becomes invalid near an edge.
3. **Make level saving atomic.** Adapt the progress service's temp/replace pattern while preserving `SaveResult` behavior.
4. **Add a third input source.** Keep platform polling in the adapter and feed the existing `InteractionPointerContext`; do not alter movement rules.
5. **Optimize sweep allocations.** First write a benchmark/Profiler scenario, then introduce reusable buffers without exposing mutable shared results.
6. **Add CBE pause.** Specify update ordering for timer, input, and exiting-block presentation; use existing game-state/timer transitions and decide whether outward presentation pauses.
7. **Support a second edge-based game mechanic.** Compare its data to CBE exits before proposing a shared edge-entity contract.
8. **Diagnose a center/corner anchor regression.** Write a picking-to-world round-trip test for both anchors and inspect the scene adapter's layout creation.

For each drill, state:

- which repository owns the change;
- which existing contract you would reuse;
- which behavior must remain unchanged;
- the smallest meaningful tests;
- whether documentation approval is required before implementation.

---

# 14. Final interview question bank

## Straightforward

### What are the framework's main runtime data structures?

**Strong answer:** `GridCoordinate`, `GridBoard`, `ShapeFootprint`, `CellOccupancySystem`, `GridWorldLayout`, `LevelDefinition`, `RuntimeLevelContext`, and explicit operation/result types. Together they represent topology, shapes, mutable use, coordinate mapping, authored data, and built shared state.

**Need to understand:** Which are authored DTOs, immutable/value-like data, and mutable runtime services.

**Follow-ups:** Which are serializable? Which use Unity types? Which own collections?

### How do the games reference the framework?

**Strong answer:** Each Unity project consumes the UPM package from Git with a full commit SHA and `?path=/Packages/com.gaming.puzzleframework`. Its game assembly definition references `PuzzleFramework.Runtime`. The framework never references game assemblies.

**Need to understand:** The package pin in a consumer is the actual version it compiles against.

**Follow-ups:** Why commit the lockfile? How do you test a local package change before pinning?

## Architecture challenges

### Isn't this overengineered for two small games?

**Strong answer:** Some parts are intentionally structured, but the framework additions are tied to concrete dual use: board topology, occupancy, layouts, swept footprint queries, content construction, timer/state, and authoring core. Systems with only speculative value—generic events, pathfinding runtime, generic capacity, CBE exits, and chipper pooling—were not promoted. The main over-complexity risk today is in large orchestration classes and mapping boilerplate, not a universal gameplay hierarchy.

**Need to understand:** Defend each abstraction with consumers, and concede parts that remain heavier than ideal.

**Follow-ups:** Which class would you delete? Which interface has only one practical implementation? Was the shared editor worth it?

### Why are so many result types used instead of exceptions?

**Strong answer:** Invalid authored data, blocked moves, invalid transitions, and unavailable cells are expected control-flow outcomes. Result values preserve old state and carry precise failure data without exceptions in normal interaction loops. Constructors still throw for programmer errors and impossible configuration, and runtime builder boundaries catch unexpected exceptions.

**Need to understand:** Distinguish expected domain rejection from programmer invariant failure.

**Follow-ups:** Are structs copied too much? Would a discriminated union help? Where are exceptions swallowed?

## Performance challenges

### What is the asymptotic cost of a drag sample?

**Strong answer:** It depends on footprint rectangles and the number of grid lines crossed. Event generation is roughly proportional to rectangle count times crossed boundaries, then each interval resolves overlapped cells and sorts candidates. CBE can repeat the sweep up to 18 times after a collision; DTM may test the original path and two axis slides. Hash lookups are expected O(1), but allocations and sorting matter more than big-O on small boards.

**Need to understand:** Be able to point to actual loops, not state “O(1) because it uses a grid.”

**Follow-ups:** Worst case for a long diagonal? How would you benchmark? Which buffers can be reused?

### Why not use ECS/Burst/Jobs?

**Strong answer:** Current boards and active entities are small, and rule clarity plus deterministic tests are the present bottlenecks. The code has managed allocations that should be profiled first. Jobs would require data layout and scheduling changes and do not automatically help a single actively dragged shape. I would adopt them only after evidence of CPU scale that buffer reuse or simpler algorithms cannot address.

**Need to understand:** Technology choice follows workload, not portfolio fashion.

**Follow-ups:** What data is job-friendly? What Unity APIs currently block worker-thread use?

## Debugging and change questions

### A block view is at the right place but occupancy is still at the old cells. Is that a bug?

**Strong answer:** During ordinary free drag, no. The continuous/view pose is separate from committed origin and occupancy. It becomes a bug if a successful release did not atomically transfer occupancy, or if an exit capture's retained corridor no longer represents required in-board cells. I would inspect lifecycle and whether the block is actively dragging, committed, or exiting before changing anything.

**Need to understand:** State must be interpreted in its lifecycle phase.

**Follow-ups:** What about DTM full-hole closing? How do other blocks collide during drag?

### How would you add undo/redo to the editor?

**Strong answer:** I would first define an approved editor command contract around existing atomic operations. Each successful command would capture enough prior state to reverse board, item, and game-owned metadata changes together. Structural policy decisions must occur before command commit. I would not put CBE exit payloads directly into `LevelAuthoringCore`; game wrappers could contribute companion commands or composite transactions.

**Need to understand:** Undo must include game-owned side data, not just generic footprints.

**Follow-ups:** Snapshot versus command diffs? How would load clear history? How do previews interact?

## “What did you personally build?”

Use wording you can defend. A precise answer is:

> I owned the architecture and technical decisions across the reusable framework and the two prototype integrations. I directed, reviewed, integrated, tested, documented, and implemented the systems represented in the repository. I can trace the code paths and explain the tradeoffs, including where implementation assistance was used. The clearest examples of my ownership are the framework/game boundary, the second-consumer editor extraction, and the different movement/outcome policies built on shared primitives.

Adjust that sentence to your actual contribution. Do not imply that you personally typed or independently invented code you cannot explain. An interviewer will test ownership by asking you to debug or modify it; this guide exists so your claims match your understanding.

---

# 15. Repository evidence and history to remember

The commit sequence tells a coherent engineering story:

1. The framework began as documentation and category boundaries.
2. Small runtime foundations were added incrementally: content, grid, occupancy, construction, input, snapping, state, timer, color, and footprints.
3. DTM supplied the first real game pressure: multi-cell movement, collection states, release commit, presentation callbacks, catalogs, progression, board visuals, and a game-specific editor.
4. CBE requirements exposed shared gaps. Commit `9de0d8e` added common swept movement and authoring primitives.
5. Commit `c485272` established the shared live authoring session.
6. DTM commit `722c5fa` adopted that session, proving it was not CBE code with generic names.
7. CBE then added movement (`e789caf`), shared-core editor adoption (`24291ef`), exit capture (`6f398a5`), and timer/outcomes (`d7c57a3`) as separate checkpoints.

This history supports an important interview answer: the architecture was not designed as one giant speculative framework. It grew from documented boundaries, a first game, and then a second game that justified extraction. It also shows unfinished scope honestly: CBE presentation remains the next game milestone.

## Final self-test

Without opening the code, explain all of the following aloud:

1. One complete authored-level-to-runtime flow for CBE.
2. One DTM drag from pointer press through cat collection and full-hole completion.
3. Why a fast drag cannot tunnel through a blocked cell.
4. How an irregular CBE block changes occupancy during capture and outward travel.
5. Why exact timer expiry can still produce a CBE win.
6. How the same authoring core supports prune-on-resize and reject-on-resize.
7. Three current architectural weaknesses and a measured improvement plan for each.
8. Five systems that are documented but not implemented.
9. One example where presentation affects DTM gameplay timing and one where CBE remains presentation-independent.
10. The next file you would inspect for each of these bugs: bad anchor, lost occupancy, invalid exit, duplicate collection, corrupt save, wrong next level.

If any answer depends on memorized wording, return to the linked code and draw the state/data flow yourself.
