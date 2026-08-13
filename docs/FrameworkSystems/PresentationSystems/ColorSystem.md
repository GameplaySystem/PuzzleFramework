# Color System

## Document Metadata

Category:
- Presentation Systems

Status:
- Approved

Parent:
- Overview.md

Related Documents:
- Overview.md
- VisualFeedbackSystem.md

Depends On:
- Gameplay color identity sources

Used By:
- Drop Away
- Color Block Jam
- Sky Rush
- Hole People
- Bus Jam

## Purpose

The Color System defines reusable color identities and maps them to player-facing visuals.

It exists so multiple games can present color consistently without pushing puzzle-specific meaning into framework code.

The Color System is a framework-level presentation and data mapping system.

---

# Core Design Idea

The framework should define a stable shared identity set that can scale across multiple
games and editor tooling.

The current approved direction is ten framework-safe color slots:

* `Slot0`
* `Slot1`
* `Slot2`
* `Slot3`
* `Slot4`
* `Slot5`
* `Slot6`
* `Slot7`
* `Slot8`
* `Slot9`

Legacy aliases such as:

* `Red`
* `Blue`
* `Green`
* `Yellow`

may remain available for compatibility where existing prototype content already uses them.

The framework may then map those identities to visuals such as:

* colors
* materials
* palette values
* theme values
* visual identifiers

Important boundary:

Color identity is framework-level.

Color meaning is game-module-level.

That means the framework can define `Slot0` or a legacy alias such as `Red`, but only a game
module decides whether that identity means:

* a red stickman
* a red bus
* a red door
* a red brick
* a red collection target

Concrete visual assets remain owned by game modules.

The Color System provides reusable color identity and presentation mapping, not game-specific content.

---

# Responsibilities

The Color System is responsible for:

* defining reusable color identities
* mapping color identities to visual colors, materials, palette values, theme values, or visual identifiers
* supporting consistent palettes across games
* providing framework-safe color references
* allowing game modules to assign meaning to colors

The system helps presentation stay coherent without creating gameplay coupling.

---

# Should Not Handle

The Color System should not handle:

* color matching rules
* collection logic
* boarding logic
* exit logic
* blocking logic
* win conditions
* lose conditions
* object ownership
* progression meaning

The Color System must not know concrete game concepts like:

* Hole
* Stickman
* Bus
* Door
* Brick

Those concepts belong to game modules.

---

# Framework vs Game Module Ownership

Framework ownership:

* color identity definitions
* safe shared color references
* mapping patterns from identity to visual output

Game module ownership:

* what a given color means in gameplay
* what kinds of game objects use that color
* puzzle-specific rules involving color

This separation keeps the color layer reusable.

---

# Data Flow

A typical conceptual flow is:

```text
Gameplay object or data exposes color identity
    ->
Color System resolves visual mapping
    ->
Renderer, material, UI, or other presentation layer uses mapped visual
```

The Color System does not decide whether the underlying gameplay identity is valid.

It only maps identity to presentation.

---

# Example Usage

## Drop Away

The framework may provide `Slot0` with a compatible `Red` alias.

The Drop Away game module may use that identity for red stickmen and red holes.

The Color System maps the identity to visuals.

The game module decides whether those two things match.

## Color Block Jam

The framework may provide shared color identities.

The game module decides that those identities belong to bricks and doors.

The Color System presents them consistently.

The game module decides exit rules.

## Sky Rush

The framework may reuse color identities across buses and passengers.

The Color System ensures visual consistency.

The game module decides matching, boarding, and level logic.

---

# Edge Cases

## Same Identity, Different Meaning

The same framework color identity may represent different kinds of objects across games.

This is acceptable.

That is the intended separation between shared presentation identity and game-specific meaning.

## Palette Changes Across Games

Different games may need different palettes or visual styles while still using the same abstract color identities.

The Color System should allow that mapping flexibility without changing gameplay meaning.

## Color Overreach

A common failure mode is letting the Color System answer gameplay questions because colors are involved in the rules.

That is incorrect.

The moment the system decides whether colors match or what that match means, it has crossed into gameplay logic.

---

# MVP Scope

The first version of the Color System should support:

* ten reusable shared color identities
* consistent visual mapping
* framework-safe color references
* game-module-defined color meaning
* compatibility with existing prototype color names where required

---

# Future Extensions

The following can be added later if needed:

* theme overrides
* palette packs
* accessibility variants
* alternate UI color mappings
* editor palette support

These are presentation improvements only.

They should not change the rule that color meaning belongs to game modules.

---

# Final Design Rule

The framework may define color identity.

The game module defines color meaning.

The Color System maps identity to presentation.

It does not decide gameplay logic.
