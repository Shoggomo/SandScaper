using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    public float speedH = 2.0f;
    public float speedV = 2.0f;
    public float speedZoom = 2.0f;
    public float panSpeed = 1.0f;

    private float yaw = 0.0f;
    private float pitch = 0.0f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Rotate
        if (Input.GetMouseButton(0))
        {
            yaw += speedH * Input.GetAxis("Mouse X");
            pitch -= speedV * Input.GetAxis("Mouse Y");

            transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
        }

        // Pan
        if (Input.GetMouseButton(1))
        {
            var x = transform.position.x + panSpeed * Input.GetAxis("Mouse X");
            var y = transform.position.y;
            var z = transform.position.z + panSpeed * Input.GetAxis("Mouse Y");

            transform.position = new Vector3(x, y, z);
        }

        // Zoom
        transform.localScale -= speedZoom * Input.GetAxis("Mouse ScrollWheel") * Vector3.one;
    }
}
