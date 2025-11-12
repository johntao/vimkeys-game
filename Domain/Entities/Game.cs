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
        Droppables = new List<Droppable>();
        State = GameState.NotStarted;
    }

    /// <summary>
    /// Initializes the game with droppables at specified positions
    /// </summary>
    public void Initialize(IEnumerable<Position> droppablePositions)
    {
        if (State != GameState.NotStarted)
        {
            throw new InvalidOperationException("Game has already been initialized");
        }

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
            throw new InvalidOperationException("Game must be in Ready state to start");
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
        if (State != GameState.Playing)
        {
            return false;
        }

        // Attempt to move the player
        bool moved = Player.TryMove(direction);

        if (!moved)
        {
            return false; // Player hit the boundary
        }

        // Check if player collected a droppable
        CheckDroppableCollection();

        // Check if all droppables are collected
        if (RemainingDroppables == 0)
        {
            CompleteGame();
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
    /// Resets the game to initial state
    /// </summary>
    public void Reset()
    {
        Player = new Player(new Position(0, 0));
        Droppables.Clear();
        StartTime = null;
        EndTime = null;
        State = GameState.NotStarted;
    }
}

/// <summary>
/// Represents the current state of the game
/// </summary>
public enum GameState
{
    NotStarted,
    Ready,
    Playing,
    Completed
}
