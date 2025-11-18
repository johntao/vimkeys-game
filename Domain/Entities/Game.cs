namespace p07_vimkeys_game.Domain.Entities;

using p07_vimkeys_game.Domain.ValueObjects;

/// <summary>
/// Game aggregate root that manages the entire game state
/// Handles player movement, droppable collection, and scoring
/// </summary>
public class Game
{
    public const int GridSize = 10;

    public Player Player { get; private set; }
    public List<Droppable> Droppables { get; private set; }
    public GameState State { get; private set; }
    public DateTime? StartTime { get; private set; }
    public DateTime? EndTime { get; private set; }
    public (double, double) Scores { get; private set; }


    /// <summary>
    /// Gets the count of remaining droppables
    /// </summary>
    public int RemainingDroppables => Droppables.Count(d => !d.IsCollected);

    public Game()
    {
        Player = new Player(new Position(0, 0)); // Start at top-left
        Scores = (999, 999);

        // Auto-initialize with default droppable positions
        var droppablePositions = new List<Position>
        {
            new Position(2, 1),
            new Position(1, 2),
            new Position(3, 2),
            new Position(2, 3),
            new Position(4, 4),
            new Position(7, 6),
            new Position(5, 8),
            new Position(8, 3),
            new Position(9, 9)
        };

        Droppables = droppablePositions.Select(pos => new Droppable(pos)).ToList();
        State = GameState.Ready;
    }

    /// <summary>
    /// Starts the game and begins the timer
    /// </summary>
    public void Start()
    {
        if (State != GameState.Ready)
        {
            return; // Silently ignore if not in Ready state
        }

        StartTime = DateTime.UtcNow;
        State = GameState.Playing;
    }

    /// <summary>
    /// Moves the player in the specified direction
    /// Automatically checks for droppable collection and game completion
    /// </summary>
    public bool MovePlayer(Direction direction)
    {
        // Attempt to move the player
        bool moved = Player.TryMove(direction);

        if (!moved)
        {
            return false; // Player hit the boundary
        }

        if (State == GameState.Playing)
        {
            // Check if player collected a droppable
            CheckDroppableCollection();

            // Check if all droppables are collected
            if (RemainingDroppables == 0)
            {
                CompleteGame();
            }
        }

        return true;
    }

    /// <summary>
    /// Moves the player multiple times in the specified direction
    /// Stops if the player hits a boundary
    /// </summary>
    public void MovePlayerMultiple(Direction direction, int count)
    {
        for (int i = 0; i < count; i++)
        {
            bool moved = MovePlayer(direction);
            if (!moved)
            {
                break; // Stop if we hit a boundary
            }
        }
    }

    /// <summary>
    /// Checks if the player's current position matches any uncollected droppable
    /// </summary>
    private void CheckDroppableCollection()
    {
        var droppable = Droppables.FirstOrDefault(d =>
            !d.IsCollected && d.Position == Player.Position);

        droppable?.Collect();
    }

    /// <summary>
    /// Completes the game, stops the timer, and auto-resets for next round
    /// </summary>
    private void CompleteGame()
    {
        if (!StartTime.HasValue || !EndTime.HasValue)
        {
            EndTime = DateTime.UtcNow;
        }

        var score = (EndTime!.Value - StartTime!.Value).TotalSeconds;

        // Update Scores tuple - assign new tuple with updated values
        var newBest = score < Scores.Item2 ? score : Scores.Item2;
        Scores = (score, newBest);

        // Auto-reset for next round
        Reset();
    }

    /// <summary>
    /// Resets the game to Ready state with fresh droppables
    /// Preserves player position
    /// </summary>
    public void Reset()
    {
        // Reset droppables to initial positions
        var droppablePositions = new List<Position>
        {
            new Position(2, 1),
            new Position(1, 2),
            new Position(3, 2),
            new Position(2, 3),
            new Position(4, 4),
            new Position(7, 6),
            new Position(5, 8),
            new Position(8, 3),
            new Position(9, 9)
        };

        Droppables = droppablePositions.Select(pos => new Droppable(pos)).ToList();
        StartTime = null;
        EndTime = null;
        State = GameState.Ready;
    }
}

/// <summary>
/// Represents the current state of the game
/// </summary>
public enum GameState
{
    Ready,
    Playing
}
