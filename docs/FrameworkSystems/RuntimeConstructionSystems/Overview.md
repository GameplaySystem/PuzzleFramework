# Runtime Construction Systems

## Document Metadata

Category:
- Runtime Construction Systems

Status:
- Future Scope / Not Started

Parent:
- None

Related Documents:
- ../ContentSystems/LevelDataSystem.md
- ../ContentSystems/LevelSaveLoadSystem.md
- ../ContentSystems/LevelEditorFoundation.md

Depends On:
- Level Data System
- Level Save Load System

Used By:
- Drop Away
- Color Block Jam
- Sky Rush
- Hole People
- Bus Jam

Runtime Construction Systems are responsible for converting loaded level definitions into runtime scene objects and runtime state.

They are not responsible for:

* saving/loading level files
* defining level data
* editing levels
* player progression
* puzzle-specific rules

They may coordinate:

* framework-level builders
* game-specific builder adapters
* runtime object factories
* validation before construction

Framework construction systems may define the socket or contract.

Game modules may provide adapters or builders for game-specific content.
