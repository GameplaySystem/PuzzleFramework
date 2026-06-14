# Vision

Build a reusable Unity Puzzle Framework by deconstructing and recreating several successful puzzle games.

The framework should prioritize:

- Clean architecture
- Reusability
- Scalability
- Maintainability
- Documentation quality
- Portfolio value
- Learning software engineering fundamentals

The project should improve:

- Unity skills
- C# skills
- Architecture skills
- System design skills
- Technical documentation skills
- Problem solving skills

---

# Project Goals

## Primary Goals

- Build a reusable puzzle framework.
- Create portfolio-quality projects.
- Improve software engineering fundamentals.
- Improve systematic thinking.
- Become capable of designing systems before implementation.
- Increase employability for Unity gameplay programming positions.

## Secondary Goals

- Learn GitHub workflows.
- Learn AI-assisted development workflows.
- Learn professional documentation practices.
- Create material suitable for technical interviews.

---

# Target Games

## Drop Away

Grid-based puzzle game where colored holes collect matching colored stickmen.

Status:

- Deconstructed

## Color Block Jam

Grid-based puzzle game where colored bricks exit through matching colored doors.

Status:

- Deconstructed

## Sky Rush Traffic Puzzle

Grid-based puzzle game where buses collect stickmen from door queues.

Status:

- Deconstructed

## Hole People

Pathfinding-based puzzle game using board holes, buffers, and hole queues.

Status:

- Deconstructed

## Bus Jam

Pathfinding-based puzzle game using buses, buffers, and stickman selection.

Status:

- Deconstructed

---

# Development Philosophy

- Understand before implementing.
- Requirements before architecture.
- Architecture before code.
- Shared systems before game-specific systems.
- Framework systems should remain game-agnostic.
- Game modules should define rules.
- Documentation should be created before implementation.
- Decisions should be documented.

---

# Major Architecture Decisions

## Framework Rule

A system belongs in the framework if it is used by at least two games.

Status:

Approved

## Pathfinding

A* will be used as the primary pathfinding algorithm.

Status:

Approved

## Shape Rotation

Shapes may rotate inside the level editor.

Shapes do not rotate during gameplay.

Status:

Approved

## Board Structure

Boards are cell-based grids.

Status:

Approved

## Cell Types

Inactive Cell:

Not part of the board.

Blocked Cell:

Part of the board but cannot be occupied or traversed.

Status:

Approved

---

# Framework Categories

## Core Board Systems

- Grid System
- Cell Occupancy System
- Shape System
- Wall Generation System
- Pathfinding System

## Runtime Flow Systems

- Game State System
- Timer System
- Event System

## Interaction Systems

- Input System
- Drag Movement System
- Grid Snap System

## Resource Processing Systems

- Queue System
- Buffer System
- Capacity System

## Content Systems

- Level Data System
- Level Save Load System
- Level Editor Foundation

Content Systems handle level content data, level content save/load, and level editor foundation.

Content Systems do not instantiate runtime objects.

## Runtime Construction Systems

- Level Runtime Builder System
- Runtime Object Factory System
- Runtime Construction Validation System

Runtime Construction Systems are responsible for turning loaded level data into runtime objects.

Runtime Construction Systems are future scope and are not current focus.

## Progression Systems

- Player Progress Data System
- Progress Save Load System

## Presentation Systems

- Color System
- Visual Feedback System

---

# Current Progress

## Game Design

- Drop Away [done]
- Color Block Jam [done]
- Sky Rush [done]
- Hole People [done]
- Bus Jam [done]

## Technical Design

### Shared Systems Analysis

- Complete [done]

### Core Board Systems

- Grid System [done]
- Cell Occupancy System [done]
- Shape System [done]
- Wall Generation System [done]
- Pathfinding System [done]

### Runtime Flow Systems

- Game State System [done]
- Timer System [done]
- Event System [done]

### Interaction Systems

- Input System [done]
- Drag Movement System [done]
- Grid Snap System [done]

### Resource Processing Systems

- Queue System [done]
- Buffer System [done]
- Capacity System [done]

### Content Systems

- Level Data System [done]
- Level Save Load System [done]
- Level Editor Foundation [done]

### Presentation Systems

- Color System [done]
- Visual Feedback System [done]

### Runtime Construction Systems (Future Scope / Not Started)

- Level Runtime Builder System
- Runtime Object Factory System
- Runtime Construction Validation System

### Progression Systems (Future Scope / Not Started)

- Player Progress Data System
- Progress Save Load System

---

# Current Focus

Designing and documenting framework systems before implementation.

Current category:

Runtime Construction Systems

Current system:

Review and analysis

Runtime Construction Systems remain future implementation scope.

---

# Next Steps

1. Runtime Construction Systems review/analysis
2. Framework Architecture Diagram
3. Implementation Roadmap
4. Framework Implementation
5. Drop Away Prototype
6. Remaining Prototypes

Progression Systems are future scope and are not part of the current focus.

---

# Open Questions

- Should Bus Jam buses use Shape System or remain road-only entities?
- Should Door System be framework-level?
- Should Visual Feedback remain framework-level or game-specific?
- How should wall generation interact with doors?
- Should occupancy be stored centrally or inside cells?

---

# Chat Migration Instructions

When continuing this project in a new ChatGPT conversation:

1. Provide this document.
2. Provide current progress.
3. Provide the current focus section.
4. Continue from the latest unfinished task.

This document is the source of truth for the project.
