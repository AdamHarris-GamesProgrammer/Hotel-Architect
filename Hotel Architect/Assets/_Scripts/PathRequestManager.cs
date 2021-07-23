using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PathRequestManager : MonoBehaviour
{
    Queue<PathRequest> _pathRequestQueue = new Queue<PathRequest>();
    PathRequest _currentPathRequest;

    static PathRequestManager instance;
    Pathfinding _pathfinder;
    bool _isProcessingPath;

    private void Awake()
    {
        instance = this;
        _pathfinder = GetComponent<Pathfinding>();
    }

    struct PathRequest
    {
        public Vector3 start;
        public Vector3 end;
        public Action<Vector3[], bool> callback;

        public PathRequest(Vector3 a, Vector3 b, Action<Vector3[], bool> call)
        {
            start = a;
            end = b;
            callback = call;
        }
    }

    public static void RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callback)
    {
        PathRequest newRequest = new PathRequest(pathStart, pathEnd, callback);
        instance._pathRequestQueue.Enqueue(newRequest);
        instance.TryProcessNext();
    }

    public void FinishedProcessingPath(Vector3[] path, bool success)
    {
        _currentPathRequest.callback(path, success);
        _isProcessingPath = false;

        TryProcessNext();
    }

    private void TryProcessNext()
    {
        if(!_isProcessingPath && _pathRequestQueue.Count > 0)
        {
            _currentPathRequest = _pathRequestQueue.Dequeue();
            _isProcessingPath = true;
            _pathfinder.StartFindPath(_currentPathRequest.start, _currentPathRequest.end);
        }
    }
}
