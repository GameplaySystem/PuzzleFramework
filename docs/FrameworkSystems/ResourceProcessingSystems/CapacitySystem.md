# Capacity System

## Document Metadata

Category:
- Resource Processing Systems

Status:
- Approved

Parent:
- Overview.md

Related Documents:
- Overview.md
- QueueSystem.md
- BufferSystem.md

Depends On:
- None

Used By:
- Drop Away
- Sky Rush
- Hole People
- Bus Jam

## Purpose

The Capacity System manages generic count-based limits.

It validates whether a tracked count may increase or decrease, owns the current limit state, and reports whether space remains.

The Capacity System should be reusable across multiple puzzle games and should not contain game-specific rules.

---

# Core Design Idea

Capacity controls limits.

Capacity does not store resources.

```text
Caller requests count change
    ->
Capacity validates change against current state
    ->
Capacity updates tracked values if valid
    ->
Caller uses result to decide next action
```

---

# Example Uses

## Drop Away

A game module may limit how many items can occupy a temporary gameplay container or rule-defined space.

The Capacity System controls:

* current count
* maximum allowed count
* whether more items may be added
* whether items may be removed safely

The game module controls:

* what the items mean
* why they enter or leave
* puzzle completion rules
* presentation and feedback

---

## Sky Rush

A game module may limit how many waiting or transferred resources a destination can currently accept.

The Capacity System controls:

* count validation
* overflow rejection
* remaining available space

The game module controls:

* transfer eligibility
* ordering
* movement
* animation
* win or loss logic

---

## Hole People

A game module may limit how many temporary resources a holding area can contain at once.

The Capacity System controls:

* whether another resource may be accepted
* whether the tracked count is full or empty
* whether a removal request is valid

The game module controls:

* color or destination matching
* routing decisions
* transfer timing
* puzzle-specific outcomes

---

## Bus Jam

A game module may limit how many resources a bus, queue-adjacent storage area, or temporary holding point can currently accept.

The Capacity System controls:

* count ownership
* max limit enforcement
* safe add and remove operations

The game module controls:

* passenger meaning
* boarding rules
* matching logic
* scene behavior

---

# Responsibilities

The Capacity System is responsible for:

* Owning current count state
* Owning maximum count state
* Reporting remaining count
* Reporting whether tracked capacity is full
* Reporting whether tracked capacity is empty
* Validating add requests
* Validating remove requests
* Rejecting invalid count transitions
* Returning safe operation results

---

# Should Not Handle

The Capacity System should not handle:

* Resource storage
* Queue ordering
* Color matching
* Transfer eligibility
* Movement
* Animation
* Audio
* Haptics
* Puzzle-specific rules
* Win conditions
* Lose conditions
* Object spawning

---

# Design Requirements

## Generic Count-Based Limit Tracker

The Capacity System should be a generic count-based limit tracker.

It tracks how much is currently used against a maximum.

It should not assume what is being counted.

That allows the same system to represent:

* temporary storage usage
* occupancy limits
* transfer slots
* abstract gameplay counters

Status:

Approved

---

## Capacity Is Both Validator And State Owner

The Capacity System should both validate changes and own the authoritative limit state.

It owns:

* Current
* Max
* Remaining
* IsFull
* IsEmpty

Reason:

If validation lives in one place and state lives in another, the framework creates avoidable coupling and duplicated rules.

Status:

Approved

---

## Capacity Does Not Store Resources

The Capacity System must not store the resources it is limiting.

Buffer stores resources.

Queue manages order.

Capacity manages limits.

Reason:

This keeps responsibilities separate and allows the same limit tracker to work with different storage models.

Status:

Approved

---

## Authority Boundary

The Capacity System owns count and limit state, but it does not store resources.

The Capacity System does not know whether a Buffer or Queue mutation actually happened.

A coordinating owner must keep Capacity in sync with storage mutations when both describe the same logical container.

Capacity failure does not decide gameplay meaning.

It only reports that the requested count change was rejected.

Status:

Approved

---

## Interface-Based Contract

The framework should expose an interface-based contract and a default implementation.

Use:

* `ICapacity`
* `CapacityTracker`

Example:

```csharp
public interface ICapacity
{
}

public sealed class CapacityTracker : ICapacity
{
}
```

This keeps callers decoupled from one concrete implementation while still providing a standard framework default.

Status:

Approved

---

## Safe Result-Based Operations

Capacity-changing operations should return result data instead of relying on exceptions during normal gameplay flow.

Use:

* `CapacityResult`

Reason:

Add and remove requests may fail during expected runtime conditions such as overflow or underflow attempts.

Status:

Approved

---

## Invariant Enforcement

The Capacity System must enforce these invariants:

* `Current` cannot exceed `Max`
* `Current` cannot go below `0`
* setting `Max` below `Current` is rejected
* zero capacity is valid

These rules should be enforced by the system itself, not left to caller discipline.

Status:

Approved

---

# Core Operations

The Capacity System should support:

```text
TryAdd
TryRemove
TrySetMax
Clear
Current
Max
Remaining
IsFull
IsEmpty
```

## TryAdd

Attempts to increase the current count.

Why it matters:

Callers need a safe way to request capacity consumption without risking overflow.

MVP rule:

`TryAdd` rejects overflow.

## TryRemove

Attempts to decrease the current count.

Why it matters:

Callers need a safe way to release capacity without risking invalid negative counts.

MVP rule:

`TryRemove` rejects removing more than `Current`.

## TrySetMax

Attempts to change the maximum allowed count.

Why it matters:

Some systems may need to change limits during setup, progression, or rule changes.

MVP rule:

Setting `Max` below `Current` is rejected.

## Clear

Resets `Current` to `0`.

Why it matters:

This supports resets, teardown, restarts, and state cleanup.

## Current

Reports the currently used count.

Why it matters:

This is the primary state value consumed by other systems.

## Max

Reports the maximum allowed count.

Why it matters:

Other systems need the actual configured limit, not just remaining space.

## Remaining

Reports how much unused capacity remains.

Why it matters:

This is a direct high-frequency query for UI, rules, and validation.

## IsFull

Reports whether `Current == Max`.

Why it matters:

This avoids repeated caller-side comparisons.

## IsEmpty

Reports whether `Current == 0`.

Why it matters:

This is a direct high-frequency query for reset and transfer logic.

---

# Final API Decision

The Capacity System should expose an interface-based contract:

* `ICapacity`

The framework should also provide a default implementation:

* `CapacityTracker : ICapacity`

Use this API shape in the documentation:

```csharp
public interface ICapacity
{
    int Current { get; }
    int Max { get; }
    int Remaining { get; }
    bool IsFull { get; }
    bool IsEmpty { get; }

    CapacityResult TryAdd(int amount = 1);
    CapacityResult TryRemove(int amount = 1);
    CapacityResult TrySetMax(int max);
    void Clear();
}
```

`CapacityTracker` should own capacity state directly.

It should not require a buffer or queue to function.

---

# Capacity Result

Capacity-changing operations should return result data where failure is possible.

Example:

```csharp
public struct CapacityResult
{
    public bool Success;
    public int RequestedAmount;
    public int AppliedAmount;
    public int Current;
    public int Max;
    public int Remaining;
}
```

This allows callers to inspect the outcome safely after:

* overflow attempts
* underflow attempts
* rejected max changes

Example:

```text
TryAdd(1) when Current == Max
    ->
Success = false
    ->
Current remains unchanged
```

---

# Edge Cases

## Zero Capacity

Zero capacity is valid.

Meaning:

* `Max` may be `0`
* `Current` must also be `0`
* `Remaining` is `0`
* `IsFull` is true
* `IsEmpty` is true

Reason:

An unavailable or disabled container is still a valid state.

Status:

Approved for MVP

---

## Overflow Attempt

If a caller tries to add beyond `Max`:

* `TryAdd` should fail safely
* `Current` should remain unchanged
* `Remaining` should remain unchanged

The system should not partially apply the request.

---

## Underflow Attempt

If a caller tries to remove more than `Current`:

* `TryRemove` should fail safely
* `Current` should remain unchanged

The system should not partially apply the request.

---

## Invalid Max Reduction

If a caller tries to set `Max` below `Current`:

* `TrySetMax` should fail safely
* `Max` should remain unchanged

This prevents hidden truncation of state.

---

## Negative Request Values

If a caller requests a negative add, remove, or max value:

MVP rule:

Reject the operation.

Reason:

Allowing negative values would create ambiguous API meaning and encourage caller misuse.

Status:

Recommended for MVP

---

## Unlimited Capacity

Unlimited capacity should not be part of the MVP.

Reason:

Mixing finite and infinite semantics into the same first implementation increases branching and muddies invariants.

Unlimited capacity may be added later through:

* a specialized implementation
* a nullable or sentinel-based max model
* a no-limit capacity provider

Status:

Future extension only

---

# Relationship With Buffer System

Buffer System stores resources.

Capacity System tracks count-based limits.

These systems are often used together, but they should remain separate.

Example:

```text
Can this buffer accept another resource?
    ->
CapacitySystem.TryAdd(1)
    ->
If success:
        BufferSystem.Add(resource)
```

The Capacity System should not know what the buffer stores.

The Buffer System should not own capacity rules by default.

---

# Relationship With Queue System

Queue System manages order.

Capacity System manages limits.

Example:

```text
Queue Count informs capacity usage
    ->
Capacity System validates addition
    ->
Queue System stores order
```

A queue may use capacity rules, but queue behavior and capacity behavior should remain conceptually separate.

---

# Atomic Synchronization Risk

Using Buffer and Capacity together creates an important coordination risk.

If one system updates and the other does not, state becomes inconsistent.

Examples:

* `Capacity.TryAdd(1)` succeeds, but `Buffer.Add(resource)` fails
* `Buffer.Remove(resource)` succeeds, but `Capacity.TryRemove(1)` is never called
* external cleanup removes a resource from storage, but capacity state remains unchanged

This is the main architectural risk in the current design.

A simpler design would derive count directly from storage.

That is simpler when:

* one buffer maps to one capacity tracker
* each stored resource always counts as exactly one unit

However, a separate Capacity System is more reusable when:

* the same limit logic must work without a buffer
* one action consumes capacity without storing a resource directly
* count rules differ from raw storage count

Recommendation:

Keep Capacity separate, but require one coordinating owner whenever storage and capacity represent the same container.

That coordinating owner should:

* perform validation and storage change in one flow
* handle rollback or reject partial updates
* avoid exposing independent unsynchronized writes from multiple systems

Status:

Architectural concern acknowledged

---

# Runtime Flow Example

```text
Game wants to add a resource to temporary storage
    ->
CapacitySystem.TryAdd(1)
    ->
If failure:
        Stop
    ->
If success:
        BufferSystem.Add(resource)
    ->
If buffer add fails:
        CapacitySystem.TryRemove(1) as rollback
```

This flow should be owned by a higher-level system, not by Capacity itself.

---

# MVP Scope

The first version of the Capacity System should support:

* Interface-based contract
* Default `CapacityTracker` implementation
* Generic count-based tracking
* `Current`
* `Max`
* `Remaining`
* `IsFull`
* `IsEmpty`
* `TryAdd`
* `TryRemove`
* `TrySetMax`
* `Clear`
* Safe result return
* Zero-capacity support
* Overflow rejection
* Underflow rejection
* Rejected max reduction below current

---

# Future Extensions

The following features can be added later:

* Unlimited capacity
* Fractional capacity values
* Reservation support
* Capacity changed events
* Read-only capacity views
* Serializable capacity state
* Editor visualization
* Thread-safe or job-safe variants
* Composite capacity providers

---

# Open Questions

* Should `TrySetMax` be part of the runtime interface, or should some games treat max as immutable after construction?
* Should `CapacityResult` include a failure reason enum in the MVP, or is `Success` plus state snapshot enough?
* Should `Clear` be part of the shared interface, or should reset flows remain external?
* Should a future implementation support weighted adds where one resource consumes more than one capacity unit?
* When Buffer and Capacity are paired, should the framework later provide a coordinator abstraction instead of leaving synchronization entirely to game modules?

---

# Final Design Rule

The Capacity System tracks limits.

It owns count state.

It does not store resources.

It does not decide what counted resources mean.

Game-specific systems decide when and why capacity should be consumed or released.
