using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorHelper
{
    public static Vector3 Round(this Vector3 v)
    {
        v.x = Mathf.Round(v.x);
        v.y = Mathf.Round(v.y);
        v.z = Mathf.Round(v.z);
        return v;
    }

    public static Vector3 Floor(this Vector3 v)
    {
        v.x = Mathf.Floor(v.x);
        v.y = Mathf.Floor(v.y);
        v.z = Mathf.Floor(v.z);
        return v;
    }

    public static Vector3 Subtract(this Vector3 v, float val)
    {
        v.x -= val;
        v.y -= val;
        v.z -= val;
        return v;
    }

    public static Vector3 Add(this Vector3 v, float val)
    {
        v.x += val;
        v.y += val;
        v.z += val;
        return v;
    }

    public static Vector3 Add(this Vector3 v, Vector3 vec)
    {
        v += vec;
        return v;
    }

    public static Vector3 Subtract(this Vector3 v, Vector3 vec)
    {
        v -= vec;
        return v;
    }
}
