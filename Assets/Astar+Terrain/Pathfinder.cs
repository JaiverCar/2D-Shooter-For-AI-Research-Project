using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Grid = AStarGrid;

public class Pathfinder : MonoBehaviour
{
    // Instance for singleton
    public static Pathfinder Instance;

    void Awake()
    {
        Instance = this;
    }

    // vars for astar search
    int toSearchCount;
    int currIteration = 1;
    static int GRID_MAX_NODES = 800 * 800;
    Node[] openSet = new Node[GRID_MAX_NODES];
    float currHeuristic = 0.0f;

    // Finds an Astar path between two positions
    // Params:
    // startPos - starting position
    // targetPos - goal position
    // weight - heuristic weight
    // stpuidity - enemies stupidity value (max is 10)
    // rubberbandOn - if the path should be rubberbanded
    // smoothOn - if the path should be smoothed
    // Returns: the optimal path from startPos to targetPos
    public List<Vector2> FindPath(Vector2 startPos, Vector2 targetPos, float weight = 1.0f, float stupidity = 0.0f, bool rubberbandOn = true, bool smoothOn = true)
    {
        Node startNode = Grid.Instance.NodeFromWorldPoint(startPos);
        Node goalNode = Grid.Instance.NodeFromWorldPoint(targetPos);

        currHeuristic = CalculateHeuristic(startNode.pos, goalNode.pos, weight, stupidity);
        currIteration++;

        startNode.gCost = 0.0f;
        startNode.hCost = currHeuristic;
        startNode.parent = null;
        startNode.iteration = currIteration;
        startNode.status = Node.SearchStatus.onOpen;


        toSearchCount = 1;


        if (!startNode.walkable || !goalNode.walkable)
        {
            return null;
        }

        openSet[0] = startNode;

        // Dumber enemies ignore path-avoidance costs from other enemies' trails
        float extraCostScale = 1f - Mathf.Clamp01(stupidity / 10f);

        var path = new List<Vector2> {};

        while (toSearchCount > 0)
        {
            // Pick lowest fCost node
            int bestIndex = 0;

            for (int i = 1; i < toSearchCount; i++)
            {
                if (openSet[i].IsLowerF(openSet[bestIndex]))
                {
                    bestIndex = i;
                }
            }

            //pop best node
            Node curr = openSet[bestIndex];
            
            if(bestIndex != toSearchCount - 1)
            {
                openSet[bestIndex] = openSet[toSearchCount - 1];
            }
            toSearchCount--;

            curr.status = Node.SearchStatus.onClosed;

            // if it is the goal node, start returning path
            if (curr.pos == goalNode.pos)
            {
                List<Vector2> reversedPath = new List<Vector2>();
                Node node = curr;

                while (node != null)
                {
                    node.extraCost += 0.5f;
                    reversedPath.Add(node.worldPosition);
                    node = node.parent;
                }

                if(rubberbandOn)
                {
                    Rubberband(reversedPath);

                    //if both rubberband and smooth
                    if(smoothOn)
                    {
                        RubberAndSmooth(reversedPath);
                    }
                }

                if(smoothOn)
                {
                    Smooth(reversedPath, 3);
                }

                reversedPath.Reverse(); 
                return reversedPath;
            }

            foreach (var neighbor in curr.neighbors)
            {
                if(!Grid.Instance.IsValidGridPos(neighbor.pos) || !Grid.Instance.IsWalkable((int)neighbor.pos.x, (int)neighbor.pos.y))
                {
                    continue;
                }

                float dx = neighbor.pos.x - curr.pos.x;
                float dy = neighbor.pos.y - curr.pos.y;

                if (math.abs(dx) == 1 && math.abs(dy) == 1)
                {
                    // check the two cells that share an edge with this diagonal move
                    Vector2 checkA = new Vector2(curr.pos.x + dx, curr.pos.y);      // same column, adjacent row
                    Vector2 checkB = new Vector2(curr.pos.x, curr.pos.y + dy); // same row, adjacent column

                    if (!Grid.Instance.IsValidGridPos(checkA) || !Grid.Instance.IsValidGridPos(checkB))
                        continue;

                    if (!Grid.Instance.IsWalkable((int)checkA.x, (int)checkA.y)
                        || !Grid.Instance.IsWalkable((int)checkB.x, (int)checkB.y))
                        continue;
                }

                //calculate new G cost (include extraCost for path avoidance)
                float newG = curr.gCost + neighbor.cost + Grid.Instance.GridGet(neighbor.pos).extraCost * extraCostScale;
                float hNeighbor = CalculateHeuristic(neighbor.pos, goalNode.pos, weight, stupidity);

                Node node = Grid.Instance.GridGet(neighbor.pos);


                //if neighbor wasnt explored in this iteration, initialize and open
                if (node.iteration != currIteration)
                {
                    node.gCost = newG;
                    node.hCost = hNeighbor;
                    node.parent = curr;
                    node.iteration = currIteration;
                    node.status = Node.SearchStatus.onOpen;

                    if (toSearchCount < GRID_MAX_NODES)
                    {
                        openSet[toSearchCount++] = node;
                    }

                }
                else if (node.status == Node.SearchStatus.onClosed)
                {
                    if (newG < node.gCost)
                    {
                        node.gCost = newG;
                        node.hCost = hNeighbor;
                        node.parent = curr;
                        node.status = Node.SearchStatus.onOpen;

                        if (toSearchCount < GRID_MAX_NODES)
                        {
                            openSet[toSearchCount++] = node;
                        }
                    }
                }
                else if (node.status == Node.SearchStatus.onOpen)
                {
                    if (newG < node.gCost)
                    {
                        node.gCost = newG;
                        node.hCost = hNeighbor;
                        node.parent = curr;
                        node.iteration = currIteration;
                    }
                }
            }
        }

        return path;
    }

    // Calculates the Heuristic value between two nodes
    // Params:
    // from - first node
    // goal - second node
    // weight - extra weight to multiply agains node costs
    // stupidity - the enemies current stupidity value 
    // Returns: new heurstic value for a node
    public float CalculateHeuristic(Vector2 from, Vector2 goal, float weight = 1.0f, float stupidity = 0.0f)
    {
        //distance between nodes (using manhattan distance)
        int dx = (int)math.abs(from.x - goal.x);
        int dy = (int)math.abs(from.y - goal.y);
        float newH = math.sqrt((dx * dx + dy * dy));
        
        //Depending on how the enemies stupidity value, adds random noise to nodes cost
        newH += UnityEngine.Random.Range(0.0f, stupidity);

        return newH * weight;
    }

    // Rubberbands the path (removes unecessary nodes)
    // Params: reversedPath - the current Astar path
    void Rubberband(List<Vector2> reversedPath)
    {
        // dont update if empty
        if (reversedPath.Count == 0)
            return;

        // remove every middle point if it can be rubberbanded
        int iStart = reversedPath.Count - 1;
        for (int i = iStart; i >= 2; --i)
        {
            bool removeMid = true;
            Node p1 = Grid.Instance.NodeFromWorldPoint(reversedPath[i]);
            Node p2 = Grid.Instance.NodeFromWorldPoint(reversedPath[i - 2]);

            int minX = math.min(p1.gridX, p2.gridX);
            int maxX = math.max(p1.gridX, p2.gridX);

            int minY = math.min(p1.gridY, p2.gridY);
            int maxY = math.max(p1.gridY, p2.gridY);

            for (int r = minX; r <= maxX && removeMid; ++r)
            {
                for (int c = minY; c <= maxY; ++c)
                {
                    Vector2 currPos = new Vector2(r, c);
                    if (!Grid.Instance.IsValidGridPos(currPos) || !Grid.Instance.IsWalkable(r, c))
                    {
                        removeMid = false;
                        break;
                    }
                }
            }

            if (removeMid)
            {
                reversedPath.Remove(reversedPath[i - 1]);
            }
        }

    }

    // Catmull Rom calculation for smoothing points
    // Params:
    // p1, p2, p3, p4 - the 4 points to smooth between
    // t - the distance between the points to add in new point
    // Returns: new smoothed point
    Vector2 CatmullRom(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, float t)
    {
        Vector2 outputPoint =
        p1 * (-0.5f*t*t*t + t*t - 0.5f*t) +
        p2 * (1.5f*t*t*t - 2.5f*t*t + 1.0f) +
        p3 * (-1.5f*t*t*t + 2.0f*t*t + 0.5f*t) +
        p4 * (0.5f*t*t*t - 0.5f*t*t);

        return outputPoint;
    }

    // Smooths an Astar path
    // Params: reversedPath - the current Astar path
    // Params: extraPoints - how many points to add between each points
    void Smooth(List<Vector2> reversedPath, int extraPoints = 3)
    {
        if (reversedPath.Count < 3 || extraPoints <= 0)
        {
            return;
        }

        List<Vector2> newPath = new List<Vector2>();
        newPath.Add(reversedPath[0]);

        //add x-points in between each point
        int n = reversedPath.Count;
        for(int i = 0; i <= n - 2; ++i)
        {
            Vector2 p1;
            Vector2 p2;
            Vector2 p3;
            Vector2 p4;

            //special rule for first in list
            if (i == 0)
            {
                p1 = reversedPath[0];
                p2 = reversedPath[0];
                p3 = reversedPath[1];
                p4 = reversedPath[2];
            }
            //special rule for last in list
            else if (i == n - 2)
            {
                p1 = reversedPath[n - 3];
                p2 = reversedPath[n - 2];
                p3 = reversedPath[n - 1];
                p4 = reversedPath[n - 1];
            }
            //rest of the normal points
            else
            {
                p1 = reversedPath[i - 1];
                p2 = reversedPath[i];
                p3 = reversedPath[i + 1];
                p4 = reversedPath[i + 2];
            }

            //add in the points
            for (int j = 1; j <= extraPoints; ++j)
            {
                float t = (float)j / (float)(extraPoints + 1);
                Vector2 newPoint = CatmullRom(p1, p2, p3, p4, t);
                newPath.Add(newPoint);
            }
            newPath.Add(p3);
        }

        reversedPath.Clear();
        reversedPath.AddRange(newPath);
    }

    // Algorithm for smoothing paths that have been rubberbanded
    // Params: reversedPath - current AStar path
    void RubberAndSmooth(List<Vector2> reversedPath)
    {
        List<Vector2> newPath = reversedPath;

        if (newPath.Count < 2)
        {
            return;
        }

        bool complete = false;

        while (!complete)
        {
            complete = true;

            int iStart = newPath.Count - 1;
            for (int i = iStart; i >= 1; --i)
            {
                Vector2 checkDist = newPath[i] - newPath[i - 1];
                checkDist.x = checkDist.x * (Grid.Instance.gridSizeX / 100.0f);
                checkDist.y = checkDist.y * (Grid.Instance.gridSizeY / 100.0f);
                float dist = checkDist.magnitude / 2.0f;
                if (dist >= 1.5f)
                {
                    Vector2 midDist = (newPath[i - 1] + newPath[i]) * 0.5f;
                    newPath.Insert(i, midDist);
                    complete = false;
                    break;
                }
            }
        }
        reversedPath = newPath;
    }
}



