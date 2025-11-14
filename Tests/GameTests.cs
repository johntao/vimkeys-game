namespace p07_vimkeys_game.Tests;

using p07_vimkeys_game.Domain.Entities;
using p07_vimkeys_game.Domain.ValueObjects;

/// <summary>
/// Basic tests for the core game logic
/// Note: These are simple tests without a testing framework for demonstration
/// You can add xUnit/NUnit later for proper unit testing
/// </summary>
public static class GameTests
{
    public static void RunAllTests()
    {
        TestPlayerMovement();
        TestPlayerBoundaries();
        TestDroppableCollection();
        TestGameCompletion();
        Console.WriteLine("All tests passed!");
    }

    private static void TestPlayerMovement()
    {
        var player = new Player(new Position(5, 5));

        player.TryMove(Direction.Up);
        Assert(player.Position == new Position(5, 4), "Player should move up");

        player.TryMove(Direction.Right);
        Assert(player.Position == new Position(6, 4), "Player should move right");

        player.TryMove(Direction.Down);
        Assert(player.Position == new Position(6, 5), "Player should move down");

        player.TryMove(Direction.Left);
        Assert(player.Position == new Position(5, 5), "Player should move left");
    }

    private static void TestPlayerBoundaries()
    {
        var player = new Player(new Position(0, 0));

        bool canMoveUp = player.TryMove(Direction.Up);
        Assert(!canMoveUp, "Player should not move outside top boundary");
        Assert(player.Position == new Position(0, 0), "Player position should not change");

        bool canMoveLeft = player.TryMove(Direction.Left);
        Assert(!canMoveLeft, "Player should not move outside left boundary");

        var player2 = new Player(new Position(9, 9));
        bool canMoveRight = player2.TryMove(Direction.Right);
        Assert(!canMoveRight, "Player should not move outside right boundary");

        bool canMoveDown = player2.TryMove(Direction.Down);
        Assert(!canMoveDown, "Player should not move outside bottom boundary");
    }

    private static void TestDroppableCollection()
    {
        var game = new Game();
        game.Start();

        // Game auto-initializes with droppables, we'll test with the default setup
        int initialDroppables = game.RemainingDroppables;
        Assert(initialDroppables > 0, "Should have droppables");

        // Move player and collect droppables until game completes
        var previousCount = game.RemainingDroppables;

        // Navigate to collect a droppable at position (2, 1)
        game.MovePlayer(Direction.Right); // Move to (1, 0)
        game.MovePlayer(Direction.Right); // Move to (2, 0)
        game.MovePlayer(Direction.Down);  // Move to (2, 1) - droppable location

        Assert(game.RemainingDroppables < previousCount, "Should have collected a droppable");
        Assert(game.State == GameState.Playing, "Game should still be playing");
    }

    private static void TestGameCompletion()
    {
        var game = new Game();
        game.Start();

        Assert(game.State == GameState.Playing, "Game should be playing");
        Assert(game.StartTime.HasValue, "Start time should be set");

        // Collect all droppables to trigger auto-reset
        int safetyCounter = 0;
        int maxMoves = 200; // Safety limit to prevent infinite loop

        while (game.RemainingDroppables > 0 && safetyCounter < maxMoves)
        {
            // Try all directions to navigate towards droppables
            if (!game.MovePlayer(Direction.Right))
                if (!game.MovePlayer(Direction.Down))
                    if (!game.MovePlayer(Direction.Left))
                        game.MovePlayer(Direction.Up);
            safetyCounter++;
        }

        // After collecting all droppables, game auto-resets to Ready state
        Assert(game.State == GameState.Ready, "Game should auto-reset to Ready after completion");
        Assert(game.Scores.Item1 >= 0, "Current score should be non-negative");
        Assert(game.Scores.Item2 >= 0, "Best score should be non-negative");
        Assert(game.Scores.Item2 <= game.Scores.Item1, "Best score should be less than or equal to current score");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new Exception($"Test failed: {message}");
        }
    }
}
