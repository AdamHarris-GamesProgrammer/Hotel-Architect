using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Objects/Base Object Config")]
public class PlacableConfig : ScriptableObject
{
    [SerializeField] public Vector3 _sizeInMetres = Vector3.one;
}
