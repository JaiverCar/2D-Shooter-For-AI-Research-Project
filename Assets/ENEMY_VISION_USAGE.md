# Enemy Vision Layer System - Usage Guide

## Overview
Each enemy now automatically gets their own vision layer that tracks what they can see in real-time! Enemies can use this vision data to intelligently decide when to aggro the player.

## Vision-Based Aggro System
Enemies now have **two aggro modes**:

### 1. Vision-Based Aggro (NEW!)
- **Enabled by default** - Enemies use their vision layer to detect the player
- **Realistic detection** - Enemies only aggro when they can actually see the player
- **Respects FOV** - Enemies won't detect players behind them
- **Respects obstacles** - Enemies won't detect players through walls

### 2. Distance-Based Aggro (Legacy)
- **Fallback mode** - Simple distance check
- **Less realistic** - Can "see" through walls
- **Still available** - Disable `Use Vision Based Aggro` to use this mode

## Inspector Settings (EnemyLogic component)

### Vision Aggro Settings
- **Use Vision Based Aggro** - Toggle vision-based detection on/off (default: ON)
- **Vision Aggro Threshold** - How visible the player must be (0-1, default: 0.1)
  - `0.0` = Enemy aggros if player is barely visible
  - `0.5` = Enemy aggros if player is moderately visible
  - `1.0` = Enemy aggros only if player is fully visible
- **Aggro Range** - Still used for deaggro distance calculations
- **Min Deaggro Range** - Distance at which enemy gives up chase

### TerrainAnalysis Settings
- **Enemy FOV Angle** - Field of view for enemies (default: 182°)
- **Enemy Vision Color Scheme** - Color scheme for enemy vision (Heatmap/BlueRed/etc.)
- **Enemy Vision Alpha** - Transparency of enemy vision overlay (default: 0.6)

## Automatic Setup
- **Enemies auto-register** when they spawn (in `Start()`)
- **Enemies auto-unregister** when they die (in `OnDestroy()`)
- Vision layers **update every frame** when enabled

## Keyboard Controls
- **E** - Toggle ALL enemy vision layers on/off
- **V** - Toggle Visibility layers
- **O** - Toggle Openness layers
- **U** - Toggle Occupancy layers
- **C** - Toggle Custom layers
- **G** - Debug info (shows layer count, enemy count, etc.)

## Inspector Settings (TerrainAnalysis component)
- **Enemy FOV Angle** - Field of view for enemies (default: 182°)
- **Enemy Vision Color Scheme** - Color scheme for enemy vision (Heatmap/BlueRed/etc.)
- **Enemy Vision Alpha** - Transparency of enemy vision overlay (default: 0.6)
- **Show Enemy Vision Layers** - Master toggle for enemy vision

## Manual Control (Optional)
You can manually control enemy vision layers via code:

```csharp
// Register an enemy manually
TerrainAnalysis.Instance.RegisterEnemy(enemyTransform, "Custom Name");

// Unregister an enemy manually
TerrainAnalysis.Instance.UnregisterEnemy(enemyTransform);

// Update a specific enemy's vision
TerrainAnalysis.Instance.UpdateEnemyVisionLayer(enemyTransform);

// Toggle a specific enemy's vision visibility
TerrainAnalysis.Instance.ToggleEnemyVision(enemyTransform);

// Get the layer for a specific enemy
LayerVisualization layer = TerrainAnalysis.Instance.GetEnemyVisionLayer(enemyTransform);
```

## How It Works
1. When an enemy spawns, it registers with `TerrainAnalysis`
2. A new vision layer is created with type `EnemyVision`
3. Every frame, if enemy vision is enabled, the layer updates to show what that enemy can see
4. When the enemy is destroyed, its layer is automatically removed

## Visualization
- Look at the **Scene view** (not Game view) to see the gizmos
- Each enemy's vision appears as a colored overlay on the grid
- Areas the enemy can see are highlighted based on their FOV and line of sight
- Multiple enemy visions blend together when overlapping

## Performance Tips
- Vision layers only update when `showEnemyVisionLayers` or `showAllEnemyVision` is enabled
- Toggle off enemy vision (press E) when you don't need to see it
- Each enemy's vision is calculated independently

## Color Schemes Available
- **Heatmap** - Multi-color gradient (black→blue→cyan→green→yellow→red→white)
- **BlueRed** - Blue to red gradient
- **GreenRed** - Green to red gradient  
- **Grayscale** - Black to white
- **Custom** - Define your own low/high colors

## Troubleshooting
- **No colors showing?** 
  - Check that "Show Visualization" is enabled in Inspector
  - Make sure you're looking at Scene view, not Game view
  - Press E to make sure enemy vision is enabled
  - Press G to see debug info

- **Vision not updating?**
  - Check that enemies have `EnemyLogic` component
  - Verify `TerrainAnalysis.Instance` is not null

- **Too many layers?**
  - Layers auto-clean up when enemies die
  - Press G to see how many enemies are tracked

- **Enemies not detecting player?**
  - Check `Use Vision Based Aggro` is enabled on EnemyLogic
  - Verify `Vision Aggro Threshold` isn't set too high (try 0.1)
  - Make sure enemy is facing the player (respects FOV)
  - Check if walls are blocking line of sight
  - Look at Scene view to see enemy's actual vision cone

- **Enemies detecting player through walls?**
  - You're probably using Distance-Based Aggro
  - Enable `Use Vision Based Aggro` in EnemyLogic Inspector

- **Enemies too sensitive/not sensitive enough?**
  - Adjust `Vision Aggro Threshold` on EnemyLogic component
  - Lower = more sensitive (0.0 = detects anything)
  - Higher = less sensitive (1.0 = only fully visible targets)

## How Vision-Based Aggro Works

1. **Every frame**, each enemy's vision layer is updated to show what they can see
2. **During aggro check**, the enemy looks up the player's position in its vision layer
3. **If vision value >= threshold**, the enemy aggros
4. **Player position** is converted to grid coordinates to check vision data
5. **Obstacles and FOV** are automatically respected by the vision calculation

## Performance Notes
- Vision layers only update when visualization OR aggro checks are active
- Each enemy maintains its own vision calculation
- Vision data is shared between visualization and aggro logic (efficient!)

## Debug Tips
- **Press E** to visualize what enemies can see
- **Press G** to see layer statistics
- **Watch console** for "detected player via VISION!" messages
- **Adjust threshold** in real-time via Inspector while game is running
- **Toggle modes** to compare vision-based vs distance-based behavior
