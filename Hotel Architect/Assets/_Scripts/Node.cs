using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : IHeapItem<Node>
{
    public bool _walkable = true;
    public Vector3 _nodeWorldPosition;

    public int gridX;
    public int gridY;

    public int movementPenalty = 1;

    public int gCost;
    public int hCost;

    public Node parent;
    int heapIndex;

    public Node(bool walkable, Vector3 worldPos, int x, int y, int penalty)
    {
        _walkable = walkable;
        _nodeWorldPosition = worldPos;
        gridX = x;
        gridY = y;
        movementPenalty = penalty;
    }

    public int fCost { get { return gCost + hCost; } }

    public int HeapIndex { get => heapIndex; set => heapIndex = value; }

    public int CompareTo(Node nodeToCompare)
    {
        int compare = fCost.CompareTo(nodeToCompare.fCost);
        if(compare == 0)
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }

        return -compare;
    }
}
