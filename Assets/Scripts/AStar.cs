using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public unsafe class AStar : MonoBehaviour
{
    public enum ListType { NoList, OpenList, ClosedList };

    private int currentIteration = 0;

    public unsafe struct Node
    {
        //IntPtr parent;       // Parent
        Node[] neighbors; // An array of the nodes neighbors, 0 = N, 1 = NE, 2 = E, 3 = SE, 4 = S, 5 = SW, 6 = W, 7 = NW    
        Vector2 gridPos;    // Node's location
        float finalCost;    // Final cost f(x)
        float givenCost;    // Given cost g(x)
        ListType onList;    // Is it on the open or closed list?
        int iteration;

        public Node(Node[] neighbors, Vector2 gridPos, float finalCost, float givenCost, ListType onList, int iteration)
        {
            this.neighbors = neighbors;
            //parent = null;
            this.gridPos = gridPos;
            this.finalCost = finalCost;
            this.givenCost = givenCost;
            this.onList = onList;
            this.iteration = iteration;
        }
    };

    Node emptyNode;

    const float SQRT2 = 1.414213562f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
