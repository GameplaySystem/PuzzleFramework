# Buffer System

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
- CapacitySystem.md

Depends On:
- None

Used By:
- Hole People
- Bus Jam

## Purpose

The Buffer System manages temporary resource storage.

It allows resources to be placed into temporary slots, queried while waiting, and removed when another system requests them.

The Buffer System should be reusable across multiple puzzle games and should not contain game-specific rules.

---

# Core Design Idea

A buffer stores resources temporarily.

A buffer does not decide meaning.

```text
Resource enters buffer
    â†“
Buffer stores resource in a slot
    â†“
Caller queries or requests resource
    â†“
Buffer returns resource data
    â†“
Game-specific system decides what happens
```

---

# Example Uses

## Bus Jam

A temporary holding area may store resources while another system decides when they can transfer.

The Buffer System controls:

* temporary storage
* slot occupancy
* finding stored resources
* removing stored resources

The game module controls:

* which resource may leave next
* whether transfer is allowed
* matching rules
* completion and failure rules

---

## Hole People

A temporary holding area may store resources before they are transferred to another destination.

The Buffer System controls:

* storing resources in slots
* checking whether resources are present
* finding one or more stored resources
* clearing temporary storage

The game module controls:

* when transfer should happen
* whether a resource is eligible to move
* queue interaction rules
* puzzle-specific outcomes

---

# Responsibilities

The Buffer System is responsible for:

* Storing resources temporarily
* Managing slot occupancy
* Adding resources
* Removing resources
* Finding resources
* Querying resources
* Reporting capacity and occupancy information

---

# Should Not Handle

The Buffer System should not handle:

* Color matching
* Transfer eligibility
* Queue ordering rules
* Win conditions
* Lose conditions
* Movement
* Animation
* Audio
* Haptics
* Puzzle-specific logic

---

# Design Requirements

## Buffer Definition

A Buffer is temporary slot-based storage that holds resources until another system or game rule requests their transfer.

The Buffer System stores resources.

It does not decide why they are stored or when they should leave.

Status:

Approved

---

## Slot-Based Storage

The Buffer System should be slot-based.

Resources occupy temporary storage slots rather than an inherently ordered line.

This means the primary concern is:

```text
Which slots are occupied?
Which slots are free?
Which resources are currently stored?
```

Not:

```text
Who is first?
Who is next?
```

Status:

Approved

---

## Generic Resource Type

The Buffer System should operate on generic resources.

It should not require concrete puzzle classes.

Example:

```csharp
public class Buffer<T>
{
}
```

This allows the same buffer system to store different resource types depending on the game while remaining framework-level and reusable.

Status:

Approved

---

## No Game-Specific Validation

The Buffer System should not decide whether a resource is allowed to enter or leave.

It should expose storage operations and query operations.

The caller decides when and why to use them.

Status:

Approved

---

## Order Is Not Inherently Important

The Buffer System should not assume FIFO behavior by default.

Unlike a queue, a buffer is temporary storage where slot occupancy matters more than ordering.

If a specific game needs ordered behavior inside a temporary holding area, that behavior should be handled by the game module or by combining the Buffer System with the Queue System.

Status:

Approved

---

# Queue vs Buffer

The framework should distinguish clearly between queues and buffers.

Queue:

* Ordered waiting
* FIFO

Buffer:

* Temporary storage
* Slot-based
* Order is not inherently important

Example:

```text
Queue:
Resource A
then Resource B
then Resource C

Buffer:
Slot 1 = occupied
Slot 2 = empty
Slot 3 = occupied
```

---

# Core Operations

The Buffer System should support:

```text
Add
Remove
Contains
Find
FindAll
Clear
Count
IsEmpty
```

## Add

Adds a resource into temporary storage.

Why it matters:

The buffer must be able to accept resources from gameplay systems, transfer systems, or setup systems.

## Remove

Removes a stored resource from temporary storage.

Why it matters:

Another system must be able to request that a stored resource leave the buffer.

## Contains

Checks whether a specific resource is currently stored.

Why it matters:

Useful for validation, debugging, editor checks, and optional caller logic.

## Find

Returns one stored resource matching a condition or lookup request.

Why it matters:

Some games need to locate a temporary resource without relying on ordering.

## FindAll

Returns all stored resources matching a condition or lookup request.

Why it matters:

Some games need to inspect multiple temporary resources at once.

## Clear

Removes all stored resources.

Why it matters:

Useful for resets, level restart, teardown, and editor workflows.

## Count

Reports how many resources are currently stored.

Why it matters:

Other systems may need occupancy information for rules or diagnostics.

## IsEmpty

Reports whether the buffer currently stores no resources.

Why it matters:

This is a simple, high-frequency query used by calling systems.

---

# Edge Cases

## Empty Buffer

If the buffer is empty:

* Count should be 0.
* IsEmpty should be true.
* Contains should return false.
* Find should return no match.
* FindAll should return an empty result.
* Remove should fail safely if the target resource is not present.

The system should not throw an exception during normal gameplay flow.

---

## Resource Not Present

If a caller tries to remove or find a resource that is not stored:

* Remove should fail safely.
* Contains should return false.
* Find should return no match.

The Buffer System should report state clearly rather than assuming the caller is correct.

---

## Duplicate Resources

The buffer may contain duplicate values by default.

Reason:

The Buffer System should not assume resources are unique unless a specific game requires that rule.

If uniqueness becomes necessary later, that should be handled by the caller or a future specialized buffer variant.

Status:

Approved for MVP

---

## Null Resource

If the resource type allows null, the buffer should decide whether null values are allowed.

MVP decision:

Null resources are not allowed for reference types.

Reason:

Null stored objects create unclear state and make debugging harder.

Status:

Approved for MVP

---

## Resource Invalidated Elsewhere

A stored resource may be destroyed, collected, or invalidated outside the buffer.

The Buffer System should not automatically know this.

The caller is responsible for removing invalid resources or rebuilding the buffer state.

Future versions may support validation callbacks or cleanup helpers.

---

# Relationship With Capacity System

Buffer System stores resources.

Capacity System determines limits.

The Buffer System may report occupancy and slot usage information.

The Capacity System or another caller decides whether additional resources are allowed.

Example:

```text
Buffer Count
    â†“
Capacity System checks limit
    â†“
Can Add?
```

Capacity rules should remain outside the Buffer System.

## When Paired With Capacity

When a Buffer is paired with Capacity, the Buffer owns temporary storage but does not own authoritative limit rules.

`Buffer.Count` describes stored resources.

`Capacity.Current` describes tracked usage.

These values must be synchronized by the coordinating owner when they describe the same logical container.

The Buffer System should not update Capacity directly in the MVP.

---

# Relationship With Queue System

Queue System manages order.

Buffer System manages temporary storage.

A Buffer may internally use a queue implementation in some games, but conceptually they are different systems.

Queue:

```text
Ordered waiting
```

Buffer:

```text
Temporary slot-based storage
```

---

# Runtime Flow Example

```text
Game wants to store a temporary resource
    â†“
BufferSystem.Add(resource)
    â†“
Resource remains stored
    â†“
Game later checks whether transfer should happen
    â†“
BufferSystem.Contains(resource)
or
BufferSystem.Find(...)
    â†“
If allowed:
        BufferSystem.Remove(resource)
        Game transfers resource
    â†“
If not allowed:
        Resource remains in buffer
```

---

# MVP Scope

The first version of the Buffer System should support:

* Generic buffer type
* Temporary slot-based storage
* Add
* Remove
* Contains
* Find
* FindAll
* Clear
* Count
* IsEmpty
* Occupancy reporting
* Empty buffer handling
* Duplicate resources allowed
* Null resources rejected for reference types

---

# Future Extensions

The following features can be added later:

* Slot reservation
* Priority buffers
* Visual slot mapping
* Buffer events
* Serializable buffer state
* Editor visualization

---

# Open Questions

* Should the Buffer System expose explicit slot identifiers in the MVP, or should slot ownership remain internal at first?
* Should Find return the first matching stored resource, or should callers always prefer FindAll for deterministic filtering?
* Should removal return result data describing success or failure in the first implementation?
* Should buffers remain pure C# classes only, or should scene-facing wrappers exist later for tooling and debug views?
* Should a future specialized buffer support unique-only storage, or should uniqueness always stay outside the framework?

---

# Final Design Rule

The Buffer System stores resources temporarily.

It does not determine eligibility.

It does not determine order by default.

It does not decide what buffered resources mean.

Game-specific systems decide when and why a resource should be stored or removed.
