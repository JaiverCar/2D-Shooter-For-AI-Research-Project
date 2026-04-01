using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class Node
{
    public bool walkable;
    public Vector2 worldPosition;
    public int gridX, gridY;

    public int gCost, hCost;
    public Node parent;
    public int fCost => gCost + hCost;

    public Node(bool walkable, Vector2 worldPos, int gridX, int gridY)
    {
        this.walkable = walkable;
        this.worldPosition = worldPos;
        this.gridX = gridX;
        this.gridY = gridY;
    }
}

public class AStarGrid : MonoBehaviour
{
    public static AStarGrid Instance;

    [Header("Grid Settings")]
    public Vector2 gridWorldSize = new Vector2(20, 20); // how much of the world to cover
    public float nodeRadius = 0.5f;
    public LayerMask obstacleLayer;

    Node[,] grid;
    float nodeDiameter;
    int gridSizeX, gridSizeY;

    void Awake()
    {
        Instance = this;
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        BuildGrid();
    }

    void BuildGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];
        Vector2 bottomLeft = (Vector2)transform.position
                           - Vector2.right * gridWorldSize.x / 2
                           - Vector2.up * gridWorldSize.y / 2;

        for (int x = 0; x < gridSizeX; x++)
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector2 worldPoint = bottomLeft
                    + Vector2.right * (x * nodeDiameter + nodeRadius)
                    + Vector2.up * (y * nodeDiameter + nodeRadius);

                bool walkable = Physics2D.OverlapCircle(worldPoint, nodeRadius * 0.9f, obstacleLayer) == null;
                grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
    }

    public Node NodeFromWorldPoint(Vector2 worldPos)
    {
        float percentX = Mathf.Clamp01((worldPos.x - (transform.position.x - gridWorldSize.x / 2)) / gridWorldSize.x);
        float percentY = Mathf.Clamp01((worldPos.y - (transform.position.y - gridWorldSize.y / 2)) / gridWorldSize.y);
        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        return grid[x, y];
    }

    public List<Node> GetNeighbors(Node node)
    {
        var neighbors = new List<Node>();
        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                int cx = node.gridX + dx, cy = node.gridY + dy;
                if (cx >= 0 && cx < gridSizeX && cy >= 0 && cy < gridSizeY)
                    neighbors.Add(grid[cx, cy]);
            }
        return neighbors;
    }

    // Visualize in editor
    void OnDrawGizmos()
    {
        // Always draw the boundary
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, gridWorldSize.y, 1));

        // Build a preview grid in edit mode
        float nodeDiameter = nodeRadius * 2;
        int sizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        int sizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        Vector2 bottomLeft = (Vector2)transform.position
                           - Vector2.right * gridWorldSize.x / 2
                           - Vector2.up * gridWorldSize.y / 2;

        for (int x = 0; x < sizeX; x++)
            for (int y = 0; y < sizeY; y++)
            {
                Vector2 worldPoint = bottomLeft
                    + Vector2.right * (x * nodeDiameter + nodeRadius)
                    + Vector2.up * (y * nodeDiameter + nodeRadius);

                bool walkable = Physics2D.OverlapCircle(worldPoint, nodeRadius * 0.9f, obstacleLayer) == null;

                Gizmos.color = walkable
                    ? new Color(0f, 1f, 0f, 0.15f)
                    : new Color(1f, 0f, 0f, 0.5f);

                Gizmos.DrawCube(worldPoint, Vector2.one * (nodeDiameter - 0.05f));
            }
        }
    }