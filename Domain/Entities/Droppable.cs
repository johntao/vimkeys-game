namespace p07_vimkeys_game.Domain.Entities;

using p07_vimkeys_game.Domain.ValueObjects;

/// <summary>
/// Entity representing a droppable item on the grid
/// Droppables have fixed positions and can be collected by the player
/// </summary>
public class Droppable
{
    public Position Position { get; }
    public bool IsCollected { get; private set; }

    public Droppable(Position position)
    {
        Position = position;
        IsCollected = false;
    }

    /// <summary>
    /// Marks this droppable as collected
    /// </summary>
    public void Collect()
    {
        IsCollected = true;
    }
}
