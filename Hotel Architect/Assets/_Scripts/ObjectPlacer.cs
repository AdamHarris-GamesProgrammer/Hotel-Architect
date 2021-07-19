using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPlacer : MonoBehaviour
{
    [SerializeField] GameObject _previewObject = null;
    [SerializeField] GameObject _placeObject = null;
    [SerializeField] LayerMask _interactableMask;
    [SerializeField] LayerMask _placeableMask;

    GameObject _placerPreview = null;

    int _placeableLayerNum;

    Vector3 _currentPoint;

    void Awake()
    {
        _placerPreview = Instantiate(_previewObject, Vector3.zero, Quaternion.identity, transform);
        _placeableLayerNum = LayerMask.NameToLayer("Placeable");
        Debug.Log(_placeableLayerNum);
    }

    private void Update()
    {
        SnapPreviewToGrid();

        if(Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if(!Physics.CheckBox(_currentPoint, Vector3.one / 3, Quaternion.identity))
            {
                Instantiate(_placeObject, _currentPoint, Quaternion.identity, transform);
            }

            if(!Physics.Raycast(ray, out hit, 100.0f, _placeableMask))
            {
                
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
            _placerPreview.SetActive(true);

            _currentPoint = hit.point;



            if (hit.transform.gameObject.layer == _placeableLayerNum)
            {
                _currentPoint += hit.normal / 2;
            }

            _currentPoint.x = Mathf.Round(_currentPoint.x);
            _currentPoint.y = 0.5f;
            _currentPoint.z = Mathf.Round(_currentPoint.z);

            _placerPreview.transform.position = _currentPoint;
        }else
        {
            _placerPreview.SetActive(false);
        }
    }
}
