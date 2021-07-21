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

    void Awake()
    {
        _placerPreview = Instantiate(_previewPrefab, Vector3.zero, Quaternion.identity, transform);
        _placeableLayerNum = LayerMask.NameToLayer("Placeable");

        _meshRenderer = _placerPreview.GetComponentInChildren<MeshRenderer>();

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
            _isRotatated = !_isRotatated;
            HandlePreviewRotations();
        }
    }

    void SnapPreviewToGrid()
    {
        Ray ray = _mainCam.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100.0f, _interactableMask))
        {
            _currentPoint = hit.point;

            if (hit.transform.gameObject.layer == _placeableLayerNum)
            {
                _currentPoint += hit.normal / 2;
                _placerPreview.SetActive(false);
            }
            else _placerPreview.SetActive(true);

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
        Vector3 _buildPoint = _currentPoint;
        _buildPoint.y += _placeObject._config._sizeInMetres.y / 2.0f;

        Vector3 extents = _placeObject._config._sizeInMetres / 2.0f;
        extents = extents.Subtract(0.02f);
        

        if (_isRotatated)
        {
            float temp = extents.x;
            extents.x = extents.z;
            extents.z = temp;
        }

        _canBuild = !Physics.CheckBox(_buildPoint, extents);

        if (_canBuild)
        {
            _meshRenderer.material = _validPlacement;

            if (Input.GetMouseButtonDown(0))
            {
                Quaternion objectRotation = Quaternion.identity;
                if (_isRotatated) objectRotation.eulerAngles = new Vector3(0.0f, 90.0f, 0.0f);

                Instantiate(_placeObject, _buildPoint, objectRotation, transform);
            }
        }
        else _meshRenderer.material = _invalidPlacement;


        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            Ray ray = _mainCam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100.0f, _placeableMask)) Destroy(hit.transform.gameObject);
        }
    }
    private void HandlePreviewRotations()
    {
        if (_isRotatated) _placerPreview.transform.localEulerAngles = new Vector3(0.0f, 90.0f, 0.0f);
        else _placerPreview.transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
    }

    public void ChangeObject(PlacableObject newObject)
    {
        _placeObject = newObject;
        _placerPreview.transform.localScale = _placeObject._config._sizeInMetres;
        _isRotatated = false;
        HandlePreviewRotations();
    }
}
