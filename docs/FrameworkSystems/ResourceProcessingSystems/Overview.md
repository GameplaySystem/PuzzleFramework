# Resource Processing Systems

## Document Metadata

Category:
- Resource Processing Systems

Status:
- Approved

Parent:
- None

Related Documents:
- QueueSystem.md
- BufferSystem.md
- CapacitySystem.md

Depends On:
- None

Used By:
- Drop Away
- Sky Rush
- Hole People
- Bus Jam

## Purpose

Resource Processing Systems define reusable framework-level tools for storing, ordering, and limiting resources during gameplay flow.

These systems exist so games can move resources through predictable shared mechanics without embedding puzzle-specific rules inside framework code.

This category does not define what resources mean.

It defines how resources can wait, be stored temporarily, and be constrained by limits.

---

# Why This Category Exists

Many puzzle games need the same kinds of resource-handling behavior:

* ordered waiting
* temporary holding
* count limits
* safe add and remove validation

Those needs appear across multiple target games, but the meaning of each resource differs from game to game.

Examples:

* a waiting passenger
* a buffered stickman
* a bus occupancy slot
* a hole collection limit

The framework should not hardcode any of those meanings.

Instead, it should provide generic systems that game modules can combine into puzzle-specific flows.

This category exists to separate:

* shared processing mechanics

from:

* game-specific rules and interpretation

---

# Systems In This Category

This category currently contains three systems:

1. Queue System
2. Buffer System
3. Capacity System

Each system solves a different problem.

## Queue System

The Queue System manages order.

Its primary concerns are:

* FIFO behavior
* waiting resources
* next-item access
* safe dequeue and peek operations

The Queue System answers questions like:

```text
Who is next?
Who has been waiting first?
Is there anything waiting?
```

## Buffer System

The Buffer System manages temporary storage.

Its primary concerns are:

* slot-based storage
* temporary holding
* resource lookup
* resource retrieval

The Buffer System answers questions like:

```text
What is currently stored?
Is this resource present?
Can I retrieve this stored resource?
```

## Capacity System

The Capacity System manages limits.

Its primary concerns are:

* current count
* maximum count
* remaining count
* safe add and remove validation

The Capacity System answers questions like:

```text
Can another resource be accepted?
Is this storage full?
Can this count be reduced safely?
```

---

# Queue vs Buffer vs Capacity

The framework should keep these responsibilities distinct.

Queue:

* manages order
* uses FIFO behavior by default
* represents waiting resources

Buffer:

* manages temporary storage
* uses slot-based storage
* supports lookup and retrieval
* does not manage ordering
* does not manage capacity

Capacity:

* manages limits
* owns `Current` and `Max`
* validates additions and removals
* does not store resources

In short:

```text
Queue
-> manages order

Buffer
-> manages temporary storage

Capacity
-> manages limits
```

The important boundary is this:

* Queue is about sequence.
* Buffer is about presence.
* Capacity is about allowed quantity.

## Queue vs Buffer Distinction

Use Queue when next-item order matters.

Use Buffer when presence, lookup, or temporary storage matters.

Do not use Buffer to imply FIFO.

Do not use Queue as random-access storage.

---

# System Relationships

These systems are designed to work together without collapsing into one shared abstraction.

High-level resource flow may look like this:

```text
Resource
    ↓
Queue (optional)
    ↓
Buffer (optional)
    ↓
Destination
```

Capacity may be attached to any storage-like system, but it is not itself part of the transfer flow.

A more complete view is:

```text
Resource
    ↓
Queue (optional)
    ↓
Buffer (optional)
    ↓
Destination

Capacity
    -> validates whether a queue, buffer, destination, or other counted state can accept more
```

Example relationships:

* A queue may use capacity checks before enqueue.
* A buffer may use capacity checks before add.
* A destination may use capacity checks before accepting transfer.

## Coordination Rule

When Queue, Buffer, and Capacity are combined to represent one logical gameplay container, one owner must coordinate their operations.

That owner may be:

* a game module system
* a container adapter
* a gameplay controller
* a future framework wrapper if repeated across multiple games

Queue, Buffer, and Capacity should not be mutated independently by unrelated callers when they represent the same logical container.

## Ownership Rule

A logical gameplay container has one coordinating owner.

The owner may internally use:

* Queue for order
* Buffer for temporary storage
* Capacity for limits

Game modules define meaning.

Framework systems provide mechanics.

## Capacity Attachment Semantics

Capacity may constrain:

* queues
* buffers
* destinations
* abstract counters

Capacity is optional.

`Queue.Count` or `Buffer.Count` and `Capacity.Current` are not automatically the same concept unless the coordinating owner defines them that way.

## Standard Constrained Storage Flow

Recommended add flow:

* Check `Capacity.CanAdd` or call `TryAdd`.
* Mutate storage only if capacity allows it.
* If capacity was mutated first and storage fails, rollback capacity.

Recommended remove flow:

* Remove from storage first.
* If storage removal succeeds, update capacity.
* Do not release capacity if storage was not changed.

Architecture note:

There is one real overlap risk between Buffer and Capacity.

If a buffer stores resources and a capacity tracker limits that same container, both systems must be updated together by a higher-level owner.

Otherwise:

* buffer state
* capacity state

can drift out of sync.

That does not invalidate the architecture, but it does mean game modules or coordinating framework layers must own the combined flow carefully.

---

# Example Game Usage

## Bus Jam

Bus Jam may combine these systems as follows:

* Queue for buses
* Buffer for waiting stickmen
* Capacity for bus occupancy

The framework systems provide:

* ordered waiting
* temporary holding
* count validation

The game module decides:

* which passenger can board
* whether boarding is allowed
* matching rules
* failure and completion logic

---

## Hole People

Hole People may combine these systems as follows:

* hole queues
* stickman buffers
* hole capacity

The framework systems provide:

* queue ordering
* temporary storage
* occupancy limits

The game module decides:

* which hole accepts which stickman
* collection rules
* pathing outcomes
* success and failure meaning

---

## Sky Rush

Sky Rush may combine these systems as follows:

* door queues
* bus capacity

The framework systems provide:

* waiting order
* boarding limit checks

The game module decides:

* which bus accepts which passenger
* when transfer occurs
* animation and rule outcomes

---

## Drop Away

Drop Away may use this category more narrowly:

* hole collection capacity

The framework system provides:

* count tracking
* limit validation

The game module decides:

* what collection means
* when the hole can accept a match
* puzzle completion behavior

---

# What Does Not Belong Here

The following do not belong inside Resource Processing Systems:

* Color matching
* Puzzle rules
* Win conditions
* Lose conditions
* Animation
* Movement
* Audio
* Game-specific logic

More specifically:

* Queue should not decide whether a resource is eligible.
* Buffer should not decide why a resource is stored or retrieved.
* Capacity should not decide what a counted resource means.

Game modules may use these systems together, but game modules must define puzzle-specific meaning.

## Anti-Patterns

Avoid these patterns:

* Do not let multiple unrelated systems write to the same buffer/capacity pair.
* Do not infer FIFO order from Buffer insertion order.
* Do not treat Capacity as resource storage.
* Do not duplicate gameplay eligibility checks inside Queue, Buffer, or Capacity.
* Do not make Queue, Buffer, or Capacity aware of game-specific concepts.

---

# Final Design Rule

Resource Processing Systems define shared mechanics for:

* ordering
* temporary storage
* limits

They do not define puzzle meaning.

Queue manages order.

Buffer manages temporary storage.

Capacity manages limits.

Game modules decide when to use them, why resources move, and what those resources mean.
