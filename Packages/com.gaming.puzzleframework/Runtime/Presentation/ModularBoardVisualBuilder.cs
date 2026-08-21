using System;
using System.Collections.Generic;
using PuzzleFramework.CoreBoard;
using UnityEngine;

namespace PuzzleFramework.Presentation
{
    /// <summary>
    /// Builds presentation-only modular board cells under a caller-owned dedicated root.
    /// </summary>
    public sealed class ModularBoardVisualBuilder
    {
        /// <summary>
        /// Clears the dedicated root and rebuilds it from a validated visual plan.
        /// Edit-mode callers must invoke this outside OnValidate and rendering callbacks.
        /// </summary>
        public bool TryRebuild(
            ModularBoardVisualPlan plan,
            ModularBoardCellView cellPrefab,
            Transform generatedRoot,
            GridWorldLayout worldLayout,
            float normalOffset,
            out IReadOnlyList<ModularBoardCellView> generatedCells,
            out string failureReason)
        {
            generatedCells = Array.Empty<ModularBoardCellView>();

            if (plan == null)
            {
                failureReason = "A modular board visual plan is required.";
                return false;
            }

            if (!plan.Success)
            {
                failureReason = plan.FailureReason;
                return false;
            }

            if (cellPrefab == null)
            {
                failureReason = "A modular board cell prefab is required.";
                return false;
            }

            if (generatedRoot == null)
            {
                failureReason = "A dedicated modular board visual root is required.";
                return false;
            }

            if (!cellPrefab.TryValidateConfiguration(out failureReason))
            {
                return false;
            }

            ClearGeneratedChildren(generatedRoot);

            Quaternion rotation = BuildBoardRotation(worldLayout);
            Vector3 normal = Vector3.Cross(worldLayout.BoardYAxis, worldLayout.BoardXAxis).normalized;
            Vector3 scale = new(worldLayout.CellSize.x, 1f, worldLayout.CellSize.y);
            List<ModularBoardCellView> cells = new(plan.CellStates.Count);

            for (int i = 0; i < plan.CellStates.Count; i++)
            {
                ModularBoardCellVisualState state = plan.CellStates[i];
                ModularBoardCellView view = UnityEngine.Object.Instantiate(cellPrefab, generatedRoot);
                view.gameObject.SetActive(false);

                Vector3 position = worldLayout.GridToWorldPosition(state.Coordinate) +
                                   (normal * normalOffset);
                view.transform.SetPositionAndRotation(position, rotation);
                view.transform.localScale = scale;

                if (!view.TryApply(state, out failureReason))
                {
                    ClearGeneratedChildren(generatedRoot);
                    return false;
                }

                view.gameObject.SetActive(true);
                cells.Add(view);
            }

            generatedCells = cells;
            failureReason = string.Empty;
            return true;
        }

        /// <summary>
        /// Removes all children from a root reserved exclusively for generated board visuals.
        /// </summary>
        public void ClearGeneratedChildren(Transform generatedRoot)
        {
            if (generatedRoot == null)
            {
                return;
            }

            for (int i = generatedRoot.childCount - 1; i >= 0; i--)
            {
                GameObject child = generatedRoot.GetChild(i).gameObject;
                child.SetActive(false);
                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(child);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(child);
                }
            }
        }

        private static Quaternion BuildBoardRotation(GridWorldLayout worldLayout)
        {
            Vector3 normal = Vector3.Cross(worldLayout.BoardYAxis, worldLayout.BoardXAxis).normalized;
            return Quaternion.LookRotation(worldLayout.BoardYAxis, normal);
        }
    }
}
