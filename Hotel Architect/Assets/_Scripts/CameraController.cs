using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] float _rotationSpeed = 15.0f;
    [SerializeField] float _movementSpeed = 5.0f;

    void Update()
    {
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));

        if (movement != Vector3.zero)
        {
            transform.Translate(movement * _movementSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.Q))
        {
            transform.Rotate(Vector3.up, -_rotationSpeed * Time.deltaTime);
        }
        else if(Input.GetKey(KeyCode.E))
        {
            transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime);
        }

    }
}
