using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Objects/Base Object Config")]
public class PlacableConfig : ScriptableObject
{
    public Vector3 _sizeInMetres = Vector3.one;
    public bool _canRotate = true;

    [Header("Drag Settings")]
    public bool _canDrag = true;
    public bool _biDirectionalDrag = false;
}
