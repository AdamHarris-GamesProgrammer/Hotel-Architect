using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node 
{
    public bool _walkable = true;
    public Vector3 _nodeWorldPosition;

    public int gridX;
    public int gridY;

    public int gCost;
    public int hCost;

    public Node parent;

    public Node(bool walkable, Vector3 worldPos, int x, int y)
    {
        _walkable = walkable;
        _nodeWorldPosition = worldPos;
        gridX = x;
        gridY = y;
    }

    public int fCost { get { return gCost + hCost; } }
}
