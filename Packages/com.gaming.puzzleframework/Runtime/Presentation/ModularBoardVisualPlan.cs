using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using PuzzleFramework.CoreBoard;

namespace PuzzleFramework.Presentation
{
    /// <summary>
    /// Result of converting boundary topology into modular cell visual state.
    /// </summary>
    public sealed class ModularBoardVisualPlan
    {
        private readonly Dictionary<GridCoordinate, ModularBoardCellVisualState> _statesByCoordinate;

        private ModularBoardVisualPlan(
            bool success,
            string failureReason,
            IList<ModularBoardCellVisualState> cellStates)
        {
            Success = success;
            FailureReason = failureReason ?? string.Empty;
            CellStates = new ReadOnlyCollection<ModularBoardCellVisualState>(
                new List<ModularBoardCellVisualState>(
                    cellStates ?? throw new ArgumentNullException(nameof(cellStates))));
            _statesByCoordinate = new Dictionary<GridCoordinate, ModularBoardCellVisualState>();
            for (int i = 0; i < CellStates.Count; i++)
            {
                _statesByCoordinate.Add(CellStates[i].Coordinate, CellStates[i]);
            }
        }

        public bool Success { get; }
        public string FailureReason { get; }
        public IReadOnlyList<ModularBoardCellVisualState> CellStates { get; }

        public bool TryGetCellState(
            GridCoordinate coordinate,
            out ModularBoardCellVisualState state)
        {
            return _statesByCoordinate.TryGetValue(coordinate, out state);
        }

        internal static ModularBoardVisualPlan Successful(
            IList<ModularBoardCellVisualState> cellStates)
        {
            return new ModularBoardVisualPlan(true, string.Empty, cellStates);
        }

        internal static ModularBoardVisualPlan Failed(string failureReason)
        {
            return new ModularBoardVisualPlan(
                false,
                failureReason,
                Array.Empty<ModularBoardCellVisualState>());
        }
    }
}
