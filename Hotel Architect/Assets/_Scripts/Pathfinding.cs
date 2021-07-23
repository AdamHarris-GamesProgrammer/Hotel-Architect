using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    NodeGrid _nodes;
    PathRequestManager _requestManager;

    private void Awake()
    {
        _nodes = FindObjectOfType<NodeGrid>();
        _requestManager = GetComponent<PathRequestManager>();
    }

    IEnumerator FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Vector3[] waypoints = new Vector3[0];
        bool success = false;

        Stopwatch sw = new Stopwatch();
        sw.Start();

        Node startNode = _nodes.GetNodeFromPosition(startPos);
        Node targetNode = _nodes.GetNodeFromPosition(targetPos);

        if(startNode._walkable && targetNode._walkable)
        {
            Heap<Node> openSet = new Heap<Node>(_nodes.MaxSize);
            HashSet<Node> closedSet = new HashSet<Node>();

            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                Node currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    sw.Stop();
                    print("Path found: " + sw.ElapsedMilliseconds + "ms ");
                    success = true;
                    break;
                }

                foreach (Node neighbor in _nodes.GetNeighbours(currentNode))
                {
                    if (!neighbor._walkable || closedSet.Contains(neighbor)) continue;

                    int newMovementCost = currentNode.gCost + GetDistance(currentNode, neighbor) + neighbor.movementPenalty;
                    if (newMovementCost < neighbor.gCost || !openSet.Contains(neighbor))
                    {
                        neighbor.gCost = newMovementCost;
                        neighbor.hCost = GetDistance(neighbor, targetNode);
                        neighbor.parent = currentNode;

                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }else
                        {
                            openSet.UpdateItem(neighbor);
                        }
                    }
                }
            }
        }


        yield return null;
        if(success)
        {
            waypoints = RetracePath(startNode, targetNode);
        }
        _requestManager.FinishedProcessingPath(waypoints, success);
    }

    internal void StartFindPath(Vector3 start, Vector3 end)
    {
        StartCoroutine(FindPath(start, end));   
    }

    Vector3[] RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while(currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        Vector3[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);
        return waypoints;

    }

    Vector3[] SimplifyPath(List<Node> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;

        for(int i = 1; i < path.Count; i++)
        {
            Vector2 directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);
            if(directionNew != directionOld)
            {
                waypoints.Add(path[i]._nodeWorldPosition);
                directionOld = directionNew;
            }
        }

        return waypoints.ToArray();
    }

    int GetDistance(Node a, Node b)
    {
        int dstX = Mathf.Abs(a.gridX - b.gridX);
        int dstY = Mathf.Abs(a.gridY - b.gridY);

        if(dstX > dstY)
        {
            return 14 * dstY + 10 * (dstX - dstY);
        }else
        {
            return 14 * dstX + 10 * (dstY - dstX);
        }
    }
}
