using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraController : MonoBehaviour
{
    [SerializeField] float _rotationSpeed = 15.0f;
    [SerializeField] float _movementSpeed = 5.0f;
    [SerializeField] float _zoomInSpeed = 3.0f;
    [SerializeField] float _pitchingSpeed = 15.0f;


    Cinemachine.CinemachineVirtualCamera _virtualCamera;
    Cinemachine.CinemachineFramingTransposer _transponder;

    void Awake()
    {
        _virtualCamera = FindObjectOfType<Cinemachine.CinemachineVirtualCamera>();
        _transponder = _virtualCamera.GetCinemachineComponent<Cinemachine.CinemachineFramingTransposer>();


        if(_transponder == null) Debug.LogError("_transponder is null");
        
        
    }

    void Update()
    {
        //Moving the camera
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
        if (movement != Vector3.zero) transform.Translate(movement * _movementSpeed * Time.deltaTime);

        //Rotating the Camera
        if (Input.GetKey(KeyCode.Q)) transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime);
        else if(Input.GetKey(KeyCode.E)) transform.Rotate(Vector3.up, -_rotationSpeed * Time.deltaTime);

        //Zoom in controls.
        Vector2 scroll = Input.mouseScrollDelta;
        scroll *= -1.0f;

        float camDistance = _transponder.m_CameraDistance;
        camDistance = Mathf.Clamp(camDistance + scroll.y * _zoomInSpeed * Time.deltaTime, 0.01f, 100.0f);
        _transponder.m_CameraDistance = camDistance;

        //"Pitch" into scene
        Vector3 trackedOffset = _transponder.m_TrackedObjectOffset;
        if (Input.GetKey(KeyCode.R)) trackedOffset.y = Mathf.Clamp(trackedOffset.y + -_pitchingSpeed * Time.deltaTime, 2.5f, 25.0f);
        else if (Input.GetKey(KeyCode.F)) trackedOffset.y = Mathf.Clamp(trackedOffset.y + _pitchingSpeed * Time.deltaTime, 2.5f, 25.0f);
        _transponder.m_TrackedObjectOffset = trackedOffset;



    }
}
