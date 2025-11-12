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
        var droppablePositions = new[] { new Position(1, 0), new Position(2, 0) };
        game.Initialize(droppablePositions);
        game.Start();

        Assert(game.RemainingDroppables == 2, "Should have 2 droppables");

        game.MovePlayer(Direction.Right); // Move to (1, 0)
        Assert(game.RemainingDroppables == 1, "Should have 1 droppable left");
        Assert(game.State == GameState.Playing, "Game should still be playing");

        game.MovePlayer(Direction.Right); // Move to (2, 0)
        Assert(game.RemainingDroppables == 0, "Should have no droppables left");
        Assert(game.State == GameState.Completed, "Game should be completed");
    }

    private static void TestGameCompletion()
    {
        var game = new Game();
        game.Initialize(new[] { new Position(1, 0) });
        game.Start();

        Assert(game.State == GameState.Playing, "Game should be playing");
        Assert(game.StartTime.HasValue, "Start time should be set");
        Assert(!game.EndTime.HasValue, "End time should not be set");

        game.MovePlayer(Direction.Right); // Collect the only droppable

        Assert(game.State == GameState.Completed, "Game should be completed");
        Assert(game.EndTime.HasValue, "End time should be set");
        Assert(game.Score.HasValue, "Score should be calculated");
        Assert(game.Score!.Value >= 0, "Score should be non-negative");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new Exception($"Test failed: {message}");
        }
    }
}
