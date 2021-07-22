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

    NodeGrid _nodeGrid;

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
            if (_placeObject._config._sizeInMetres.x % 2 == 0) _currentPoint.x -= 0.5f;

            _placerPreview.transform.position = _currentPoint;
        }
        else _placerPreview.SetActive(false);
    }

    private void InteractWithMouse()
    {
        //Gets the build point, this is needed as some objects have offsets to them
        Vector3 _buildPoint = _currentPoint;
        _buildPoint.y += _placeObject._config._sizeInMetres.y / 2.0f;

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
        _canBuild = !Physics.CheckBox(_buildPoint, extents);

        if (_canBuild)
        {
            //Sets the material of the placer preview
            _meshRenderer.material = _validPlacement;

            //If we LMB click 
            if (Input.GetMouseButtonDown(0))
            {
                //Store the rotation and set it to be y:90 if we are rotating the object 
                Quaternion objectRotation = Quaternion.identity;
                if (_isRotatated) objectRotation.eulerAngles = new Vector3(0.0f, 90.0f, 0.0f);

                //Instantiates the object we are building
                Instantiate(_placeObject, _buildPoint, objectRotation, transform);
                //Sets the node as non walkable
                _nodeGrid.GetNodeFromPosition(_buildPoint)._walkable = false;
            }
        }
        //if we cannot build then set the placer previews material to red.
        else _meshRenderer.material = _invalidPlacement;

        //if the RMB is clicked
        if (Input.GetMouseButtonDown(1))
        {
            //Perform a raycast from the mouse position to the the ground 
            RaycastHit hit;
            Ray ray = _mainCam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100.0f, _placeableMask))
            {
                //Destroy the object at that point
                Destroy(hit.transform.gameObject);
                //Sets that node to be walkable. 
                _nodeGrid.GetNodeFromPosition(hit.transform.position)._walkable = true;
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
        //Rotate the preview back. 
        HandlePreviewRotations();
    }
}
