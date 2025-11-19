using p07_vimkeys_game.Domain.Entities;
using p07_vimkeys_game.Domain.ValueObjects;

namespace p07_vimkeys_game.Domain.Plugins;

/// <summary>
/// "Fill Up" game mechanic (GridV3).
/// Players fill cells within a threshold while collecting droppables.
/// Features:
/// - Only collects droppables when "fill mode" is enabled (ShowTrail)
/// - Supports "unfill" mechanic by revisiting cells when fill mode is disabled
/// - Win condition: collect all droppables AND stay within visit threshold
/// - Collected cells turn black for visual feedback
/// </summary>
public class FillUpPlugin : IGridPlugin
{
    private HashSet<Position> _visitedPositions = new();
    private HashSet<Position> _collectedPositions = new();
    private bool _showTrail = false;
    private int _visitThreshold = 5;

    public string Name => "fillup";
    public string Description => "Fill cells to collect droppables within a visit threshold";

    public void Configure(Dictionary<string, object> config)
    {
        if (config.TryGetValue("ShowTrail", out var showTrail) && showTrail is bool trailValue)
        {
            _showTrail = trailValue;
        }

        if (config.TryGetValue("VisitThreshold", out var threshold) && threshold is int thresholdValue)
        {
            _visitThreshold = thresholdValue;
        }
    }

    public void OnGameStart(Game game)
    {
        // Nothing special to do on start
    }

    public void OnGameReset(Game game)
    {
        _visitedPositions.Clear();
        _collectedPositions.Clear();
    }

    public void OnPlayerMoved(Position oldPosition, Position newPosition, Game game)
    {
        var hasDroppable = game.Droppables.Any(d => !d.IsCollected && d.Position == newPosition);

        if (_showTrail)
        {
            // Fill mode: Add to visited positions (but not droppable cells)
            if (!hasDroppable)
            {
                _visitedPositions.Add(newPosition);
            }
        }
        else
        {
            // Unfill mode: Remove from visited positions if present
            if (_visitedPositions.Contains(newPosition))
            {
                _visitedPositions.Remove(newPosition);
            }
        }
    }

    public bool ShouldCollectDroppable(Position position, Game game)
    {
        // FillUp mode: only collect when "fill mode" is enabled
        return _showTrail;
    }

    public void OnDroppableCollected(Position position, Game game)
    {
        // Track collected positions for black cell visualization
        _collectedPositions.Add(position);
    }

    public bool IsGameComplete(Game game)
    {
        // Win condition: all droppables collected AND within threshold
        if (game.RemainingDroppables > 0)
        {
            return false;
        }

        // Check if player stayed within visit threshold
        return _visitedPositions.Count <= _visitThreshold;
    }

    public CellVisualState GetCellVisualState(Position position, Game game)
    {
        // Show visited trail (grey) and collected cells (black)
        var isCollected = _collectedPositions.Contains(position);
        var isVisited = _visitedPositions.Contains(position) && !isCollected;

        return new CellVisualState
        {
            IsVisited = isVisited,
            IsCollected = isCollected
        };
    }

    /// <summary>
    /// Update trail configuration (called when user toggles trail in UI)
    /// </summary>
    public void SetShowTrail(bool showTrail, Position currentPlayerPosition, Game game)
    {
        var wasEnabled = _showTrail;
        _showTrail = showTrail;

        // If enabling fill mode for first time, add current position (if not a droppable)
        if (!wasEnabled && showTrail)
        {
            var hasDroppable = game.Droppables.Any(d => !d.IsCollected && d.Position == currentPlayerPosition);
            if (!hasDroppable)
            {
                _visitedPositions.Add(currentPlayerPosition);
            }
        }
    }

    /// <summary>
    /// Update visit threshold configuration
    /// </summary>
    public void SetVisitThreshold(int threshold)
    {
        _visitThreshold = threshold;
    }

    /// <summary>
    /// Get the visited positions count (for UI display)
    /// </summary>
    public int VisitedCount => _visitedPositions.Count;

    /// <summary>
    /// Get the visit threshold (for UI display)
    /// </summary>
    public int VisitThreshold => _visitThreshold;

    /// <summary>
    /// Get the visited positions (for external access if needed)
    /// </summary>
    public IReadOnlySet<Position> VisitedPositions => _visitedPositions;

    /// <summary>
    /// Get the collected positions (for external access if needed)
    /// </summary>
    public IReadOnlySet<Position> CollectedPositions => _collectedPositions;
}
