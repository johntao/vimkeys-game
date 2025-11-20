# Decoupled Plugin Architecture - Refactoring Summary

## Objective
Decouple `ShowTrail` and `VisitedPositions` from the core Game logic, as they are plugin-specific visualization concerns, not core game mechanics.

## Core Principle
**The core game is about exploring different ways to move a player.**
- Movement mechanics
- Collection logic (delegated to plugins)
- Win conditions (delegated to plugins)
- State management (Ready/Playing)
- Timing and scoring

**NOT about:**
- Trail visualization (ShowTrail)
- Visited positions tracking (VisitedPositions)
- UI components (GridV2, GridV3)

---

## Changes Made

### 1. Game.cs - Cleaned Core Game Logic

**Removed:**
- ❌ `ShowTrail` property
- ❌ `VisitedPositions` property
- ❌ `VisitThreshold` property
- ❌ `UpdatePluginConfiguration()` method
- ❌ All plugin type casting (`if (_plugin is PickUpPlugin)`)

**Result:**
```csharp
public class Game
{
    // Core game state only
    public Player Player { get; private set; }
    public List<Droppable> Droppables { get; private set; }
    public GameState State { get; private set; }
    public IGridPlugin Plugin => _plugin;  // Just the interface!

    // Clean delegation - no casting
    public bool MovePlayer(Direction direction)
    {
        _plugin.OnPlayerMoved(oldPosition, Player.Position, this);
        if (_plugin.ShouldCollectDroppable(Player.Position, this))
            // ... collect
        if (_plugin.IsGameComplete(this))
            CompleteGame();
    }
}
```

**Game.cs now knows NOTHING about:**
- ShowTrail
- VisitedPositions
- PickUpPlugin/FillUpPlugin concrete types
- GridV2/GridV3 components

---

### 2. IGridPlugin - Minimal Interface

**Removed:**
- ❌ `Configure(Dictionary<string, object> config)` method

**Result:**
```csharp
public interface IGridPlugin
{
    string Name { get; }
    string Description { get; }

    // Core lifecycle hooks
    void OnGameStart(Game game);
    void OnGameReset(Game game);
    void OnPlayerMoved(Position oldPosition, Position newPosition, Game game);

    // Game mechanics
    bool ShouldCollectDroppable(Position position, Game game);
    void OnDroppableCollected(Position position, Game game);
    bool IsGameComplete(Game game);

    // Visual state (encapsulated)
    CellVisualState GetCellVisualState(Position position, Game game);
}
```

**No configuration API in the interface!** Plugins expose their own public methods for configuration.

---

### 3. Plugins - Own Their Configuration

**PickUpPlugin.cs:**
```csharp
public class PickUpPlugin : IGridPlugin
{
    // Private state - not exposed to Game
    private HashSet<Position> _visitedPositions = new();
    private bool _showTrail = false;

    // Public configuration API (plugin-specific)
    public void SetShowTrail(bool showTrail, Position currentPlayerPosition)
    {
        // Plugin owns this logic
    }

    // Read-only access for advanced scenarios
    public IReadOnlySet<Position> VisitedPositions => _visitedPositions;
}
```

**FillUpPlugin.cs:**
```csharp
public class FillUpPlugin : IGridPlugin
{
    // Private state
    private HashSet<Position> _visitedPositions = new();
    private HashSet<Position> _collectedPositions = new();
    private bool _showTrail = false;
    private int _visitThreshold = 5;

    // Plugin-specific configuration API
    public void SetShowTrail(bool showTrail, Position currentPlayerPosition, Game game);
    public void SetVisitThreshold(int threshold);

    // Read-only properties for UI/debugging
    public int VisitedCount => _visitedPositions.Count;
    public int VisitThreshold => _visitThreshold;
}
```

**Key Point:** Each plugin exposes its own configuration API. There's no forced uniformity through the interface.

---

### 4. Home.razor - Direct Plugin Configuration

**Before (tightly coupled through Game):**
```csharp
private Game game = new Game();

private void OnShowTrailChange(ChangeEventArgs e)
{
    showTrail = (bool)(e.Value ?? false);
    game.ShowTrail = showTrail;  // ❌ Goes through Game
}
```

**After (direct plugin access):**
```csharp
// Hold references to concrete plugins
private PickUpPlugin? pickUpPlugin = new PickUpPlugin();
private FillUpPlugin? fillUpPlugin = null;
private Game game = new Game(new PickUpPlugin());

private void OnShowTrailChange(ChangeEventArgs e)
{
    showTrail = (bool)(e.Value ?? false);

    // ✅ Configure plugin directly
    pickUpPlugin?.SetShowTrail(showTrail, game.Player.Position);
    fillUpPlugin?.SetShowTrail(showTrail, game.Player.Position, game);
}

private void OnGridVersionChange(ChangeEventArgs e)
{
    if (version == "v2")
    {
        pickUpPlugin = new PickUpPlugin();
        fillUpPlugin = null;
        game.SetPlugin(pickUpPlugin);
        gridComponentType = typeof(GridV2);
    }
    else if (version == "v3")
    {
        fillUpPlugin = new FillUpPlugin();
        pickUpPlugin = null;
        game.SetPlugin(fillUpPlugin);
        gridComponentType = typeof(GridV3);
    }
}
```

**Key Change:** Home.razor holds references to concrete plugins and configures them directly.

---

## Architecture Diagram

### Before (Tight Coupling)
```
Home.razor
    ↓ game.ShowTrail = value
Game.cs (knows about ShowTrail, VisitedPositions)
    ↓ if (_plugin is PickUpPlugin) { ... }  [❌ TYPE CASTING]
    ↓ pickUpPlugin.SetShowTrail(...)
PickUpPlugin (owns _visitedPositions)
```

### After (Clean Separation)
```
Home.razor
    ↓ pickUpPlugin?.SetShowTrail(...)  [✅ DIRECT]
PickUpPlugin (owns _visitedPositions, _showTrail)
    ↑
    └─ implements IGridPlugin
           ↑
           └─ Game.cs uses (NO TYPE CASTING)
```

**Game.cs is completely decoupled from:**
- Plugin configuration (ShowTrail, VisitThreshold)
- Plugin internal state (VisitedPositions, CollectedPositions)
- Concrete plugin types (PickUpPlugin, FillUpPlugin)
- UI components (GridV2, GridV3)

---

## Dependency Flow

### Configuration Flow
```
User toggles "ShowTrail" checkbox
    ↓
Home.razor.OnShowTrailChange()
    ↓
pickUpPlugin?.SetShowTrail(value, position)
    ↓
PickUpPlugin updates _showTrail and _visitedPositions
```

### Rendering Flow
```
GridV2.razor needs to render a cell
    ↓
var cellState = Game.Plugin.GetCellVisualState(position, game)
    ↓
PickUpPlugin.GetCellVisualState() returns CellVisualState
    ↓
GridV2.razor applies styling based on cellState.IsVisited
```

**No coupling through Game.cs!**

---

## Component Relationships

### Core Game (Game.cs)
**Depends on:**
- IGridPlugin (interface only)
- Player, Droppable, Position (domain entities)

**Does NOT depend on:**
- Concrete plugins (PickUpPlugin, FillUpPlugin)
- UI components (GridV2, GridV3, Home)
- Visualization concerns (ShowTrail, VisitedPositions)

### Plugins (PickUpPlugin, FillUpPlugin)
**Depends on:**
- IGridPlugin (implements)
- Game, Position (for context)

**Strongly coupled to:**
- Their respective UI components (GridV2 ↔ PickUpPlugin, GridV3 ↔ FillUpPlugin)

### Home.razor (UI Layer)
**Depends on:**
- Game (core game)
- Concrete plugins (PickUpPlugin, FillUpPlugin) for configuration
- UI components (GridV2, GridV3)

**Responsibility:**
- Manages UI state (showTrail checkbox)
- Creates and configures plugins
- Switches between plugins based on user selection

---

## Benefits

### ✅ Single Responsibility Principle
- Game.cs: Core game mechanics only
- Plugins: Game rules + visualization state
- Home.razor: UI coordination + plugin configuration

### ✅ Open/Closed Principle
- Add new plugins without modifying Game.cs
- Add new visualization features without touching core game

### ✅ Dependency Inversion
- Game.cs depends on IGridPlugin abstraction
- No dependency on concrete plugins
- No plugin type casting

### ✅ Clean Architecture Layers
```
UI Layer (Home.razor, GridV2, GridV3)
    ↓ configures
Plugin Layer (PickUpPlugin, FillUpPlugin)
    ↑ implements
Interface Layer (IGridPlugin, CellVisualState)
    ↑ uses
Core Domain Layer (Game, Player, Droppable)
```

---

## Example: Adding a New Plugin

### 1. Create RaceModePlugin
```csharp
public class RaceModePlugin : IGridPlugin
{
    private DateTime _startTime;
    private bool _showTimer = true;

    // Plugin-specific configuration
    public void SetShowTimer(bool show) => _showTimer = show;

    // Implement IGridPlugin interface
    public bool ShouldCollectDroppable(Position position, Game game) => true;
    public bool IsGameComplete(Game game) => game.RemainingDroppables == 0;
    // ... other methods
}
```

### 2. Create GridV4.razor
```razor
@using p07_vimkeys_game.Domain.Plugins

<div id="grid">
  @* Render logic using Game.Plugin.GetCellVisualState() *@
</div>
```

### 3. Update Home.razor
```csharp
private RaceModePlugin? raceModePlugin = null;

private void OnGridVersionChange(ChangeEventArgs e)
{
    if (version == "v4")
    {
        raceModePlugin = new RaceModePlugin();
        game.SetPlugin(raceModePlugin);
        gridComponentType = typeof(GridV4);
    }
}

// Add timer checkbox handler
private void OnShowTimerChange(ChangeEventArgs e)
{
    raceModePlugin?.SetShowTimer((bool)(e.Value ?? false));
}
```

**NO CHANGES to Game.cs!** ✅

---

## Summary

| Concern | Before | After |
|---------|--------|-------|
| **ShowTrail location** | Game.cs property | Plugin private field |
| **VisitedPositions location** | Game.cs property | Plugin private field |
| **Configuration path** | Home → Game → Plugin | Home → Plugin (direct) |
| **Plugin type checks** | `if (_plugin is PickUpPlugin)` | None! |
| **Game.cs dependencies** | Knows concrete plugins | IGridPlugin only |
| **Adding new plugin** | Modify Game.cs | Only modify Home.razor |

**Core game is now pure and focused on movement mechanics.** ✅
**Plugins own their visualization and game rules.** ✅
**Home.razor orchestrates UI and plugin configuration.** ✅

---

## Files Modified

```
✏️ Domain/Entities/Game.cs
   - Removed ShowTrail, VisitedPositions, VisitThreshold properties
   - Removed UpdatePluginConfiguration() method
   - Removed all plugin type casting
   - Cleaned Start() method

✏️ Domain/Plugins/IGridPlugin.cs
   - Removed Configure() method
   - Interface is now minimal and focused

✏️ Domain/Plugins/PickUpPlugin.cs
   - Removed Configure() implementation
   - Kept SetShowTrail() as public API

✏️ Domain/Plugins/FillUpPlugin.cs
   - Removed Configure() implementation
   - Kept SetShowTrail() and SetVisitThreshold() as public API

✏️ Pages/Home.razor
   - Added pickUpPlugin and fillUpPlugin references
   - Updated OnShowTrailChange to configure plugins directly
   - Updated OnGridVersionChange to maintain plugin references
   - Updated OnGameValidFormSubmit to configure plugins directly
   - Updated space key handler to configure plugins directly
```

**Result:** Clean separation of concerns with proper encapsulation!
