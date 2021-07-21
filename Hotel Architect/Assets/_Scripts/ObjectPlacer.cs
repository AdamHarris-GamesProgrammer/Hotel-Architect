using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPlacer : MonoBehaviour
{
    [SerializeField] GameObject _previewObject = null;

    [SerializeField] LayerMask _interactableMask;
    [SerializeField] LayerMask _placeableMask;


    [SerializeField] PlacableObject _fullWall = null;
    [SerializeField] PlacableObject _halfWall = null;
    PlacableObject _placeObject = null;

    GameObject _placerPreview = null;

    int _placeableLayerNum;

    Vector3 _currentPoint;

    void Awake()
    {
        _placerPreview = Instantiate(_previewObject, Vector3.zero, Quaternion.identity, transform);
        _placeableLayerNum = LayerMask.NameToLayer("Placeable");

        //Sets placable to the full wall by default
        _placeObject = _fullWall;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _placeObject = _fullWall;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            _placeObject = _halfWall;
        }

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
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100.0f, _placeableMask))
            {
                Destroy(hit.transform.gameObject);
            }
        }
    }

    void SnapPreviewToGrid()
    {
        Vector2 screenPos = Input.mousePosition;

        Ray ray = Camera.main.ScreenPointToRay(screenPos);

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

            _placerPreview.transform.position = _currentPoint;
        }
        else
        {
            _placerPreview.SetActive(false);
        }
    }
}
