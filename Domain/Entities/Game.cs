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

    /// <summary>
    /// Gets the elapsed time in seconds (score)
    /// Returns null if game hasn't started or ended
    /// </summary>
    public double? Score => StartTime.HasValue && EndTime.HasValue
        ? (EndTime.Value - StartTime.Value).TotalSeconds
        : null;

    /// <summary>
    /// Gets the count of remaining droppables
    /// </summary>
    public int RemainingDroppables => Droppables.Count(d => !d.IsCollected);

    public Game()
    {
        Player = new Player(new Position(0, 0)); // Start at top-left

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
    /// Checks if the player's current position matches any uncollected droppable
    /// </summary>
    private void CheckDroppableCollection()
    {
        var droppable = Droppables.FirstOrDefault(d =>
            !d.IsCollected && d.Position == Player.Position);

        droppable?.Collect();
    }

    /// <summary>
    /// Completes the game and stops the timer
    /// </summary>
    private void CompleteGame()
    {
        EndTime = DateTime.UtcNow;
        State = GameState.Completed;
    }

    /// <summary>
    /// Resets the game to Ready state with fresh droppables
    /// </summary>
    public void Reset()
    {
        Player = new Player(new Position(0, 0));

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
    Playing,
    Completed
}
