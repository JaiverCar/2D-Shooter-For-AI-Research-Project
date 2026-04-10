
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;



// Helper class for storing float values per grid cell
public class MapLayer<T>
{
    private T[,] data;
    private int width;
    private int height;

    public MapLayer(int width, int height)
    {
        this.width = width;
        this.height = height;
        data = new T[height, width];
    }

    public T GetValue(int row, int col)
    {
        if (row < 0 || row >= height || col < 0 || col >= width)
            return default(T);
        return data[row, col];
    }

    public T GetValue(Vector2 pos)
    {
        return GetValue((int)pos.x, (int)pos.y);
    }

    public void SetValue(int row, int col, T value)
    {
        if (row >= 0 && row < height && col >= 0 && col < width)
            data[row, col] = value;
    }

    public void SetValue(Vector2 pos, T value)
    {
        SetValue((int)pos.x, (int)pos.y, value);
    }

    public void Clear(T value = default(T))
    {
        for (int r = 0; r < height; r++)
            for (int c = 0; c < width; c++)
                data[r, c] = value;
    }
}

// Visualization settings for a single layer
[System.Serializable]
public class LayerVisualization
{
    public string layerName = "Unnamed Layer";
    public LayerType layerType = LayerType.Custom;
    public bool enabled = true;
    [HideInInspector] public MapLayer<float> layer;

    [Header("Color Settings")]
    public ColorScheme colorScheme = ColorScheme.Heatmap;
    [Range(0f, 1f)] public float alpha = 0.7f;
    public float minValue = 0f;
    public float maxValue = 1f;

    [Header("Optional Custom Colors")]
    public Color lowValueColor = Color.blue;
    public Color highValueColor = Color.red;
}

public enum ColorScheme
{
    Heatmap,        // Black -> Blue -> Cyan -> Green -> Yellow -> Red -> White
    BlueRed,        // Blue -> Red gradient
    GreenRed,       // Green -> Red gradient
    Grayscale,      // Black -> White
    Custom          // Use custom colors
}

public enum LayerType
{
    Visibility,
    Openness,
    EnemyVision,
    Occupancy,
    Custom
}

public class TerrainAnalysis : MonoBehaviour
{
    // Singleton pattern
    public static TerrainAnalysis Instance { get; private set; }

    private AStarGrid grid;

    [Header("Visualization")]
    [Tooltip("Enable to show layer visualization in Scene view")]
    public bool showVisualization = false;

    [Tooltip("Show all enemy vision layers")]
    public bool showAllEnemyVision = false;

    [Tooltip("All layers to visualize - each can have different colors and settings")]
    public List<LayerVisualization> layers = new List<LayerVisualization>();

    [Header("Layer Type Visibility (Keyboard Shortcuts)")]
    [Tooltip("V - Toggle Visibility Layers")]
    public bool showVisibilityLayers = true;
    [Tooltip("O - Toggle Openness Layers")]
    public bool showOpennessLayers = true;
    [Tooltip("E - Toggle Enemy Vision Layers")]
    public bool showEnemyVisionLayers = true;
    [Tooltip("U - Toggle Occupancy Layers")]
    public bool showOccupancyLayers = true;
    [Tooltip("C - Toggle Custom Layers")]
    public bool showCustomLayers = true;

    [Header("Enemy Vision Settings")]
    [Tooltip("FOV angle for enemy vision layers (degrees)")]
    public float enemyFOVAngle = 182f;
    [Tooltip("Color scheme for enemy vision layers")]
    public ColorScheme enemyVisionColorScheme = ColorScheme.Heatmap;
    [Tooltip("Alpha/transparency for enemy vision layers")]
    [Range(0f, 1f)]
    public float enemyVisionAlpha = 0.6f;

    // Track enemy vision layers
    private Dictionary<Transform, LayerVisualization> enemyVisionLayers = new Dictionary<Transform, LayerVisualization>();

    void Awake()
    {
        Instance = this;

        // Force initialization here to ensure it happens before scene values override
        InitializeOnSceneLoad();
    }

    private void InitializeOnSceneLoad()
    {
        // Clear old data
        if (layers == null)
            layers = new List<LayerVisualization>();
        else
            layers.Clear();

        if (enemyVisionLayers == null)
            enemyVisionLayers = new Dictionary<Transform, LayerVisualization>();
        else
            enemyVisionLayers.Clear();

        grid = null; // Force grid reference refresh

        // FORCE visualization to be on
        showVisualization = true;
        showVisibilityLayers = true;
        showOpennessLayers = true;
        showEnemyVisionLayers = true;
        showOccupancyLayers = true;
        showCustomLayers = true;

        Debug.Log("TerrainAnalysis: Initialized - Visualization ENABLED");
    }

    void Start()
    {
        // Get grid reference
        if (grid == null)
        {
            grid = AStarGrid.Instance;
        }

        // Setup common layers - ONLY in Start when grid is ready!
        if (grid != null)
        {
            SetupCommonLayers();
            Debug.Log($"TerrainAnalysis: Created {layers.Count} common layers in Start()");
        }
        else
        {
            Debug.LogError("TerrainAnalysis: Grid not ready in Start()!");
        }
    }


    void Update()
    {
        // Keyboard shortcuts to toggle layer types
        if (Input.GetKeyDown(KeyCode.V))
        {
            showVisibilityLayers = !showVisibilityLayers;
            Debug.Log($"Visibility Layers: {(showVisibilityLayers ? "ON" : "OFF")} | Total layers: {layers.Count}");
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            showOpennessLayers = !showOpennessLayers;
            Debug.Log($"Openness Layers: {(showOpennessLayers ? "ON" : "OFF")} | Total layers: {layers.Count}");
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            showEnemyVisionLayers = !showEnemyVisionLayers;
            Debug.Log($"Enemy Vision Layers: {(showEnemyVisionLayers ? "ON" : "OFF")} | Total layers: {layers.Count}");
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            showOccupancyLayers = !showOccupancyLayers;
            Debug.Log($"Occupancy Layers: {(showOccupancyLayers ? "ON" : "OFF")} | Total layers: {layers.Count}");
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            showCustomLayers = !showCustomLayers;
            Debug.Log($"Custom Layers: {(showCustomLayers ? "ON" : "OFF")} | Total layers: {layers.Count}");
        }

        // Debug: Press G to see gizmo status
        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log($"=== GIZMO DEBUG ===");
            Debug.Log($"showVisualization: {showVisualization}");
            Debug.Log($"Grid null: {grid == null}");
            Debug.Log($"Total layers: {layers.Count}");
            for (int i = 0; i < layers.Count; i++)
            {
                var layer = layers[i];
                Debug.Log($"Layer {i}: {layer.layerName} | Type: {layer.layerType} | Enabled: {layer.enabled} | Data null: {layer.layer == null}");
            }
            Debug.Log($"Tracked enemies: {enemyVisionLayers.Count}");
        }

        // Update all enemy vision layers
        if (showAllEnemyVision || showEnemyVisionLayers)
        {
            UpdateAllEnemyVisionLayers();
        }
    }

    // Helper method to create a new layer with correct dimensions
    public MapLayer<float> CreateLayer()
    {
        if (grid == null)
            grid = AStarGrid.Instance;
        return new MapLayer<float>(grid.gridSizeX, grid.gridSizeY);
    }

    // Add a layer for visualization
    public LayerVisualization AddLayer(string name, ColorScheme scheme = ColorScheme.Heatmap, float alpha = 0.7f, LayerType layerType = LayerType.Custom)
    {
        LayerVisualization vis = new LayerVisualization
        {
            layerName = name,
            layerType = layerType,
            layer = CreateLayer(),
            colorScheme = scheme,
            alpha = alpha,
            enabled = true
        };
        layers.Add(vis);
        return vis;
    }

    // Example: Create common analysis layers
    public void SetupCommonLayers()
    {
        // Openness layer (blue-red gradient)
        var opennessVis = AddLayer("Openness", ColorScheme.BlueRed, 0.4f, LayerType.Openness);
        AnalyzeOpenness(opennessVis.layer);

        // Visibility layer (green-red gradient)
        var visibilityVis = AddLayer("Visibility", ColorScheme.GreenRed, 0.4f, LayerType.Visibility);
        AnalyzeVisibility(visibilityVis.layer);

        showVisualization = true;
    }

    // Register an enemy to get its own vision layer
    public LayerVisualization RegisterEnemy(Transform enemy, string enemyName = null)
    {
        // Initialize dictionary if null
        if (enemyVisionLayers == null)
        {
            enemyVisionLayers = new Dictionary<Transform, LayerVisualization>();
        }

        // If already registered, just return existing layer
        if (enemyVisionLayers.ContainsKey(enemy))
        {
            Debug.Log($"Enemy {enemy.name} already registered, reusing existing layer.");
            return enemyVisionLayers[enemy];
        }

        string layerName = enemyName ?? $"Enemy Vision - {enemy.name}";
        var visionLayer = AddLayer(layerName, ColorScheme.Custom, enemyVisionAlpha, LayerType.EnemyVision);
        visionLayer.lowValueColor = Color.clear;
        visionLayer.highValueColor = Color.red;
        enemyVisionLayers[enemy] = visionLayer;

        // Initial update
        UpdateEnemyVisionLayer(enemy);

        Debug.Log($"Registered enemy vision layer: {layerName}");
        return visionLayer;
    }

    // Unregister an enemy and remove its vision layer
    public void UnregisterEnemy(Transform enemy)
    {
        if (enemyVisionLayers == null || !enemyVisionLayers.ContainsKey(enemy))
            return;

        var layer = enemyVisionLayers[enemy];
        if (layers != null)
        {
            layers.Remove(layer);
        }
        enemyVisionLayers.Remove(enemy);

        Debug.Log($"Unregistered enemy vision layer: {layer.layerName}");
    }

    // Update a specific enemy's vision layer
    public void UpdateEnemyVisionLayer(Transform enemy)
    {
        if (enemyVisionLayers == null || !enemyVisionLayers.ContainsKey(enemy))
            return;

        var visionLayer = enemyVisionLayers[enemy];
        if (visionLayer.layer != null)
        {
            visionLayer.layer.Clear(0f);
            AnalyzeAgentVision(visionLayer.layer, enemy, enemyFOVAngle);
        }
    }

    // Update all registered enemy vision layers
    private void UpdateAllEnemyVisionLayers()
    {
        if (enemyVisionLayers == null)
            return;

        // Clean up any destroyed enemies
        List<Transform> toRemove = new List<Transform>();
        foreach (var enemy in enemyVisionLayers.Keys)
        {
            if (enemy == null)
                toRemove.Add(enemy);
        }

        foreach (var enemy in toRemove)
        {
            UnregisterEnemy(enemy);
        }

        // Update remaining enemies
        foreach (var enemy in enemyVisionLayers.Keys)
        {
            UpdateEnemyVisionLayer(enemy);
        }
    }

    // Get the vision layer for a specific enemy
    public LayerVisualization GetEnemyVisionLayer(Transform enemy)
    {
        if (enemyVisionLayers != null && enemyVisionLayers.ContainsKey(enemy))
            return enemyVisionLayers[enemy];
        return null;
    }

    // Toggle visibility of a specific enemy's vision layer
    public void ToggleEnemyVision(Transform enemy)
    {
        if (enemyVisionLayers == null || !enemyVisionLayers.ContainsKey(enemy))
            return;

        var layer = enemyVisionLayers[enemy];
        layer.enabled = !layer.enabled;
        Debug.Log($"{layer.layerName}: {(layer.enabled ? "ON" : "OFF")}");
    }

    void OnDrawGizmos()
    {
        if (!showVisualization)
            return;

        if (grid == null)
            grid = AStarGrid.Instance;

        if (layers == null || layers.Count == 0 || grid == null)
            return;

        float nodeSize = grid.nodeRadius * 2f * 0.9f;

        for (int x = 0; x < grid.gridSizeX; x++)
        {
            for (int y = 0; y < grid.gridSizeY; y++)
            {
                if (!grid.IsWalkable(x, y))
                    continue;

                Node node = grid.GridGet(new Vector2(x, y));

                // Blend all enabled layers together
                Color finalColor = Color.clear;
                int layerCount = 0;

                foreach (var layerVis in layers)
                {
                    if (!layerVis.enabled || layerVis.layer == null)
                        continue;

                    // Check if this layer type should be shown
                    if (!ShouldShowLayerType(layerVis.layerType))
                        continue;

                    float value = layerVis.layer.GetValue(y, x);
                    Color layerColor = GetColorForScheme(value, layerVis);

                    // Blend colors additively with alpha
                    finalColor += layerColor * layerVis.alpha;
                    layerCount++;
                }

                if (layerCount > 0)
                {

                    finalColor.a = Mathf.Clamp01(finalColor.a);
                    Gizmos.color = finalColor;
                    Gizmos.DrawCube(node.worldPosition, Vector3.one * nodeSize);
                }
            }
        }
    }

    private bool ShouldShowLayerType(LayerType type)
    {
        switch (type)
        {
            case LayerType.Visibility:
                return showVisibilityLayers;
            case LayerType.Openness:
                return showOpennessLayers;
            case LayerType.EnemyVision:
                return showEnemyVisionLayers;
            case LayerType.Occupancy:
                return showOccupancyLayers;
            case LayerType.Custom:
                return showCustomLayers;
            default:
                return true;
        }
    }

    private Color GetColorForScheme(float value, LayerVisualization vis)
    {
        float normalized = Mathf.Clamp01((value - vis.minValue) / (vis.maxValue - vis.minValue));

        switch (vis.colorScheme)
        {
            case ColorScheme.BlueRed:
                return Color.Lerp(Color.blue, Color.red, normalized);

            case ColorScheme.GreenRed:
                return Color.Lerp(Color.green, Color.red, normalized);

            case ColorScheme.Grayscale:
                return Color.Lerp(Color.black, Color.white, normalized);

            case ColorScheme.Custom:
                return Color.Lerp(vis.lowValueColor, vis.highValueColor, normalized);

            case ColorScheme.Heatmap:
            default:
                return GetHeatmapColor(normalized);
        }
    }

    private Color GetHeatmapColor(float normalized)
    {
        // Create a heatmap: black -> blue -> cyan -> green -> yellow -> red -> white
        if (normalized < 0.2f)
        {
            float t = normalized / 0.2f;
            return Color.Lerp(Color.black, Color.blue, t);
        }
        else if (normalized < 0.4f)
        {
            float t = (normalized - 0.2f) / 0.2f;
            return Color.Lerp(Color.blue, Color.cyan, t);
        }
        else if (normalized < 0.6f)
        {
            float t = (normalized - 0.4f) / 0.2f;
            return Color.Lerp(Color.cyan, Color.green, t);
        }
        else if (normalized < 0.8f)
        {
            float t = (normalized - 0.6f) / 0.2f;
            return Color.Lerp(Color.green, Color.yellow, t);
        }
        else if (normalized < 0.95f)
        {
            float t = (normalized - 0.8f) / 0.15f;
            return Color.Lerp(Color.yellow, Color.red, t);
        }
        else
        {
            float t = (normalized - 0.95f) / 0.05f;
            return Color.Lerp(Color.red, Color.white, t);
        }
    }

    // Helper function for line intersection (needed for is_clear_path)
    private bool LineIntersect(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        float denom = ((p4.y - p3.y) * (p2.x - p1.x) - (p4.x - p3.x) * (p2.y - p1.y));

        if (math.abs(denom) < 0.0001f)
            return false;

        float ua = ((p4.x - p3.x) * (p1.y - p3.y) - (p4.y - p3.y) * (p1.x - p3.x)) / denom;
        float ub = ((p2.x - p1.x) * (p1.y - p3.y) - (p2.y - p1.y) * (p1.x - p3.x)) / denom;

        return (ua >= 0.0f && ua <= 1.0f && ub >= 0.0f && ub <= 1.0f);
    }

    // Return the four corners of a cell (for checking wall sides)
    private Vector2[] GetCellCorners(int row, int col)
    {
        // Get the actual size of each cell (node diameter)
        float cellSize = grid.nodeRadius * 2f;
        float halfSize = cellSize * 0.5f;

        // Add small padding to catch diagonal lines passing by corners
        float padding = 0.01f;
        float halfW = halfSize + padding;
        float halfH = halfSize + padding;

        Node node = grid.GridGet(row, col);
        Vector2 center = node.worldPosition;

        // Return corners: TL, TR, BR, BL
        return new Vector2[]
        {
            new Vector2(center.x - halfW, center.y - halfH), // Top-left
            new Vector2(center.x + halfW, center.y - halfH), // Top-right
            new Vector2(center.x + halfW, center.y + halfH), // Bottom-right
            new Vector2(center.x - halfW, center.y + halfH)  // Bottom-left
        };
    }

    public float DistanceToClosestWall(int row, int col)
    {
        int upperColBound = grid.gridSizeX + 1;
        int lowerColBound = -1;

        if (col > grid.gridSizeX - col)
            lowerColBound = col - (grid.gridSizeX - col);
        else
            upperColBound = col + col + 1;

        int upperRowBound = grid.gridSizeY + 1;
        int lowerRowBound = -1;

        if (row > grid.gridSizeY - row)
            lowerRowBound = row - (grid.gridSizeY - row);
        else
            upperRowBound = row + row + 1;

        float shortestDist = float.PositiveInfinity;

        for (int r = lowerRowBound; r < upperRowBound; r++)
        {
            for (int c = lowerColBound; c < upperColBound; c++)
            {
                Vector2 currPos = new Vector2(c, r); // x=col, y=row

                if (!grid.IsValidGridPos(currPos) || !grid.IsWalkable(c, r)) // IsWalkable takes (x, y) = (col, row)
                {
                    float dx = math.abs(r - row);
                    float dy = math.abs(c - col);
                    float tempDist = math.sqrt(dx * dx + dy * dy);

                    if (tempDist < shortestDist)
                        shortestDist = tempDist;
                }
            }
        }

        return shortestDist;
    }

    public bool IsClearPath(int row0, int col0, int row1, int col1)
    {
        Node firstNode = grid.GridGet(row0, col0);
        Node secondNode = grid.GridGet(row1, col1);

        Vector2 firstPos = firstNode.worldPosition;
        Vector2 secondPos = secondNode.worldPosition;

        int smallestRow = math.min(row0, row1);
        int smallestCol = math.min(col0, col1);
        int largestRow = math.max(row0, row1);
        int largestCol = math.max(col0, col1);

        // Expand search by one cell
        smallestRow = math.max(0, smallestRow - 1);
        smallestCol = math.max(0, smallestCol - 1);
        largestRow = math.min(largestRow + 1, grid.gridSizeY - 1);
        largestCol = math.min(largestCol + 1, grid.gridSizeX - 1);

        for (int r = smallestRow; r <= largestRow; r++)
        {
            for (int c = smallestCol; c <= largestCol; c++)
            {
                if (!grid.IsWalkable(c, r)) // IsWalkable takes (x, y) = (col, row)
                {
                    Vector2[] corners = GetCellCorners(r, c);

                    if (LineIntersect(firstPos, secondPos, corners[0], corners[1]))
                        return false;
                    if (LineIntersect(firstPos, secondPos, corners[1], corners[2]))
                        return false;
                    if (LineIntersect(firstPos, secondPos, corners[2], corners[3]))
                        return false;
                    if (LineIntersect(firstPos, secondPos, corners[3], corners[0]))
                        return false;
                }
            }
        }

        return true;
    }

    public void AnalyzeOpenness(MapLayer<float> layer)
    {
        for (int r = 0; r < grid.gridSizeY; r++)
        {
            for (int c = 0; c < grid.gridSizeX; c++)
            {
                Vector2 currPos = new Vector2(c, r); // x=col, y=row

                if (grid.IsValidGridPos(currPos) && grid.IsWalkable(c, r)) // IsWalkable takes (x, y) = (col, row)
                {
                    float d = DistanceToClosestWall(r, c);
                    float opennessVal = 1.0f / (d * d);
                    layer.SetValue(r, c, opennessVal);
                }
            }
        }
    }

    public void AnalyzeVisibility(MapLayer<float> layer)
    {
        int rows = grid.gridSizeY;
        int cols = grid.gridSizeX;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (!grid.IsWalkable(c, r)) // IsWalkable takes (x, y) = (col, row)
                    continue;

                float visibleCount = 0.0f;
                bool skipCell = false;

                for (int r1 = 0; r1 < rows; r1++)
                {
                    for (int c1 = 0; c1 < cols; c1++)
                    {
                        if ((r1 == r && c1 == c) || !grid.IsWalkable(c1, r1)) // IsWalkable takes (x, y) = (col, row)
                            continue;

                        if (IsClearPath(r, c, r1, c1))
                            visibleCount += 1.0f;

                        if (visibleCount >= 160)
                        {
                            skipCell = true;
                            break;
                        }
                    }

                    if (skipCell)
                        break;
                }

                visibleCount = visibleCount / 160.0f;
                visibleCount = math.min(visibleCount, 1.0f);
                layer.SetValue(r, c, visibleCount);
            }
        }
    }

    public void AnalyzeVisibleToCell(MapLayer<float> layer, int row, int col)
    {
        List<Vector2> visibles = new List<Vector2>();

        for (int r = 0; r < grid.gridSizeY; r++)
        {
            for (int c = 0; c < grid.gridSizeX; c++)
            {
                Vector2 currPos = new Vector2(c, r); // x=col, y=row

                if (grid.IsValidGridPos(currPos) && grid.IsWalkable(c, r)) // IsWalkable takes (x, y) = (col, row)
                {
                    layer.SetValue(r, c, 0.0f);

                    if (IsClearPath(r, c, row, col))
                    {
                        layer.SetValue(r, c, 1.0f);
                        visibles.Add(currPos);
                    }
                }
            }
        }

        // Mark neighbors of visible cells as 0.5
        foreach (Vector2 v in visibles)
        {
            int c = (int)v.x; // v.x is column
            int r = (int)v.y; // v.y is row

            for (int r1 = -1; r1 <= 1; r1++)
            {
                for (int c1 = -1; c1 <= 1; c1++)
                {
                    Vector2 currNeighbor = new Vector2(c + c1, r + r1); // x=col, y=row

                    if (!grid.IsValidGridPos(currNeighbor) || 
                        !grid.IsWalkable(c + c1, r + r1) ||  // IsWalkable takes (x, y) = (col, row)
                        (row == r + r1 && col == c + c1))
                        continue;

                    if (!IsClearPath(r, c, r + r1, c + c1))
                        continue;

                    if (layer.GetValue(r + r1, c + c1) != 1.0f)
                        layer.SetValue(r + r1, c + c1, 0.5f);
                }
            }
        }
    }

    public void AnalyzeAgentVision(MapLayer<float> layer, Transform agent, float FOVDeg = 182f)
    {
        Vector2 agentPos = new Vector2(agent.position.x, agent.position.y);
        Node agentNode = grid.NodeFromWorldPoint(agentPos);
        int row = agentNode.gridY;
        int col = agentNode.gridX;

        Vector2 facing = new Vector2(agent.up.x, agent.up.y).normalized;

        float halfFOV = (FOVDeg * 0.5f) * (math.PI / 180.0f);
        float FOV = math.cos(halfFOV);

        for (int r = 0; r < grid.gridSizeY; r++)
        {
            for (int c = 0; c < grid.gridSizeX; c++)
            {
                Vector2 currPos = new Vector2(c, r); // x=col, y=row

                if (grid.IsValidGridPos(currPos) && grid.IsWalkable(c, r)) // IsWalkable takes (x, y) = (col, row)
                {
                    Node node = grid.GridGet(r, c);
                    Vector2 agentToCell = (node.worldPosition - agentPos).normalized;

                    float cos = Vector2.Dot(facing, agentToCell);

                    if (cos >= FOV && IsClearPath(r, c, row, col))
                        layer.SetValue(r, c, 1.0f);
                }
            }
        }
    }

    public void PropagateSoloOccupancy(MapLayer<float> layer, float decay, float growth)
    {
        int rows = grid.gridSizeY;
        int cols = grid.gridSizeX;

        float[,] tempLayer = new float[rows, cols];

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                Vector2 currPos = new Vector2(c, r); // x=col, y=row
                float currVal = layer.GetValue(r, c);
                float highestValue = 0.0f;

                if (grid.IsValidGridPos(currPos) && grid.IsWalkable(c, r)) // IsWalkable takes (x, y) = (col, row)
                {
                    for (int r1 = r - 1; r1 <= r + 1; r1++)
                    {
                        for (int c1 = c - 1; c1 <= c + 1; c1++)
                        {
                            if (r1 == r && c1 == c)
                                continue;

                            Vector2 otherPos = new Vector2(c1, r1); // x=col, y=row

                            if (!grid.IsValidGridPos(otherPos) || !grid.IsWalkable(c1, r1)) // IsWalkable takes (x, y) = (col, row)
                                continue;

                            if (!IsClearPath(r, c, r1, c1))
                                continue;

                            float neighborVal = layer.GetValue(r1, c1);
                            float dx = r1 - r;
                            float dy = c1 - c;
                            float dist = math.sqrt(dx * dx + dy * dy);
                            float decayVal = neighborVal * math.exp(-decay * dist);

                            if (decayVal > highestValue)
                                highestValue = decayVal;
                        }
                    }
                }

                float newVal = math.lerp(currVal, highestValue, growth);
                tempLayer[r, c] = newVal;
            }
        }

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                layer.SetValue(r, c, tempLayer[r, c]);
            }
        }
    }

    public void NormalizeSoloOccupancy(MapLayer<float> layer)
    {
        int rows = grid.gridSizeY;
        int cols = grid.gridSizeX;

        float highestValue = 0.00001f;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                float tempVal = layer.GetValue(r, c);
                if (tempVal > highestValue)
                    highestValue = tempVal;
            }
        }

        if (highestValue == 0)
            return;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                float currValue = layer.GetValue(r, c);

                if (currValue < 0)
                    continue;

                float newValue = currValue / highestValue;
                layer.SetValue(r, c, newValue);
            }
        }
    }

    public void EnemyFieldOfView(MapLayer<float> layer, float fovAngle, float closeDistance, float occupancyValue, Transform enemy)
    {
        Vector2 agentPos = new Vector2(enemy.position.x, enemy.position.y);
        Node agentNode = grid.NodeFromWorldPoint(agentPos);
        int row = agentNode.gridY;
        int col = agentNode.gridX;

        Vector2 facing = new Vector2(enemy.up.x, enemy.up.y).normalized;

        float FOVDeg = fovAngle;
        float halfFOV = (FOVDeg * 0.5f) * (math.PI / 180.0f);
        float FOV = math.cos(halfFOV);

        for (int r = 0; r < grid.gridSizeY; r++)
        {
            for (int c = 0; c < grid.gridSizeX; c++)
            {
                Vector2 currPos = new Vector2(c, r); // x=col, y=row

                if (grid.IsValidGridPos(currPos) && grid.IsWalkable(c, r)) // IsWalkable takes (x, y) = (col, row)
                {
                    if (!IsClearPath(r, c, row, col))
                        continue;

                    float currValue = layer.GetValue(r, c);
                    if (currValue < 0)
                        layer.SetValue(r, c, 0.0f);

                    float dx = math.abs(r - row);
                    float dy = math.abs(c - col);
                    float tempDist = math.sqrt(dx * dx + dy * dy);

                    if (tempDist < closeDistance)
                    {
                        layer.SetValue(r, c, occupancyValue);
                        continue;
                    }

                    Node node = grid.GridGet(r, c);
                    Vector2 agentToCell = (node.worldPosition - agentPos).normalized;

                    float cos = Vector2.Dot(facing, agentToCell);

                    if (cos >= FOV)
                        layer.SetValue(r, c, occupancyValue);
                }
            }
        }
    }

    public bool EnemyFindPlayer(MapLayer<float> layer, Transform player)
    {
        Vector2 playerWorldPos = new Vector2(player.position.x, player.position.y);
        Node playerNode = grid.NodeFromWorldPoint(playerWorldPos);

        if (grid.IsValidGridPos(new Vector2(playerNode.gridX, playerNode.gridY)))
        {
            if (layer.GetValue(playerNode.gridY, playerNode.gridX) < 0.0f)
                return true;
        }

        return false;
    }

    public bool EnemySeekPlayer(MapLayer<float> layer, Transform enemy)
    {
        int rows = grid.gridSizeY;
        int cols = grid.gridSizeX;

        float highestValue = 0.00001f;
        Vector2 highestPos = new Vector2(-1, -1);
        float bestDist = float.PositiveInfinity;

        Vector2 enemyWorldPos = new Vector2(enemy.position.x, enemy.position.y);
        Node enemyNode = grid.NodeFromWorldPoint(enemyWorldPos);
        int enemyRow = enemyNode.gridY;
        int enemyCol = enemyNode.gridX;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                float tempVal = layer.GetValue(r, c);

                if (tempVal > highestValue)
                {
                    highestValue = tempVal;
                    highestPos = new Vector2(r, c);

                    float dx = math.abs(r - enemyRow);
                    float dy = math.abs(c - enemyCol);
                    bestDist = math.sqrt(dx * dx + dy * dy);
                    continue;
                }

                if (tempVal == highestValue)
                {
                    float dx = math.abs(r - enemyRow);
                    float dy = math.abs(c - enemyCol);
                    float tempDist = math.sqrt(dx * dx + dy * dy);

                    if (tempDist < bestDist)
                    {
                        highestPos = new Vector2(r, c);
                        bestDist = tempDist;
                    }
                }
            }
        }

        if (grid.IsValidGridPos(highestPos))
        {
            Node targetNode = grid.GridGet((int)highestPos.x, (int)highestPos.y);
            // You could integrate this with your pathfinding here
            // For example: enemy.GetComponent<EnemyLogic>().AstarTarget = targetNode.worldPosition;
            return true;
        }

        return false;
    }
}
