using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ObjectPlacer : MonoBehaviour
{
    [SerializeField] GameObject _previewPrefab = null;

    [SerializeField] LayerMask _interactableMask;
    [SerializeField] LayerMask _placeableMask;

    [SerializeField] Material _invalidPlacement = null;
    [SerializeField] Material _validPlacement = null;
    MeshRenderer _meshRenderer;


    [Header("Objects (TESTING PURPOSE ONLY)")]
    [SerializeField] PlacableObject _fullWall = null;
    [SerializeField] PlacableObject _halfWall = null;
    [SerializeField] PlacableObject _sofa = null;
    [SerializeField] PlacableObject _largeSofa = null;

    PlacableObject _placeObject = null;

    GameObject _placerPreview = null;

    int _placeableLayerNum;

    Vector3 _currentPoint;

    Camera _mainCam;

    bool _isRotatated = false;
    bool _canBuild = false;

    bool _dragging = false;

    NodeGrid _nodeGrid;

    Vector3 _dragStartPositon;

    Ray _rayFromCamera;
    

    void Awake()
    {
        _placerPreview = Instantiate(_previewPrefab, Vector3.zero, Quaternion.identity, transform);
        _placeableLayerNum = LayerMask.NameToLayer("Placeable");

        _meshRenderer = _placerPreview.GetComponentInChildren<MeshRenderer>();

        _nodeGrid = FindObjectOfType<NodeGrid>();

        //Sets placeable to the full wall by default
        ChangeObject(_fullWall);

        _mainCam = Camera.main;
    }

    private void Update()
    {
        _rayFromCamera = _mainCam.ScreenPointToRay(Input.mousePosition);

        InteractWithKeyboard();

        SnapPreviewToGrid();

        InteractWithMouse();
    }

    private void InteractWithKeyboard()
    {
        //Sets the placeable that can be put down
        if (Input.GetKeyDown(KeyCode.Alpha1)) ChangeObject(_fullWall);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) ChangeObject(_halfWall);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) ChangeObject(_sofa);
        else if (Input.GetKeyDown(KeyCode.Alpha4)) ChangeObject(_largeSofa);

        //Rotates the placeable
        if (Input.GetKeyDown(KeyCode.X))
        {
            //Toggles the rotated bool
            _isRotatated = !_isRotatated;
            //Handles the rotation of the preview object
            HandlePreviewRotations();
        }
    }

    void SnapPreviewToGrid()
    {
        RaycastHit hit;
        //Casts from mouse point to world
        if (Physics.Raycast(_rayFromCamera, out hit, 100.0f, _interactableMask))
        {
            _currentPoint = hit.point;

            //If the object we are hitting is a placed item, then add a slight offset to the contact point (allows quicker placement)
            if (hit.transform.gameObject.layer == _placeableLayerNum) _currentPoint += hit.normal / 2;
            else _placerPreview.SetActive(true);

            //Rounds th current point down
            _currentPoint = _currentPoint.Round();
            _currentPoint.y = 0.0f;

            //Offsets objects slightly based on how large they are. 
            if (_placeObject._config._sizeInMetres.x % 2 == 0)
            {
                _currentPoint.z -= 0.5f;

                if(!_isRotatated)
                {
                    _currentPoint.x += 0.5f;
                    _currentPoint.z -= 0.5f;
                }
            } 

            _placerPreview.transform.position = _currentPoint;
        }
        else _placerPreview.SetActive(false);
    }

    private void BuildObject(Vector3 pos)
    {
        Quaternion objectRotation = Quaternion.identity;
        if (_isRotatated) objectRotation.eulerAngles = new Vector3(0.0f, 90.0f, 0.0f);

        //Instantiates the object we are building
        Instantiate(_placeObject, pos, objectRotation, transform);

        //Sets the node as non walkable
        SetWalkable(_placeObject._config._sizeInMetres, pos, false);
        _placerPreview.transform.localScale = _placeObject._config._sizeInMetres;
    }

    private void DragPreview()
    {
        RaycastHit hit;
        if (Physics.Raycast(_rayFromCamera, out hit, 100.0f, _interactableMask))
        {
            Vector3 hitPoint = hit.point;

            float diffInX = hitPoint.x - _dragStartPositon.x;
            float diffInZ = hitPoint.z - _dragStartPositon.z;

            float absX = Mathf.Abs(diffInX);
            float absZ = Mathf.Abs(diffInZ);

            Vector3 halfwayPoint = _dragStartPositon;
            halfwayPoint.y = 0.0f;
            Vector3 scale = _placerPreview.transform.localScale;

            //Dragging along X
            if (absX > absZ)
            {
                if (diffInX > 0.0f) halfwayPoint.x = _dragStartPositon.x + (diffInX / 2.0f);
                else halfwayPoint.x = _dragStartPositon.x - (absX / 2.0f);

                halfwayPoint.x = RoundToRange(halfwayPoint.x);

                float xScale;
                if (diffInX > 0.0f) xScale = Mathf.Max(diffInX, 1.0f);
                else xScale = Mathf.Min(diffInX, -1.0f);

                _placerPreview.transform.localScale = new Vector3(xScale, scale.y, scale.z);
            }
            //Dragging along Z
            else
            {
                if (diffInZ > 0.0f) halfwayPoint.z = _dragStartPositon.z + (diffInZ / 2.0f);
                else halfwayPoint.z = _dragStartPositon.z - (absZ / 2.0f);

                halfwayPoint.z = RoundToRange(halfwayPoint.z);

                float zScale;
                if (diffInZ > 0.0f) zScale = Mathf.Max(diffInZ, 1.0f);
                else zScale = Mathf.Min(diffInZ, -1.0f);

                _placerPreview.transform.localScale = new Vector3(scale.x, scale.y, zScale);
            }

            _placerPreview.transform.position = halfwayPoint;
        }
    }

    private void RMBLogic()
    {
        if (_dragging)
        {
            _dragging = false;
            _placerPreview.transform.localScale = _placeObject._config._sizeInMetres;
        }
        else
        {
            //Perform a raycast from the mouse position to the the ground 
            RaycastHit hit;
            if (Physics.Raycast(_rayFromCamera, out hit, 100.0f, _placeableMask))
            {
                PlacableObject objectToDestroy = hit.transform.GetComponentInParent<PlacableObject>();
                if (objectToDestroy)
                {
                    //Sets that node to be walkable. 
                    SetWalkable(objectToDestroy._config._sizeInMetres, hit.transform.position, true);

                    //Destroy the object at that point
                    Destroy(hit.transform.gameObject);
                }
            }
        }
    }

    private void InteractWithMouse()
    {
        //Gets the build point, this is needed as some objects have offsets to them
        Vector3 buildPoint = _currentPoint;
        buildPoint.y += _placeObject._config._sizeInMetres.y / 2.0f;

        Vector3 extents = _placeObject._config._sizeInMetres / 2.0f;
        //Subtracts a small amount of the extents to avoid overlapping colliders
        extents = extents.Subtract(0.02f);

        //If the object is rotated swap the extents for the X and Z axis
        if (_isRotatated)
        {
            float temp = extents.x;
            extents.x = extents.z;
            extents.z = temp;
        }

        //we can build if there is no collision
        _canBuild = !Physics.CheckBox(buildPoint, extents);

        if (_dragging) DragPreview();

        if (_canBuild)
        {
            //Sets the material of the placer preview
            _meshRenderer.material = _validPlacement;

            //If we LMB click 
            if (Input.GetMouseButtonDown(0))
            {
                if (_placeObject._config._canDrag)
                {
                    _dragging = !_dragging;

                    if(_dragging) _dragStartPositon = buildPoint;
                    //Handle placement logic
                    if (!_dragging)
                    {
                        RaycastHit hit;
                        if(Physics.Raycast(_rayFromCamera, out hit, 100.0f, _interactableMask))
                        {
                            Vector3 bp = _dragStartPositon;
                            bp.Floor();
                            Vector3 hp = hit.point;
                            hp.Floor();

                            //Cover edge case where the players wants to place an object on the same point they started on
                            if (bp.x == hp.x && bp.z == hp.z) BuildObject(_dragStartPositon);
                            else
                            {
                                Vector3 groundPoint = hit.point;

                                //Maybe this needs to be the absolute value
                                float diffInX = groundPoint.x - _dragStartPositon.x;
                                float diffInZ = groundPoint.z - _dragStartPositon.z;
                                diffInX = Mathf.Round(diffInX);
                                diffInZ = Mathf.Round(diffInZ);

                                float absX = Mathf.Abs(diffInX);
                                float absZ = Mathf.Abs(diffInZ);

                                float incremeneter = 1.0f;

                                if (absX > absZ)
                                {
                                    if (diffInX < 0) incremeneter = -1.0f;

                                    for (int i = 0; i < absX; i++)
                                    {
                                        BuildObject(_dragStartPositon);

                                        _dragStartPositon.x += incremeneter;
                                    }
                                }
                                else
                                {
                                    if (diffInZ < 0) incremeneter = -1.0f;

                                    for (int i = 0; i < absZ; i++)
                                    {
                                        BuildObject(_dragStartPositon);

                                        _dragStartPositon.z += incremeneter;
                                    }
                                }

                                BuildObject(_dragStartPositon);
                            }
                        }
                    }
                }
                else
                {
                    BuildObject(buildPoint);
                }
            }
        }
        //if we cannot build then set the placer previews material to red.
        else _meshRenderer.material = _invalidPlacement;

        //if the RMB is clicked
        if (Input.GetMouseButtonDown(1))
            RMBLogic();
    }

    float RoundToRange(float inVal)
    {
        float flooredVal = Mathf.Floor(inVal);
        float newVal = inVal - flooredVal;

        if (newVal > 0.75f) newVal = 1.0f;
        else if (newVal > 0.25f) newVal = 0.5f;
        else newVal = 0.0f;

        newVal += flooredVal;

        return newVal;
    }

    void SetWalkable(Vector3 size, Vector3 pos, bool toggle)
    {
        pos.x += 0.2f;
        pos.z += 0.2f;

        _nodeGrid.GetNodeFromPosition(pos)._walkable = toggle;

        if (size.x > 1)
        {
            //Handle rotated objects here as well
            if (_isRotatated)
            {
                if (size.x == 2.0f)
                {
                    _nodeGrid.GetNodeFromPosition(new Vector3(pos.x, pos.y, pos.z + 1.0f))._walkable = toggle;
                }
                else if (size.x == 3.0f)
                {
                    _nodeGrid.GetNodeFromPosition(new Vector3(pos.x, pos.y, pos.z - 1.0f))._walkable = toggle;
                    _nodeGrid.GetNodeFromPosition(new Vector3(pos.x, pos.y, pos.z + 1.0f))._walkable = toggle;
                }
            }
            else
            {
                if (size.x == 2.0f)
                {
                    _nodeGrid.GetNodeFromPosition(new Vector3(pos.x + 1.0f, pos.y, pos.z))._walkable = toggle;
                }
                else if (size.x == 3.0f)
                {
                    _nodeGrid.GetNodeFromPosition(new Vector3(pos.x - 1.0f, pos.y, pos.z))._walkable = toggle;
                    _nodeGrid.GetNodeFromPosition(new Vector3(pos.x + 1.0f, pos.y, pos.z))._walkable = toggle;
                }
            }
        }
    }

    private void HandlePreviewRotations()
    {
        //Rotates on y by 90 degrees
        if (_isRotatated) _placerPreview.transform.localEulerAngles = new Vector3(0.0f, 90.0f, 0.0f);
        else _placerPreview.transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
    }

    public void ChangeObject(PlacableObject newObject)
    {
        //Sets the new object
        _placeObject = newObject;
        //Sets the previews scale 
        _placerPreview.transform.localScale = _placeObject._config._sizeInMetres;
        //No longer rotating
        _isRotatated = false;
        _dragging = false;
        //Rotate the preview back. 
        HandlePreviewRotations();
    }
}
