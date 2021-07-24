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
        //Gets a ray from the camera to the screen based on mouse position
        Ray ray = _mainCam.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        //Casts from mouse point to world
        if (Physics.Raycast(ray, out hit, 100.0f, _interactableMask))
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
                //_currentPoint.x -= 1.0f;
                _currentPoint.z -= 0.5f;

                if(_isRotatated)
                {
                    //_currentPoint.x -= 1.0f;
                }
                else
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
        Vector3 nodePos = pos;

        nodePos.x += 0.2f;
        nodePos.z += 0.2f;
        _nodeGrid.GetNodeFromPosition(nodePos)._walkable = false;

        Vector3 size = _placeObject._config._sizeInMetres;
        SetWalkable(size, nodePos, false);

        _placerPreview.transform.localScale = size;
    }

    private void InteractWithMouse()
    {
        //Gets the build point, this is needed as some objects have offsets to them
        Vector3 buildPoint = _currentPoint;
        buildPoint.y += _placeObject._config._sizeInMetres.y / 2.0f;

        Vector3 extents = _placeObject._config._sizeInMetres / 2.0f;
        //Subtracts a small amount of the extents to avoid overlappint colliders
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

        if (_dragging)
        {
            Ray ray = _mainCam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100.0f, _interactableMask))
            {
                Vector3 hitPoint = hit.point;

                float diffInX = hitPoint.x - _dragStartPositon.x;
                float diffInZ = hitPoint.z - _dragStartPositon.z;

                float absX = Mathf.Abs(diffInX);
                float absZ = Mathf.Abs(diffInZ);

                //Dragging along X
                if (absX > absZ)
                {
                    Vector3 halfwayPoint = _dragStartPositon;
                    //Moves the position to the center of the drag start and drag end.
                    //halfwayPoint.x -= diffInX / 2.0f;
                    halfwayPoint.y = 0.0f;
                    halfwayPoint.x = _dragStartPositon.x + (diffInX / 2.0f);
                    //halfwayPoint.x = Mathf.Round(halfwayPoint.x);


                    float flooredVal = Mathf.Floor(halfwayPoint.x);
                    float newVal = halfwayPoint.x - flooredVal;

                    if(newVal > 0.75f) newVal = 1.0f;
                    else if(newVal > 0.25f) newVal = 0.5f;
                    else newVal = 0.0f;

                    newVal += flooredVal;
                    halfwayPoint.x = newVal;

                    _placerPreview.transform.position = halfwayPoint;



                    Vector3 scale = _placerPreview.transform.localScale;
                    float xScale = Mathf.Max(diffInX, 1.0f);

                    absX = Mathf.Round(absX);
                    _placerPreview.transform.localScale = new Vector3(xScale, scale.y, scale.z);
                }
                //Dragging along Z
                else
                {

                }
            }


        }

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
                        Ray ray = _mainCam.ScreenPointToRay(Input.mousePosition);
                        RaycastHit hit;
                        if(Physics.Raycast(ray, out hit, 100.0f, _interactableMask))
                        {
                            Vector3 bp = _dragStartPositon;
                            bp.x = Mathf.Floor(bp.x);
                            bp.z = Mathf.Floor(bp.z);
                            Vector3 hp = hit.point;
                            hp.x = Mathf.Floor(hp.x);
                            hp.z = Mathf.Floor(hp.z);

                            if (bp.x == hp.x && bp.z == hp.z)
                            {
                                BuildObject(_dragStartPositon);
                            }
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

                                if (absX > absZ)
                                {
                                    float incremeneter = 1.0f;

                                    if (diffInX < 0) incremeneter = -1.0f;

                                    for (int i = 0; i < absX; i++)
                                    {
                                        BuildObject(_dragStartPositon);

                                        _dragStartPositon.x += incremeneter;
                                    }

                                    BuildObject(_dragStartPositon);
                                }
                                else
                                {
                                    float incremeneter = 1.0f;

                                    if (diffInZ < 0) incremeneter = -1.0f;

                                    for (int i = 0; i < absZ; i++)
                                    {
                                        BuildObject(_dragStartPositon);

                                        _dragStartPositon.z += incremeneter;
                                    }

                                    BuildObject(_dragStartPositon);
                                }
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
        {
            if (_dragging)
            {
                _dragging = false;
            }
            else
            {
                //Perform a raycast from the mouse position to the the ground 
                RaycastHit hit;
                Ray ray = _mainCam.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, 100.0f, _placeableMask))
                {
                    PlacableObject objectToDestroy = hit.transform.GetComponentInParent<PlacableObject>();
                    if (objectToDestroy)
                    {
                        //Sets that node to be walkable. 
                        Vector3 deletePosition = hit.transform.position;
                        deletePosition.x += 0.2f;
                        deletePosition.z += 0.2f;
                        _nodeGrid.GetNodeFromPosition(deletePosition)._walkable = true;

                        //This shouldnt be the placed object this should be the object to destroyed.
                        Vector3 size = objectToDestroy._config._sizeInMetres;
                        SetWalkable(size, deletePosition, true);

                        //Destroy the object at that point
                        Destroy(hit.transform.gameObject);
                    }
                }
            }
        }
    }

    void SetWalkable(Vector3 size, Vector3 pos, bool toggle)
    {
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
