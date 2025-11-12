namespace p07_vimkeys_game.Domain.ValueObjects;

/// <summary>
/// Value object representing a position on the game grid
/// </summary>
public record Position(int X, int Y)
{
    public const int MinCoordinate = 0;
    public const int MaxCoordinate = 9;

    /// <summary>
    /// Validates that the position is within the 10x10 grid bounds
    /// </summary>
    public bool IsValid()
    {
        return X >= MinCoordinate && X <= MaxCoordinate &&
               Y >= MinCoordinate && Y <= MaxCoordinate;
    }

    /// <summary>
    /// Creates a new position by moving in the specified direction
    /// </summary>
    public Position Move(Direction direction)
    {
        return direction switch
        {
            Direction.Up => this with { Y = Y - 1 },
            Direction.Down => this with { Y = Y + 1 },
            Direction.Left => this with { X = X - 1 },
            Direction.Right => this with { X = X + 1 },
            _ => this
        };
    }
}
