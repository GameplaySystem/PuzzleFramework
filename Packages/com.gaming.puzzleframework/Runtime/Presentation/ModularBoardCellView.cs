using System.Collections.Generic;
using PuzzleFramework.CoreBoard;
using UnityEngine;

namespace PuzzleFramework.Presentation
{
    /// <summary>
    /// Passive prefab adapter for the reusable modular board visual profile.
    /// Child transforms and rotations remain owned by the concrete prefab.
    /// </summary>
    [AddComponentMenu("Puzzle Framework/Presentation/Modular Board Cell View")]
    [DisallowMultipleComponent]
    public sealed class ModularBoardCellView : MonoBehaviour
    {
        [Header("Cell")]
        [SerializeField] private GameObject cellBase;

        [Header("North Half Walls")]
        [SerializeField] private GameObject northWestHalfWall;
        [SerializeField] private GameObject northEastHalfWall;

        [Header("East Half Walls")]
        [SerializeField] private GameObject eastNorthHalfWall;
        [SerializeField] private GameObject eastSouthHalfWall;

        [Header("South Half Walls")]
        [SerializeField] private GameObject southEastHalfWall;
        [SerializeField] private GameObject southWestHalfWall;

        [Header("West Half Walls")]
        [SerializeField] private GameObject westSouthHalfWall;
        [SerializeField] private GameObject westNorthHalfWall;

        [Header("Convex / Default Corner Pillars")]
        [SerializeField] private GameObject convexNorthEast;
        [SerializeField] private GameObject convexSouthEast;
        [SerializeField] private GameObject convexSouthWest;
        [SerializeField] private GameObject convexNorthWest;

        [Header("Concave / Inner L Corners")]
        [SerializeField] private GameObject concaveNorthEast;
        [SerializeField] private GameObject concaveSouthEast;
        [SerializeField] private GameObject concaveSouthWest;
        [SerializeField] private GameObject concaveNorthWest;

        private GridCoordinate _coordinate;

        public GridCoordinate Coordinate => _coordinate;

        /// <summary>
        /// Validates that every logical slot maps to one unique prefab object.
        /// </summary>
        public bool TryValidateConfiguration(out string failureReason)
        {
            HashSet<GameObject> assignedObjects = new();
            if (!TryAddRequired(cellBase, nameof(cellBase), assignedObjects, out failureReason) ||
                !TryAddRequired(northWestHalfWall, nameof(northWestHalfWall), assignedObjects, out failureReason) ||
                !TryAddRequired(northEastHalfWall, nameof(northEastHalfWall), assignedObjects, out failureReason) ||
                !TryAddRequired(eastNorthHalfWall, nameof(eastNorthHalfWall), assignedObjects, out failureReason) ||
                !TryAddRequired(eastSouthHalfWall, nameof(eastSouthHalfWall), assignedObjects, out failureReason) ||
                !TryAddRequired(southEastHalfWall, nameof(southEastHalfWall), assignedObjects, out failureReason) ||
                !TryAddRequired(southWestHalfWall, nameof(southWestHalfWall), assignedObjects, out failureReason) ||
                !TryAddRequired(westSouthHalfWall, nameof(westSouthHalfWall), assignedObjects, out failureReason) ||
                !TryAddRequired(westNorthHalfWall, nameof(westNorthHalfWall), assignedObjects, out failureReason) ||
                !TryAddRequired(convexNorthEast, nameof(convexNorthEast), assignedObjects, out failureReason) ||
                !TryAddRequired(convexSouthEast, nameof(convexSouthEast), assignedObjects, out failureReason) ||
                !TryAddRequired(convexSouthWest, nameof(convexSouthWest), assignedObjects, out failureReason) ||
                !TryAddRequired(convexNorthWest, nameof(convexNorthWest), assignedObjects, out failureReason) ||
                !TryAddRequired(concaveNorthEast, nameof(concaveNorthEast), assignedObjects, out failureReason) ||
                !TryAddRequired(concaveSouthEast, nameof(concaveSouthEast), assignedObjects, out failureReason) ||
                !TryAddRequired(concaveSouthWest, nameof(concaveSouthWest), assignedObjects, out failureReason) ||
                !TryAddRequired(concaveNorthWest, nameof(concaveNorthWest), assignedObjects, out failureReason))
            {
                return false;
            }

            failureReason = string.Empty;
            return true;
        }

        /// <summary>
        /// Resets every slot and applies a complete derived state.
        /// </summary>
        public bool TryApply(
            ModularBoardCellVisualState state,
            out string failureReason)
        {
            if (!TryValidateConfiguration(out failureReason))
            {
                return false;
            }

            _coordinate = state.Coordinate;
            gameObject.name = $"BoardCell_{_coordinate.X}_{_coordinate.Y}";
            ResetVisuals();
            cellBase.SetActive(true);

            ApplyHalfWalls(state.HalfWalls);
            ApplyCorners(state.ConvexCorners, true);
            ApplyCorners(state.ConcaveCorners, false);

            failureReason = string.Empty;
            return true;
        }

        /// <summary>
        /// Disables the base and every optional piece so stale prefab state cannot survive reuse.
        /// </summary>
        public void ResetVisuals()
        {
            SetActive(cellBase, false);
            SetActive(northWestHalfWall, false);
            SetActive(northEastHalfWall, false);
            SetActive(eastNorthHalfWall, false);
            SetActive(eastSouthHalfWall, false);
            SetActive(southEastHalfWall, false);
            SetActive(southWestHalfWall, false);
            SetActive(westSouthHalfWall, false);
            SetActive(westNorthHalfWall, false);
            SetActive(convexNorthEast, false);
            SetActive(convexSouthEast, false);
            SetActive(convexSouthWest, false);
            SetActive(convexNorthWest, false);
            SetActive(concaveNorthEast, false);
            SetActive(concaveSouthEast, false);
            SetActive(concaveSouthWest, false);
            SetActive(concaveNorthWest, false);
        }

        private void ApplyHalfWalls(ModularHalfWallFlags flags)
        {
            SetActive(northWestHalfWall, flags.HasFlag(ModularHalfWallFlags.NorthWest));
            SetActive(northEastHalfWall, flags.HasFlag(ModularHalfWallFlags.NorthEast));
            SetActive(eastNorthHalfWall, flags.HasFlag(ModularHalfWallFlags.EastNorth));
            SetActive(eastSouthHalfWall, flags.HasFlag(ModularHalfWallFlags.EastSouth));
            SetActive(southEastHalfWall, flags.HasFlag(ModularHalfWallFlags.SouthEast));
            SetActive(southWestHalfWall, flags.HasFlag(ModularHalfWallFlags.SouthWest));
            SetActive(westSouthHalfWall, flags.HasFlag(ModularHalfWallFlags.WestSouth));
            SetActive(westNorthHalfWall, flags.HasFlag(ModularHalfWallFlags.WestNorth));
        }

        private void ApplyCorners(ModularCornerFlags flags, bool convex)
        {
            SetActive(convex ? convexNorthEast : concaveNorthEast,
                flags.HasFlag(ModularCornerFlags.NorthEast));
            SetActive(convex ? convexSouthEast : concaveSouthEast,
                flags.HasFlag(ModularCornerFlags.SouthEast));
            SetActive(convex ? convexSouthWest : concaveSouthWest,
                flags.HasFlag(ModularCornerFlags.SouthWest));
            SetActive(convex ? convexNorthWest : concaveNorthWest,
                flags.HasFlag(ModularCornerFlags.NorthWest));
        }

        private static bool TryAddRequired(
            GameObject target,
            string fieldName,
            HashSet<GameObject> assignedObjects,
            out string failureReason)
        {
            if (target == null)
            {
                failureReason = $"Modular board cell slot '{fieldName}' is not assigned.";
                return false;
            }

            if (!assignedObjects.Add(target))
            {
                failureReason =
                    $"Modular board cell slot '{fieldName}' reuses an object assigned to another slot.";
                return false;
            }

            failureReason = string.Empty;
            return true;
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null && target.activeSelf != active)
            {
                target.SetActive(active);
            }
        }
    }
}
