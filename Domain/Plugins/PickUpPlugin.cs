using p07_vimkeys_game.Domain.Entities;
using p07_vimkeys_game.Domain.ValueObjects;

namespace p07_vimkeys_game.Domain.Plugins;

/// <summary>
/// "Pick Up" game mechanic (GridV2).
/// Players collect droppables by moving over them.
/// Simple win condition: collect all droppables.
/// </summary>
public class PickUpPlugin : IGridPlugin
{
    private HashSet<Position> _visitedPositions = new();
    private bool _showTrail = false;

    public string Name => "pickup";
    public string Description => "Collect all droppables by moving over them";

    public void Configure(Dictionary<string, object> config)
    {
        if (config.TryGetValue("ShowTrail", out var showTrail) && showTrail is bool trailValue)
        {
            _showTrail = trailValue;
        }
    }

    public void OnGameStart(Game game)
    {
        // Nothing special to do on start
    }

    public void OnGameReset(Game game)
    {
        _visitedPositions.Clear();
    }

    public void OnPlayerMoved(Position oldPosition, Position newPosition, Game game)
    {
        // Track visited positions for trail visualization
        if (_showTrail)
        {
            _visitedPositions.Add(newPosition);
        }
    }

    public bool ShouldCollectDroppable(Position position, Game game)
    {
        // PickUp mode: always collect droppables on contact
        return true;
    }

    public void OnDroppableCollected(Position position, Game game)
    {
        // No special logic needed for pickup mode
    }

    public bool IsGameComplete(Game game)
    {
        // Simple win condition: all droppables collected
        return game.RemainingDroppables == 0;
    }

    public CellVisualState GetCellVisualState(Position position, Game game)
    {
        // Only show visited trail, no "collected" state for PickUp mode
        return new CellVisualState
        {
            IsVisited = _visitedPositions.Contains(position),
            IsCollected = false
        };
    }

    /// <summary>
    /// Update trail configuration (called when user toggles trail in UI)
    /// </summary>
    public void SetShowTrail(bool showTrail, Position currentPlayerPosition)
    {
        var wasEnabled = _showTrail;
        _showTrail = showTrail;

        // If enabling trail for first time, add current position
        if (!wasEnabled && showTrail)
        {
            _visitedPositions.Add(currentPlayerPosition);
        }
    }

    /// <summary>
    /// Get the visited positions (for external access if needed)
    /// </summary>
    public IReadOnlySet<Position> VisitedPositions => _visitedPositions;
}
