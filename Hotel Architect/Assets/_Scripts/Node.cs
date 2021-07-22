using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node 
{
    public bool _walkable = true;
    public Vector3 _nodeWorldPosition;

    public Node(bool walkable, Vector3 worldPos)
    {
        _walkable = walkable;
        _nodeWorldPosition = worldPos;
    }
}
