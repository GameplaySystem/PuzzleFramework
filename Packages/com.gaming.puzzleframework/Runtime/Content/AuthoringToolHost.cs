using System;
using System.Collections.Generic;
using PuzzleFramework.CoreBoard;

namespace PuzzleFramework.Content
{
    public enum AuthoringTargetKind { Cell, BoundaryEdge }

    /// <summary>A structural pick delivered to a game-owned authoring tool.</summary>
    public readonly struct AuthoringTarget
    {
        private AuthoringTarget(AuthoringTargetKind kind, GridCoordinate cell, BoardBoundaryEdge edge)
        {
            Kind = kind;
            Cell = cell;
            Edge = edge;
        }

        public AuthoringTargetKind Kind { get; }
        public GridCoordinate Cell { get; }
        public BoardBoundaryEdge Edge { get; }
        public static AuthoringTarget ForCell(GridCoordinate cell) => new(AuthoringTargetKind.Cell, cell, default);
        public static AuthoringTarget ForBoundaryEdge(BoardBoundaryEdge edge) =>
            new(AuthoringTargetKind.BoundaryEdge, edge.CellCoordinate, edge);
    }

    /// <summary>Game-owned behavior registered explicitly with the shared authoring host.</summary>
    public interface IAuthoringTool
    {
        string Id { get; }
        AuthoringEditResult Preview(LevelAuthoringCore session, AuthoringTarget target);
        AuthoringEditResult Apply(LevelAuthoringCore session, AuthoringTarget target);
    }

    /// <summary>Routes a target to one explicitly registered tool; owns no game payload or UI.</summary>
    public sealed class AuthoringToolHost
    {
        private readonly Dictionary<string, IAuthoringTool> _tools = new(StringComparer.Ordinal);

        public AuthoringToolHost(LevelAuthoringCore session)
        {
            Session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public LevelAuthoringCore Session { get; }

        public void Register(IAuthoringTool tool)
        {
            if (tool == null || string.IsNullOrWhiteSpace(tool.Id) || !_tools.TryAdd(tool.Id, tool))
                throw new ArgumentException("Tool needs a unique non-empty ID.", nameof(tool));
        }

        public bool SelectTool(string id)
        {
            if (id == null || !_tools.ContainsKey(id)) return false;
            Session.CurrentToolId = id;
            return true;
        }

        public AuthoringEditResult Preview(AuthoringTarget target) => ResolveTool()?.Preview(Session, target) ??
            new AuthoringEditResult(false, "No registered authoring tool is selected.", null);

        public AuthoringEditResult Apply(AuthoringTarget target) => ResolveTool()?.Apply(Session, target) ??
            new AuthoringEditResult(false, "No registered authoring tool is selected.", null);

        private IAuthoringTool ResolveTool() =>
            Session.CurrentToolId != null && _tools.TryGetValue(Session.CurrentToolId, out IAuthoringTool tool)
                ? tool : null;
    }
}
