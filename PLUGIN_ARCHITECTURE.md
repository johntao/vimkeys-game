# Grid Plugin Architecture

## Overview

The game has been refactored to use a **plugin-based architecture** that cleanly separates core game mechanics (movement, timing, state management) from grid-specific game rules (collection logic, win conditions, visual states).

## Architecture

### Core Components

1. **IGridPlugin Interface** (`Domain/Plugins/IGridPlugin.cs`)
   - Defines the contract for all grid mechanics plugins
   - Provides hooks for game lifecycle events
   - Controls collection rules, win conditions, and visual rendering

2. **Built-in Plugins**
   - **PickUpPlugin** (`Domain/Plugins/PickUpPlugin.cs`) - "Pick Up" mode (GridV2)
   - **FillUpPlugin** (`Domain/Plugins/FillUpPlugin.cs`) - "Fill Up" mode (GridV3)

3. **Game Class** (`Domain/Entities/Game.cs`)
   - Clean core game engine
   - Delegates grid-specific logic to the active plugin
   - No more string-based version checks!

## Key Benefits

### Before Refactoring ❌

```csharp
// Game.cs was polluted with version-specific logic
if (GridVersion == "v3" && !ShowTrail)
{
    return; // Don't collect
}

if (GridVersion == "v3" && hasDroppable)
{
    // Skip adding this position
}
```

### After Refactoring ✅

```csharp
// Game.cs delegates to plugin
_plugin.OnPlayerMoved(oldPosition, Player.Position, this);
if (_plugin.ShouldCollectDroppable(Player.Position, this))
{
    // Collect droppable
}
if (_plugin.IsGameComplete(this))
{
    CompleteGame();
}
```

## Creating a New Plugin

To add a new game mechanic (e.g., "Race Mode", "Puzzle Mode"), follow these steps:

### 1. Create a new plugin class

```csharp
using p07_vimkeys_game.Domain.Entities;
using p07_vimkeys_game.Domain.ValueObjects;

namespace p07_vimkeys_game.Domain.Plugins;

public class RaceModePlugin : IGridPlugin
{
    public string Name => "race";
    public string Description => "Collect droppables as fast as possible";

    // Implement all IGridPlugin methods
    public void Configure(Dictionary<string, object> config) { }
    public void OnGameStart(Game game) { }
    public void OnGameReset(Game game) { }
    public void OnPlayerMoved(Position oldPosition, Position newPosition, Game game) { }
    public bool ShouldCollectDroppable(Position position, Game game) => true;
    public void OnDroppableCollected(Position position, Game game) { }
    public bool IsGameComplete(Game game) => game.RemainingDroppables == 0;
    public CellVisualState GetCellVisualState(Position position, Game game)
    {
        return CellVisualState.Empty;
    }
}
```

### 2. Create a corresponding Razor component (optional)

```razor
@* GridV4.razor - Race Mode visualization *@
@using p07_vimkeys_game.Domain.Entities
@using p07_vimkeys_game.Domain.ValueObjects

<div id="grid">
  @for (int y = 0; y < Game.GridSize; y++)
  {
    @for (int x = 0; x < Game.GridSize; x++)
    {
      var currentPos = new Position(x, y);
      var cellState = Game.Plugin.GetCellVisualState(currentPos, Game);
      var isPlayer = Game.Player.Position == currentPos;
      var hasDroppable = Game.Droppables.Any(d => !d.IsCollected && d.Position == currentPos);

      var classes = "cell";
      if (isPlayer) classes += " player";
      if (hasDroppable) classes += " droppable";

      <div class="@classes" data-row="@y" data-col="@x"></div>
    }
  }
</div>

@code {
    [Parameter] public Game Game { get; set; } = null!;
}
```

### 3. Register in Home.razor

```csharp
// Add to the grid version radio buttons
<label>
  <input type="radio" name="gridVersion" value="v4" checked="@(selectedGridVersion == "v4")" @onchange="OnGridVersionChange" />
  Race Mode
</label>

// Update OnGridVersionChange
private void OnGridVersionChange(ChangeEventArgs e)
{
    var version = e.Value?.ToString() ?? "v2";
    selectedGridVersion = version;
    gridComponentType = version switch
    {
        "v2" => typeof(GridV2),
        "v3" => typeof(GridV3),
        "v4" => typeof(GridV4), // New!
        _ => typeof(GridV2)
    };

    var plugin = version switch
    {
        "v2" => new PickUpPlugin(),
        "v3" => new FillUpPlugin(),
        "v4" => new RaceModePlugin(), // New!
        _ => new PickUpPlugin()
    };
    game.SetPlugin(plugin);
}
```

## Plugin Interface Reference

### Lifecycle Hooks

- **`Configure(Dictionary<string, object> config)`**
  - Called when game settings change
  - Use to sync ShowTrail, VisitThreshold, etc.

- **`OnGameStart(Game game)`**
  - Called when game transitions from Ready → Playing
  - Initialize any start-of-game state

- **`OnGameReset(Game game)`**
  - Called when game resets to Ready state
  - Clear plugin-specific state

### Movement & Collection

- **`OnPlayerMoved(Position oldPosition, Position newPosition, Game game)`**
  - Called after player moves to a new position
  - Update visited positions, trails, etc.

- **`ShouldCollectDroppable(Position position, Game game) : bool`**
  - Determines if a droppable at current position should be collected
  - Return `true` to collect, `false` to skip

- **`OnDroppableCollected(Position position, Game game)`**
  - Called after a droppable is successfully collected
  - Track collection for visual state, scoring, etc.

### Win Conditions

- **`IsGameComplete(Game game) : bool`**
  - Checks if game completion conditions are met
  - PickUpPlugin: All droppables collected
  - FillUpPlugin: All droppables collected + within threshold

### Rendering

- **`GetCellVisualState(Position position, Game game) : CellVisualState`**
  - Returns visual state for a grid cell
  - `IsVisited`: Show grey trail effect
  - `IsCollected`: Show black filled effect

## Examples: PickUp vs FillUp

### PickUpPlugin (Simple Collection)

```csharp
public bool ShouldCollectDroppable(Position position, Game game)
{
    return true; // Always collect on contact
}

public bool IsGameComplete(Game game)
{
    return game.RemainingDroppables == 0; // Simple win condition
}
```

### FillUpPlugin (Threshold-Based)

```csharp
public bool ShouldCollectDroppable(Position position, Game game)
{
    return _showTrail; // Only collect when "fill mode" is enabled
}

public bool IsGameComplete(Game game)
{
    if (game.RemainingDroppables > 0) return false;
    return _visitedPositions.Count <= _visitThreshold; // Must stay within threshold
}
```

## Backward Compatibility

The refactoring maintains backward compatibility:

- **Game.VisitedPositions** - Delegates to plugin's visited positions
- **Game.ShowTrail** - Property setters notify plugins
- **Game.VisitThreshold** - Property setters update plugin config
- **Existing tests** - All pass without modification (default to PickUpPlugin)

## Future Extensions

The plugin system enables easy addition of:

- **Time Attack Mode** - Collect under time pressure
- **Puzzle Mode** - Collect in specific order
- **Strategy Mode** - Plan optimal path before moving
- **Multiplayer Mode** - Competitive collection

Each mode is a standalone plugin with zero coupling to the core game engine!
