using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;

public class Pathfinder : MonoBehaviour
{
    public static Pathfinder Instance;

    void Awake() => Instance = this;

    int toSearchCount;

    public List<Vector2> FindPath(Vector2 startPos, Vector2 targetPos)
    {
        Node startNode = AStarGrid.Instance.NodeFromWorldPoint(startPos);
        Node goalNode = AStarGrid.Instance.NodeFromWorldPoint(targetPos);

        int toSeatchCount = 1;


        if (!startNode.walkable || !goalNode.walkable)
        {
            return null;
        }

        var openSet = new List<Node> { startNode };

        var path = new List<Vector2> {};

        while (toSeatchCount > 0)
        {
            // Pick lowest fCost node
            int bestIndex = 0;

            for (int i = 1; i < toSeatchCount; i++)
            {
                if (openSet[i].IsLowerF(openSet[bestIndex]))
                {
                    bestIndex = i;
                }
            }

            //pop best node
            Node curr = openSet[bestIndex];
            
            if(bestIndex != toSeatchCount - 1)
            {
                openSet[bestIndex] = openSet[toSeatchCount - 1];
            }
            toSearchCount--;

            curr.status = Node.SearchStatus.onClosed;

            // if it is the goal node, start returning path
            if ( curr == goalNode)
            {
                // create reversed path vec
                List<Vector2> reversedPath = new List<Vector2> {};
                Vector2 currPos = curr.pos;
                reversedPath.Add(currPos);

                Node parent = curr.parent;

                while(parent != null)
                {
                    reversedPath.Add(parent.worldPosition);
                    parent = parent.parent;
                }

                foreach(Vector2 it in reversedPath)
                {
                    path.Add(it);
                }
            }

            foreach (var neighbor in curr.neighbors)
            {
                if(!neighbor.walkable || !AStarGrid.Instance.IsValidGridPos(neighbor.pos))
                {
                    continue;
                }

                int dr = neighbor.pos.x - curr.pos.x;
                int dc = neighbor.pos.y - curr.pos.y;

            }


        }
        List<Vector2> tempEnd = new List<Vector2>();
        return tempEnd;
    }


}



