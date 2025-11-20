using p07_vimkeys_game.Domain.Entities;
using p07_vimkeys_game.Domain.ValueObjects;

namespace p07_vimkeys_game.Domain.Plugins;

/// <summary>
/// Defines the contract for grid game mechanics plugins.
/// Plugins control collection rules, win conditions, and visual cell states.
/// </summary>
public interface IGridPlugin
{
    /// <summary>
    /// Unique identifier for the plugin (e.g., "pickup", "fillup")
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Human-readable description of the game mechanic
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Called when the game starts
    /// </summary>
    void OnGameStart(Game game);

    /// <summary>
    /// Called when the game is reset
    /// </summary>
    void OnGameReset(Game game);

    /// <summary>
    /// Called after the player moves to a new position
    /// </summary>
    /// <param name="oldPosition">Previous player position</param>
    /// <param name="newPosition">New player position</param>
    /// <param name="game">Game instance for context</param>
    void OnPlayerMoved(Position oldPosition, Position newPosition, Game game);

    /// <summary>
    /// Determines if a droppable at the current position should be collected
    /// </summary>
    /// <param name="position">Position to check</param>
    /// <param name="game">Game instance for context</param>
    /// <returns>True if the droppable should be collected</returns>
    bool ShouldCollectDroppable(Position position, Game game);

    /// <summary>
    /// Called after a droppable is collected
    /// </summary>
    /// <param name="position">Position where droppable was collected</param>
    /// <param name="game">Game instance for context</param>
    void OnDroppableCollected(Position position, Game game);

    /// <summary>
    /// Checks if the game completion conditions are met
    /// </summary>
    /// <param name="game">Game instance to evaluate</param>
    /// <returns>True if the game should complete</returns>
    bool IsGameComplete(Game game);

    /// <summary>
    /// Gets the visual state of a cell for rendering
    /// </summary>
    /// <param name="position">Cell position</param>
    /// <param name="game">Game instance for context</param>
    /// <returns>Visual state information for the UI</returns>
    CellVisualState GetCellVisualState(Position position, Game game);
}

/// <summary>
/// Represents the visual rendering state of a grid cell
/// </summary>
public record CellVisualState
{
    /// <summary>
    /// Whether this cell should show the "visited" trail effect
    /// </summary>
    public bool IsVisited { get; init; }

    /// <summary>
    /// Whether this cell should show the "collected/filled" effect
    /// </summary>
    public bool IsCollected { get; init; }

    /// <summary>
    /// Default empty state
    /// </summary>
    public static CellVisualState Empty => new() { IsVisited = false, IsCollected = false };
}
