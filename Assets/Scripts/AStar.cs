using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class AStar : MonoBehaviour
{
    // the types of list a node can be on
    private enum ListType { NoList, OpenList, ClosedList };

    // int used to track the total number of A-star requests recieved, used to avoid reseting nodes
    private int currentIteration = 0;

    private class Node
    {
        Node parent;      // Parent ref
        Node[] neighbors; // An array of the nodes neighbors, 0 = N, 1 = NE, 2 = E, 3 = SE, 4 = S, 5 = SW, 6 = W, 7 = NW    
        Vector2 gridPos;  // Node's location
        float finalCost;  // Final cost f(x)
        float givenCost;  // Given cost g(x)
        ListType onList;  // Is it on the open or closed list?
        int iteration;    // local iteration value

        public Node(int iteration)
        {
            Debug.Assert(iteration == 0, "Error: Node class only accepts 0 in it's constructor");

            parent = null;
            neighbors = new Node[8];
            gridPos = Vector2.zero;
            finalCost = 0.0f;
            givenCost = 0.0f;
            onList = ListType.NoList;
            this.iteration = iteration;
        }
    };

    Node emptyNode;

    const float SQRT2 = 1.414213562f;

    // ------ Bucket stuff
    const int listsSize = 71;
    const float bucketSize = 3.995f;
    private struct NodeList
    {
        Node[] openList;
        int currentSize;
        int currentIndex;

        public NodeList(int currentSize)
        {
            Debug.Assert(currentSize == 0, "Error: NodeList struct only accepts 0 in it's constructor");
            
            openList = new Node[listsSize];
            this.currentSize = currentSize;
            currentIndex = -1;
        }
    };

    // size of the open list
    const int listsContainerSize = 600; 
    NodeList[] openLists = new NodeList[listsContainerSize];
    int totalSize = 0;

    // Used to track the current cheapest bucket:
    int cheapestBucketIndex = 0;
    NodeList cheapestBucket;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
