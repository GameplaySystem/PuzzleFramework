# Perspective Board Camera Framing System

## Document Metadata

Category:
- Presentation Systems

Status:
- Approved

Parent:
- Overview.md

Related Documents:
- Overview.md
- ../CoreBoardSystems/GridSystem.md
- ModularBoardVisualSystem.md

Depends On:
- ../CoreBoardSystems/GridSystem.md

Used By:
- Drop Away
- Color Block Escape

## Purpose

The Perspective Board Camera Framing System positions a perspective camera so a padded logical
board rectangle fits inside the current camera aspect ratio. It supports any orthogonal
`GridWorldLayout`, including DTM's center-anchored XZ board and CBE's corner-anchored XZ board.

The system exists because both prototypes need the same resolution-independent board framing.
Games still choose camera rotation, field of view, padding, minimum distance, viewport and when a
resize or level change should trigger reframing.

## Contract

`PerspectiveBoardCameraPositioner.TryPosition`:

- requires a perspective camera and positive logical board dimensions
- derives the complete rectangular board center and extents from `GridWorldLayout`
- accounts for the camera's vertical field of view and current aspect ratio
- evaluates every padded board corner in camera-local space
- moves only the camera position along its current forward axis
- preserves camera rotation, field of view, clipping configuration and gameplay state
- reports invalid projection/layout inputs without changing the board

Center and corner anchors require different center offsets. A center-anchored cell-zero layout uses
`(dimension - 1) / 2`; a corner-anchored layout uses `dimension / 2`. The shared implementation
must not assume one prototype's anchor convention.

## Ownership Boundary

Framework owns the projection-independent framing calculation and camera-position application.
Game modules own presentation choices such as a 60-degree pitch, portrait composition, UI-safe
padding, transitions and camera animation. Camera framing must not mutate grid coordinates,
occupancy, authored content, movement or outcome state.

## Deliberate Limits

The first shared contract fits the logical board plane. It does not automatically account for
safe-area UI, tall decorative meshes, animated camera transitions, Cinemachine, perspective lens
shift or obstacle occlusion. Those need concrete reuse before entering the framework.

