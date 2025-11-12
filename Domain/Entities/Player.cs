namespace p07_vimkeys_game.Domain.Entities;

using p07_vimkeys_game.Domain.ValueObjects;

/// <summary>
/// Entity representing the player in the game
/// Player moves around the grid without increasing in size
/// </summary>
public class Player
{
    public Position Position { get; private set; }

    public Player(Position startPosition)
    {
        Position = startPosition;
    }

    /// <summary>
    /// Attempts to move the player in the specified direction
    /// Returns true if the move was successful (within bounds)
    /// </summary>
    public bool TryMove(Direction direction)
    {
        var newPosition = Position.Move(direction);

        if (!newPosition.IsValid())
        {
            return false; // Player cannot move outside the grid
        }

        Position = newPosition;
        return true;
    }
}
