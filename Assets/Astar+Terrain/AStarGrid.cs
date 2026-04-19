using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Node
{
    public bool walkable;
    public Vector2 worldPosition;
    public int gridX, gridY;

    public Vector2 pos => new Vector2(gridX, gridY);

    public float gCost, hCost;
    public float extraCost = 0;
    public Node parent;
    public float fCost => gCost + hCost;

    public enum SearchStatus
    { 
        onOpen = 0,
        onClosed = 1,
        none = 2
    }

    public SearchStatus status;

    public int iteration = 1;

    public class nodeEdge
    {
        public Vector2 pos;
        public float cost;
    }


    public List<nodeEdge> neighbors = new List<nodeEdge>();

    public void getNeighbors()
    {
        int[] dirR = { -1, -1, -1, 0, 0, 1, 1, 1};
        int[] dirC = { -1, 0, 1, -1, 1, -1, 0, 1 };

        neighbors.Clear();

        for(int i = 0; i < 8; i++)
        {
            Vector2 np = pos;
            np.x += dirR[i];
            np.y += dirC[i];

            bool diagonal = (math.abs(dirR[i]) == 1 && math.abs(dirC[i]) == 1);
            float cost = diagonal ? 1.41421356237f : 1.0f;

            nodeEdge edge = new nodeEdge();
            edge.pos = np;
            edge.cost = cost;

            neighbors.Add(edge);
        }
    }


    public Node(bool walkable, Vector2 worldPos, int gridX, int gridY)
    {
        this.walkable = walkable;
        this.worldPosition = worldPos;
        this.gridX = gridX;
        this.gridY = gridY;
        this.status = SearchStatus.none;
    }


    public bool IsLowerF(Node other)
    {
        if (fCost < other.fCost)
        {
            return true;
        }

        if (fCost > other.fCost)
        {
            return false;
        }

        if(gCost < other.gCost)
        {
            return true;
        }

       return false;
    }
}

// Note: almost everything from here down was written by copilot
// we felt it would not be a good use of our time to setup the grid system by hand
// that being said, GetNeighbors was written by us 
public class AStarGrid : MonoBehaviour
{
    public static AStarGrid Instance;

    public bool drawGrid = false;

    [Header("Grid Settings")]
    public Vector2 gridWorldSize = new Vector2(20, 20); // how much of the world to cover
    public float nodeRadius = 0.5f;
    public LayerMask obstacleLayer;

    [Header("Path Cost Decay")]
    [Tooltip("How much extraCost decays per second")]
    public float costDecayRate = 0.1f;

    Node[,] grid;
    float nodeDiameter;
    public int gridSizeX, gridSizeY;

    void Awake()
    {
        Instance = this;
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        BuildGrid();
        GetAllNeighbors();
    }

    void Update()
    {
        // Decay extraCost over time for all nodes
        if (grid != null && costDecayRate > 0)
        {
            float decayAmount = costDecayRate * Time.deltaTime;

            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    if (grid[x, y].extraCost > 0)
                    {
                        grid[x, y].extraCost = Mathf.Max(0, grid[x, y].extraCost - decayAmount);
                    }
                }
            }
        }
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

                bool walkable = Physics2D.OverlapBox(worldPoint, Vector2.one * nodeRadius * 1.5f, 0f, obstacleLayer) == null;
                grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
    }

    void GetAllNeighbors()
    {
        foreach(Node n in grid)
        {
            n.getNeighbors();
        }
    }

    public bool IsValidGridPos(Vector2 pos)
    {
        if (pos.x >= gridSizeX || pos.x < 0)
        {
            return false;
        }
        if (pos.y >= gridSizeY || pos.y < 0)
        {
            return false;
        }

        return true;
    }

    public Node NodeFromWorldPoint(Vector2 worldPos)
    {
        float percentX = Mathf.Clamp01((worldPos.x - (transform.position.x - gridWorldSize.x / 2)) / gridWorldSize.x);
        float percentY = Mathf.Clamp01((worldPos.y - (transform.position.y - gridWorldSize.y / 2)) / gridWorldSize.y);
        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        return grid[x, y];
    }

    public bool IsWalkable(int x, int y)
    {
        return grid[x, y].walkable;
    }

    public Node GridGet(Vector2 pos)
    {
        return grid[(int)pos.x, (int)pos.y];
    }
    public Node GridGet(int row, int col)
    {
        return grid[col, row];
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
        if (drawGrid == true)
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
            {
                for (int y = 0; y < sizeY; y++)
                {
                    Vector2 worldPoint = bottomLeft
                        + Vector2.right * (x * nodeDiameter + nodeRadius)
                        + Vector2.up * (y * nodeDiameter + nodeRadius);

                    bool walkable = Physics2D.OverlapBox(worldPoint, Vector2.one * nodeRadius * 1.5f, 0f, obstacleLayer) == null;

                    Gizmos.color = walkable
                        ? new Color(0f, 1f, 0f, 0.15f)
                        : new Color(1f, 0f, 0f, 0.5f);

                    Gizmos.DrawCube(worldPoint, Vector2.one * (nodeDiameter - 0.05f));
                }

            }

            if (grid != null)
            {
                foreach (Node n in grid)
                {
                    if (!n.walkable) continue;
                    bool nextToWall = false;
                    for (int dx = -1; dx <= 1; dx++)
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            if (dx == 0 && dy == 0) continue;
                            int cx = n.gridX + dx;
                            int cy = n.gridY + dy;
                            if (cx >= 0 && cx < gridSizeX && cy >= 0 && cy < gridSizeY)
                                if (!grid[cx, cy].walkable)
                                    nextToWall = true;
                        }
                    if (nextToWall)
                    {
                        Gizmos.color = Color.cyan;
                        Gizmos.DrawSphere(n.worldPosition, 0.1f);
                    }
                }
            }
        }
    }
}