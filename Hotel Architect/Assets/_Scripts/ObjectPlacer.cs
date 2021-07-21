using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPlacer : MonoBehaviour
{
    [SerializeField] GameObject _previewPrefab = null;

    [SerializeField] LayerMask _interactableMask;
    [SerializeField] LayerMask _placeableMask;


    [SerializeField] PlacableObject _fullWall = null;
    [SerializeField] PlacableObject _halfWall = null;
    [SerializeField] PlacableObject _sofa = null;
    [SerializeField] PlacableObject _largeSofa = null;

    PlacableObject _placeObject = null;

    GameObject _placerPreview = null;

    int _placeableLayerNum;

    Vector3 _currentPoint;

    Camera _mainCam;

    void Awake()
    {
        _placerPreview = Instantiate(_previewPrefab, Vector3.zero, Quaternion.identity, transform);
        _placeableLayerNum = LayerMask.NameToLayer("Placeable");

        //Sets placable to the full wall by default
        _placeObject = _fullWall;

        _mainCam = Camera.main;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) ChangeObject(_fullWall);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) ChangeObject(_halfWall);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) ChangeObject(_sofa);
        else if (Input.GetKeyDown(KeyCode.Alpha4)) ChangeObject(_largeSofa);
        

        SnapPreviewToGrid();

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 _buildPoint = _currentPoint;
            _buildPoint.y += _placeObject._config._sizeInMetres.y / 2.0f;

            //Check that there isnt an object occupying this space.
            if (!Physics.CheckBox(_buildPoint, Vector3.one / 3, Quaternion.identity))
                Instantiate(_placeObject, _buildPoint, Quaternion.identity, transform);
        }
        else if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            Ray ray = _mainCam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100.0f, _placeableMask))
            {
                Destroy(hit.transform.gameObject);
            }
        }
    }

    void SnapPreviewToGrid()
    {
        Vector2 screenPos = Input.mousePosition;

        Ray ray = _mainCam.ScreenPointToRay(screenPos);

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

            _currentPoint.x = Mathf.Round(_currentPoint.x);
            _currentPoint.y = 0.0f;
            _currentPoint.z = Mathf.Round(_currentPoint.z);

            //Offsets objects slightly based on how large they are.
            if(_placeObject._config._sizeInMetres.x % 2 == 0) _currentPoint.x -= 0.5f;

            _placerPreview.transform.position = _currentPoint;
        }
        else
        {
            _placerPreview.SetActive(false);
        }
    }

    public void ChangeObject(PlacableObject newObject)
    {
        _placeObject = newObject;
        _placerPreview.transform.localScale = _placeObject._config._sizeInMetres;
    }
}
