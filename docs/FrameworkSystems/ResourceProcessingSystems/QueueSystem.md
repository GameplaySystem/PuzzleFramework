# Queue System

## Document Metadata

Category:
- Resource Processing Systems

Status:
- Approved

Parent:
- Overview.md

Related Documents:
- Overview.md
- BufferSystem.md
- CapacitySystem.md

Depends On:
- None

Used By:
- Sky Rush
- Hole People
- Bus Jam

## Purpose

The Queue System manages ordered waiting.

It allows resources to enter, wait, peek, leave, or be processed in a predictable order.

The Queue System should be reusable across multiple puzzle games and should not contain game-specific rules.

---

# Core Design Idea

A queue controls order.

A queue does not decide meaning.

```text
Resource enters queue
    ↓
Queue stores resource in order
    ↓
Caller requests next resource
    ↓
Queue returns resource data
    ↓
Game-specific system decides what happens
```

---

# Example Uses

## Sky Rush

Door queues store stickmen waiting to board buses.

The Queue System controls:

* waiting order
* next passenger access
* removing passengers from queue

The game module controls:

* passenger color matching
* bus capacity
* boarding animation
* win/loss rules

---

## Bus Jam

Buffered or waiting stickmen may be processed in order.

The Queue System controls:

* who is first
* who is next
* whether the queue is empty

The game module controls:

* whether the next bus accepts that stickman
* whether the buffer is full
* whether the level is failed

---

## Hole People

Hole queues or people queues may process resources in order.

The Queue System controls:

* ordering
* enqueue/dequeue
* peek next resource

The game module controls:

* hole color rules
* buffer transfer rules
* capacity rules

---

# Responsibilities

The Queue System is responsible for:

* Adding resources to the back of a queue
* Removing resources from the front of a queue
* Peeking at the next resource without removing it
* Checking whether the queue is empty
* Reporting queue count
* Clearing the queue
* Preserving order
* Returning operation results

---

# Should Not Handle

The Queue System should not handle:

* Color matching
* Capacity rules
* Boarding rules
* Collection rules
* Movement
* Animation
* Sound effects
* Haptics
* Win conditions
* Lose conditions
* Object spawning
* Game-specific resource meaning

---

# Design Requirements

## FIFO By Default

The default queue behavior is FIFO.

FIFO means:

```text
First In
First Out
```

The first resource added should be the first resource removed.

Status:

Approved

---

## Generic Resource Type

The Queue System should operate on generic resources.

It should not require concrete puzzle classes.

Example:

```csharp
public interface IResourceQueue<T>
{
}

public class ResourceQueue<T> : IResourceQueue<T>
{
}
```

The framework should expose an interface-based contract and also provide a default FIFO implementation.

This allows the same queue system to store different resource types depending on the game while keeping callers decoupled from a specific implementation.

Status:

Approved

---

## Peek Support

The Queue System must support peeking.

Peeking means checking the next resource without removing it.

Example use:

```text
Can the next passenger board this bus?
```

The game module can inspect the next resource before deciding whether to dequeue it.

Status:

Approved

---

## Safe Dequeue

The Queue System should not throw errors when trying to dequeue from an empty queue.

It should return a result instead.

Example:

```csharp
public struct QueueResult<T>
{
    public bool Success;
    public T Value;
}
```

Status:

Approved

---

## No Game-Specific Validation

The Queue System should not decide whether a resource is allowed to leave the queue.

It should expose queue operations.

The caller decides when to call them.

Status:

Approved

---

# Core Operations

The Queue System should support:

```text
Enqueue
Dequeue
Peek
Clear
Contains
Count
IsEmpty
```

# Final API Decision

The Queue System should expose an interface-based contract:

- `IResourceQueue<T>`

The framework should also provide a default FIFO implementation:

- `ResourceQueue<T> : IResourceQueue<T>`

Use this API shape in the documentation:

```csharp
public interface IResourceQueue<T>
{
    int Count { get; }
    bool IsEmpty { get; }

    void Enqueue(T resource);
    QueueResult<T> Dequeue();
    QueueResult<T> Peek();
    bool Contains(T resource);
    void Clear();
}
```

`Contains(T resource)` should remain as a utility method.

It should not be treated as a required gameplay dependency.

It exists for:

* validation
* debugging
* editor checks
* optional caller logic

The default `ResourceQueue<T>` should behave like a normal FIFO queue.

---

# Queue Result

Queue operations should return result data where failure is possible.

Example:

```csharp
public struct QueueResult<T>
{
    public bool Success;
    public T Value;
}
```

This avoids unsafe behavior when the queue is empty.

Example:

```text
Dequeue empty queue
    ↓
Success = false
    ↓
Value = default
```

---

# Edge Cases

## Empty Queue

If the queue is empty:

* Peek should return failure.
* Dequeue should return failure.
* Count should be 0.
* IsEmpty should be true.

The system should not throw an exception during normal gameplay flow.

---

## Null Resource

If the resource type allows null, the queue should decide whether null values are allowed.

MVP decision:

Null resources are not allowed for reference types.

Reason:

Null queued objects create unclear behavior and make debugging harder.

Status:

Approved for MVP

---

## Duplicate Resources

Duplicate values are allowed by default.

Same-property objects are not considered duplicates.

Two separate objects with the same data may exist in the queue.

Reason:

The default `ResourceQueue<T>` should behave like a normal FIFO queue and should not assume resources are unique.

Duplicate-prevention is not part of the MVP.

If a game requires uniqueness, that should be handled by the caller or a future `UniqueResourceQueue<T>` variant.

Status:

Approved for MVP

---

## Resource Removed Elsewhere

A resource may be destroyed, collected, or invalidated outside the queue.

The Queue System should not automatically know this.

The caller is responsible for removing invalid resources or rebuilding the queue.

Future versions may support validation callbacks.

---

## Queue Modified During Processing

A queue may be modified while another system is processing it.

MVP rule:

Avoid modifying the same queue while iterating over it.

Processing systems should use controlled operations like Peek and Dequeue.

---

## Queue Capacity

The Queue System itself should not own capacity rules by default.

Capacity belongs to the Capacity System.

A queue can report Count, and another system can decide whether more resources are allowed.

```text
Queue Count
    ↓
Capacity System
    ↓
Can Add?
```

---

# Relationship With Capacity System

Queue System controls order.

Capacity System controls limits.

Example:

```text
Can this queue accept another resource?
    ↓
Capacity System checks limit
    ↓
If yes:
        QueueSystem.Enqueue(resource)
```

The Queue System may later support optional capacity, but the MVP should keep capacity separate.

---

# Relationship With Buffer System

Queue System controls ordered waiting.

Buffer System controls temporary storage slots.

A buffer may use queues internally, but they are conceptually different.

Queue:

```text
Ordered line
```

Buffer:

```text
Temporary holding area
```

---

# Runtime Flow Example

```text
Game wants to process next resource
    ↓
QueueSystem.Peek()
    ↓
If no resource:
        Stop
    ↓
Game checks rules
    ↓
If allowed:
        QueueSystem.Dequeue()
        Game processes resource
    ↓
If not allowed:
        Resource remains in queue
```

---

# MVP Scope

The first version of the Queue System should support:

* Interface-based contract
* Default FIFO implementation
* Generic queue type
* FIFO behavior
* Enqueue
* Dequeue
* Peek
* Clear
* Count
* IsEmpty
* Contains
* Safe result return
* Empty queue handling
* Duplicate resources allowed
* Null resources rejected for reference types

---

# Future Extensions

The following features can be added later:

* Priority queues
* `UniqueResourceQueue<T>`
* Capacity-limited queues
* Queue changed events
* Queue item validation
* Remove specific item
* Insert at index
* Move item within queue
* Serializable queue data
* Editor queue preview
* Queue debug visualization
* Async queue processing
* Queue reservations

---

# Open Questions

* Should queues be pure C# classes only, or should we also provide MonoBehaviour queue components for scene usage?
* Should the Queue System support removing a specific resource in the MVP?
* Should queue changes raise events immediately, or should events be added later?
* Should queue order be serializable for level save/load?
* Should capacity remain fully separate, or should queues optionally accept a capacity provider later?

---

# Final Design Rule

The Queue System determines order.

It does not determine eligibility.

It does not decide what queued resources mean.

Game-specific systems decide when and why a resource should be processed.
