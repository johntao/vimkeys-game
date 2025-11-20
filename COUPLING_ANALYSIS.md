# ShowTrail and VisitedPositions Usage Analysis

## Current Usage Pattern

### 1. **Pages/Home.razor** (UI Layer)
```csharp
private bool showTrail = false;  // Local UI state

// User toggles trail (checkbox or space key)
private void OnShowTrailChange(ChangeEventArgs e)
{
    showTrail = (bool)(e.Value ?? false);
    game.ShowTrail = showTrail;  // ⚠️ Pushes to Game layer
}
```

**Role:** Maintains UI state, propagates changes to Game

---

### 2. **Domain/Entities/Game.cs** (Core Game Layer)

```csharp
private bool _showTrail = false;  // Internal state

public bool ShowTrail
{
    get => _showTrail;
    set
    {
        if (_showTrail != value)
        {
            _showTrail = value;
            UpdatePluginConfiguration();  // ⚠️ Generic notification

            // ⚠️ PROBLEM: Direct plugin type casting!
            if (_plugin is PickUpPlugin pickUpPlugin)
            {
                pickUpPlugin.SetShowTrail(value, Player.Position);
            }
            else if (_plugin is FillUpPlugin fillUpPlugin)
            {
                fillUpPlugin.SetShowTrail(value, Player.Position, this);
            }
        }
    }
}

// ⚠️ PROBLEM: Also in Start() method - duplicate plugin casting
public void Start()
{
    if (ShowTrail)
    {
        if (_plugin is PickUpPlugin pickUpPlugin)
        {
            pickUpPlugin.SetShowTrail(true, Player.Position);
        }
        else if (_plugin is FillUpPlugin fillUpPlugin)
        {
            fillUpPlugin.SetShowTrail(true, Player.Position, this);
        }
    }
}

// Backward compatibility property
public HashSet<Position> VisitedPositions
{
    get
    {
        if (_plugin is PickUpPlugin pickUpPlugin)
            return new HashSet<Position>(pickUpPlugin.VisitedPositions);
        else if (_plugin is FillUpPlugin fillUpPlugin)
            return new HashSet<Position>(fillUpPlugin.VisitedPositions);
        return new HashSet<Position>();
    }
}
```

**Role:** Coordinates between UI and plugins, but **tightly coupled** to concrete plugin types

---

### 3. **Domain/Plugins/** (Plugin Layer)

**PickUpPlugin.cs:**
```csharp
private HashSet<Position> _visitedPositions = new();
private bool _showTrail = false;

public void Configure(Dictionary<string, object> config)
{
    if (config.TryGetValue("ShowTrail", out var showTrail))
        _showTrail = (bool)showTrail;  // ⚠️ Notification #1
}

public void SetShowTrail(bool showTrail, Position currentPlayerPosition)
{
    var wasEnabled = _showTrail;
    _showTrail = showTrail;  // ⚠️ Notification #2 (duplicate!)

    if (!wasEnabled && showTrail)
        _visitedPositions.Add(currentPlayerPosition);
}

public IReadOnlySet<Position> VisitedPositions => _visitedPositions;
```

**FillUpPlugin.cs:**
```csharp
// Same pattern, but SetShowTrail has 3 parameters (inconsistent!)
public void SetShowTrail(bool showTrail, Position currentPlayerPosition, Game game)
{
    // Implementation differs from PickUpPlugin
}
```

**Role:** Owns the actual trail state, but has **dual notification system** (Configure + SetShowTrail)

---

## Problems Identified

### 🔴 **Problem 1: Tight Coupling to Concrete Plugin Types**

```csharp
// Game.cs ShowTrail setter
if (_plugin is PickUpPlugin pickUpPlugin)
    pickUpPlugin.SetShowTrail(value, Player.Position);
else if (_plugin is FillUpPlugin fillUpPlugin)
    fillUpPlugin.SetShowTrail(value, Player.Position, this);
```

**Why it's bad:**
- Game.cs knows about specific plugin implementations
- Adding a new plugin requires modifying Game.cs
- Violates Open/Closed Principle

---

### 🔴 **Problem 2: Dual Notification System**

```csharp
// Game.ShowTrail setter calls BOTH:
UpdatePluginConfiguration();  // Calls Configure()
pickUpPlugin.SetShowTrail();  // Direct method call
```

**Why it's bad:**
- Plugin gets notified twice
- Confusion about which method to use
- `SetShowTrail()` is not in IGridPlugin interface

---

### 🔴 **Problem 3: Inconsistent Plugin APIs**

```csharp
// PickUpPlugin
public void SetShowTrail(bool showTrail, Position currentPlayerPosition)

// FillUpPlugin
public void SetShowTrail(bool showTrail, Position currentPlayerPosition, Game game)
```

**Why it's bad:**
- Different signatures for same concept
- Can't call polymorphically
- Hard to maintain

---

### 🔴 **Problem 4: Exposed VisitedPositions**

```csharp
// On plugins
public IReadOnlySet<Position> VisitedPositions => _visitedPositions;

// On Game.cs
public HashSet<Position> VisitedPositions
{
    get { /* Plugin casting logic */ }
}
```

**Why it's bad:**
- Only exists for backward compatibility
- Leaks plugin internal state
- Should use GetCellVisualState() instead

---

## Recommendations for Decoupling

### ✅ **Solution 1: Move SetShowTrail to IGridPlugin Interface**

```csharp
// IGridPlugin.cs
public interface IGridPlugin
{
    // ... existing methods ...

    /// <summary>
    /// Called when trail visualization setting changes
    /// </summary>
    void OnTrailChanged(bool enabled, Position currentPlayerPosition, Game game);
}
```

**Benefits:**
- No more plugin type casting in Game.cs
- Polymorphic calls
- All plugins implement same contract

---

### ✅ **Solution 2: Single Notification Path**

```csharp
// Game.cs - Remove dual notification
public bool ShowTrail
{
    get => _showTrail;
    set
    {
        if (_showTrail != value)
        {
            _showTrail = value;
            // Option A: Just update config, let plugin handle it
            UpdatePluginConfiguration();

            // Option B: Single lifecycle hook (preferred)
            _plugin.OnTrailChanged(value, Player.Position, this);
        }
    }
}
```

**Benefits:**
- Single source of truth
- Clear notification path
- No duplicate logic

---

### ✅ **Solution 3: Remove VisitedPositions from Public API**

```csharp
// Instead of:
var visited = game.VisitedPositions;

// Use:
var cellState = game.Plugin.GetCellVisualState(position, game);
if (cellState.IsVisited) { /* ... */ }
```

**Benefits:**
- Cleaner encapsulation
- Forces use of proper abstraction
- Easier to extend (e.g., add IsHighlighted, IsPath, etc.)

---

### ✅ **Solution 4: Home.razor Owns Trail State**

```csharp
// Home.razor - Keep trail as pure UI state
private bool showTrail = false;

// When plugin changes, sync UI state to plugin
private void OnGridVersionChange(ChangeEventArgs e)
{
    var plugin = CreatePlugin(version);
    game.SetPlugin(plugin);

    // Sync current UI state to new plugin
    game.ShowTrail = showTrail;
}
```

**Benefits:**
- UI layer owns UI concerns
- Clear data flow: UI → Game → Plugin
- No bidirectional dependency

---

## Proposed Refactoring (Minimal Changes)

### Step 1: Add to IGridPlugin

```csharp
void OnTrailChanged(bool enabled, Position currentPlayerPosition, Game game);
```

### Step 2: Update Game.cs

```csharp
public bool ShowTrail
{
    get => _showTrail;
    set
    {
        if (_showTrail != value)
        {
            _showTrail = value;
            _plugin.OnTrailChanged(value, Player.Position, this);
        }
    }
}

// In Start() method - remove plugin casting
public void Start()
{
    StartTime = DateTime.UtcNow;
    State = GameState.Playing;
    _plugin.OnGameStart(this);

    // Plugin handles trail initialization via OnTrailChanged
    if (ShowTrail)
    {
        _plugin.OnTrailChanged(true, Player.Position, this);
    }
}
```

### Step 3: Update Plugins

```csharp
// PickUpPlugin & FillUpPlugin
public void OnTrailChanged(bool enabled, Position currentPlayerPosition, Game game)
{
    var wasEnabled = _showTrail;
    _showTrail = enabled;

    if (!wasEnabled && enabled)
    {
        // Plugin-specific logic for enabling trail
        _visitedPositions.Add(currentPlayerPosition);
    }
}

// Remove SetShowTrail() methods (replaced by OnTrailChanged)
```

### Step 4: Mark VisitedPositions as Obsolete

```csharp
// Game.cs
[Obsolete("Use Plugin.GetCellVisualState() instead")]
public HashSet<Position> VisitedPositions { get { /* ... */ } }
```

---

## Dependency Flow (After Refactoring)

```
User Action (checkbox/space)
    ↓
Home.razor (showTrail state)
    ↓
game.ShowTrail = value
    ↓
Game.cs (ShowTrail setter)
    ↓
_plugin.OnTrailChanged(...)
    ↓
PickUpPlugin/FillUpPlugin (owns _visitedPositions)
    ↓
Used by GetCellVisualState()
    ↓
GridV2/GridV3 Razor components
```

**Clean, unidirectional flow!**

---

## Summary

| Component | Current Responsibility | Should Be |
|-----------|----------------------|-----------|
| **Home.razor** | UI state + pushes to Game | UI state only |
| **Game.cs** | Coordinates + casts to plugins | Delegates to IGridPlugin |
| **Plugins** | Dual notification (Configure + SetShowTrail) | Single notification via interface |
| **VisitedPositions** | Exposed on Game + Plugins | Hidden, use GetCellVisualState() |

**Key Improvement:** Add `OnTrailChanged()` to IGridPlugin interface and remove all plugin type casting from Game.cs.
