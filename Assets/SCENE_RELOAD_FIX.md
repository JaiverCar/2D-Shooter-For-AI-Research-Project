# Scene Reload Fix - Summary

## Problem
When the level restarted, all vision layers would disappear because:
1. The TerrainAnalysis singleton persisted across scene reloads
2. But its internal state (layers list, enemyVisionLayers dictionary) wasn't being cleared
3. Old grid references were kept, causing issues with new scenes

## Solution Applied

### 1. Added OnEnable() Method
```csharp
void OnEnable()
{
    // Clear old data when scene reloads
    if (layers != null)
    {
        layers.Clear();
    }
    if (enemyVisionLayers != null)
    {
        enemyVisionLayers.Clear();
    }
    
    grid = null; // Force grid reference refresh
}
```
- Clears all layers when the component is enabled (happens on scene reload)
- Clears the enemy vision dictionary
- Resets grid reference to force refresh

### 2. Updated Start() Method
```csharp
void Start()
{
    grid = AStarGrid.Instance;
    
    // Only setup common layers if they don't exist yet
    if (layers.Count == 0)
    {
        SetupCommonLayers();
    }
}
```
- Refreshes grid reference
- Only creates common layers if none exist (prevents duplicates)

### 3. Added Null Safety Checks
All methods now check if `enemyVisionLayers` is null before accessing:
- `RegisterEnemy()` - Initializes dictionary if null
- `UnregisterEnemy()` - Checks for null before accessing
- `UpdateEnemyVisionLayer()` - Checks for null before accessing
- `UpdateAllEnemyVisionLayers()` - Checks for null before iterating
- `GetEnemyVisionLayer()` - Checks for null before accessing
- `ToggleEnemyVision()` - Checks for null before accessing

### 4. Improved Enemy Registration
- Now handles duplicate registrations gracefully
- Reuses existing layer if enemy is already registered
- Prevents warnings and duplicate layers

## How It Works Now

1. **Scene Loads** → `OnEnable()` clears old data
2. **Start() Runs** → Grid reference refreshed, common layers created
3. **Enemies Spawn** → Each enemy registers and gets a new layer
4. **Scene Reloads** → Process repeats, old data is cleared first

## Testing
- Start the game → Enemies get vision layers ✓
- Restart level → Old layers cleared, new ones created ✓
- Enemies re-register → Each gets a fresh vision layer ✓
- Vision-based aggro works → Enemies detect player properly ✓

## Benefits
- No memory leaks from old layers
- Clean state on each scene reload
- Enemies always have valid vision data
- No duplicate layer registrations
- Proper singleton lifecycle management
