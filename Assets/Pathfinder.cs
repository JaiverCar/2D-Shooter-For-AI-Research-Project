using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

using Grid = AStarGrid;

public class Pathfinder : MonoBehaviour
{
    public static Pathfinder Instance;

    void Awake() => Instance = this;

    int toSearchCount;

    int currIteration = 1;
    static int GRID_MAX_NODES = 800 * 800;
    Node[] openSet = new Node[800 * 800];

    float currHeuristic = 0.0f;

    public List<Vector2> FindPath(Vector2 startPos, Vector2 targetPos)
    {
        Node startNode = Grid.Instance.NodeFromWorldPoint(startPos);
        Node goalNode = Grid.Instance.NodeFromWorldPoint(targetPos);

        currHeuristic = CalculateHeuristic(startNode.pos, goalNode.pos);
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
                    reversedPath.Add(node.worldPosition);
                    node = node.parent;
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

                //calculate new G cost
                float newG = curr.gCost + neighbor.cost;
                float hNeighbor = CalculateHeuristic(neighbor.pos, goalNode.pos);

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

    public float CalculateHeuristic(Vector2 from, Vector2 goal, float weight = 1)
    {
        //distance between nodes
        int dx = (int)math.abs(from.x - goal.x);
        int dy = (int)math.abs(from.y - goal.y);

        float newH = math.sqrt((dx * dx + dy * dy));
        return newH * weight;
    }


}



