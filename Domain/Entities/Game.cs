namespace p07_vimkeys_game.Domain.Entities;

using p07_vimkeys_game.Domain.ValueObjects;
using p07_vimkeys_game.Domain.Plugins;

/// <summary>
/// Game aggregate root that manages the entire game state
/// Handles player movement, droppable collection, and scoring
/// </summary>
public class Game
{
    public const int GridSize = 10;
    private static readonly Random _random = new Random();

    public Player Player { get; private set; }
    public List<Droppable> Droppables { get; private set; }
    public GameState State { get; private set; }
    public DateTime? StartTime { get; private set; }
    public DateTime? EndTime { get; private set; }
    public (double, double) Scores { get; private set; }
    public bool UseRandomDroppables { get; set; } = false;
    public int DroppableCount { get; set; } = 9;

    // Plugin system
    private IGridPlugin _plugin;

    /// <summary>
    /// Gets the current grid plugin
    /// </summary>
    public IGridPlugin Plugin => _plugin;


    /// <summary>
    /// Gets the count of remaining droppables
    /// </summary>
    public int RemainingDroppables => Droppables.Count(d => !d.IsCollected);

    /// <summary>
    /// Creates a new game with the specified plugin
    /// </summary>
    /// <param name="plugin">Grid plugin to use (defaults to PickUpPlugin)</param>
    public Game(IGridPlugin? plugin = null)
    {
        _plugin = plugin ?? new PickUpPlugin();
        Player = new Player(new Position(0, 0)); // Start at top-left
        Scores = (999, 999);

        // Initialize with fixed positions by default (UseRandomDroppables defaults to false)
        Droppables = FixedPositions.Select(pos => new Droppable(pos)).ToList();
        State = GameState.Ready;
    }

    /// <summary>
    /// Sets the grid plugin (allows switching between PickUp and FillUp modes)
    /// </summary>
    public void SetPlugin(IGridPlugin plugin)
    {
        _plugin = plugin;
        _plugin.OnGameReset(this);
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

        // Delegate to plugin for game start logic
        _plugin.OnGameStart(this);
    }

    /// <summary>
    /// Moves the player in the specified direction
    /// Automatically checks for droppable collection and game completion
    /// </summary>
    public bool MovePlayer(Direction direction)
    {
        var oldPosition = Player.Position;

        // Attempt to move the player
        bool moved = Player.TryMove(direction);

        if (!moved)
        {
            return false; // Player hit the boundary
        }

        if (State == GameState.Playing)
        {
            // Delegate to plugin for movement handling
            _plugin.OnPlayerMoved(oldPosition, Player.Position, this);

            // Check if player collected a droppable
            CheckDroppableCollection();

            // Check if game completion conditions are met
            if (_plugin.IsGameComplete(this))
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
    /// Delegates to plugin to determine if collection should occur
    /// </summary>
    private void CheckDroppableCollection()
    {
        // Check if plugin allows collection at current position
        if (!_plugin.ShouldCollectDroppable(Player.Position, this))
        {
            return;
        }

        var droppable = Droppables.FirstOrDefault(d =>
            !d.IsCollected && d.Position == Player.Position);

        if (droppable != null)
        {
            droppable.Collect();
            _plugin.OnDroppableCollected(Player.Position, this);
        }
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
        var droppablePositions = UseRandomDroppables
            ? GenerateRandomPositions()
            : FixedPositions;

        Droppables = droppablePositions.Select(pos => new Droppable(pos)).ToList();
        StartTime = null;
        EndTime = null;
        State = GameState.Ready;

        // Delegate to plugin for reset logic
        _plugin.OnGameReset(this);
    }

    /// <summary>
    /// Gets the fixed droppable positions
    /// Returns up to DroppableCount positions from a predefined list
    /// </summary>
    private List<Position> FixedPositions = [
        new(2, 1), new(1, 2), new(3, 2),
        new(2, 3), new(4, 4), new(7, 6),
        new(5, 8), new(8, 3), new(9, 9),
    ];

    /// <summary>
    /// Generates random unique positions for droppables
    /// Ensures no position overlaps with the player's starting position
    /// </summary>
    private List<Position> GenerateRandomPositions()
    {
        var positions = new HashSet<Position>();

        while (positions.Count < DroppableCount)
        {
            var x = _random.Next(0, GridSize);
            var y = _random.Next(0, GridSize);
            var pos = new Position(x, y);

            // Don't place droppable at player's starting position
            if (pos != Player.Position)
            {
                positions.Add(pos);
            }
        }

        return positions.ToList();
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
