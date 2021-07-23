using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeGrid : MonoBehaviour
{
    public LayerMask _unwalkableMask;


    public bool _displayGizmos = false;

    public Vector2 _gridWorldSize;
    Node[,] _nodes;
    public float _nodeRadius;

    float _nodeDiamater;
    int _gridSizeX;
    int _gridSizeY;

    private void Awake()
    {
        _nodeDiamater = _nodeRadius * 2;
        _gridSizeX = Mathf.RoundToInt(_gridWorldSize.x / _nodeDiamater);
        _gridSizeY = Mathf.RoundToInt(_gridWorldSize.y / _nodeDiamater);

        CreateGrid();
    }

    public int MaxSize
    {
        get { return _gridSizeX * _gridSizeY; }
    }

    private void CreateGrid()
    {
        _nodes = new Node[_gridSizeX, _gridSizeY];

        Vector3 worldBottomLeft = transform.position - Vector3.right * _gridWorldSize.x / 2 - Vector3.forward * _gridWorldSize.y / 2;

        for(int x = 0; x < _gridSizeX; x++)
        {
            for(int y = 0; y < _gridSizeY; y++)
            {
                Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * _nodeDiamater) + Vector3.forward * (y * _nodeDiamater);

                bool walkable = !(Physics.CheckSphere(worldPoint, _nodeRadius, _unwalkableMask));

                //TODO: Implement different movement costs for different objects. 
                _nodes[x, y] = new Node(walkable, worldPoint, x, y, 1);
            }
        }
    }

    public Node GetNodeFromPosition(Vector3 worldPos)
    {
        Node node = null;

        float percentX = (worldPos.x + _gridWorldSize.x / 2) / _gridWorldSize.x;
        float percentY = (worldPos.z + _gridWorldSize.y / 2) / _gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((_gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((_gridSizeY - 1) * percentY);

        node = _nodes[x, y];

        return node;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(_gridWorldSize.x, 1f, _gridWorldSize.y));

        if(_nodes != null && _displayGizmos)
        {
            foreach(Node n in _nodes)
            {
                Gizmos.color = (n._walkable) ? Color.white : Color.red;
                Gizmos.DrawCube(n._nodeWorldPosition, Vector3.one * (_nodeDiamater - 0.1f));
            }
        }
    }

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        for(int x = -1; x <= 1; x++)
        {
            for(int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if(checkX >= 0 && checkX < _gridSizeX && checkY >= 0 && checkY < _gridSizeY)
                {
                    neighbours.Add(_nodes[checkX, checkY]);
                } 
            }
        }

        return neighbours;
    }
}
