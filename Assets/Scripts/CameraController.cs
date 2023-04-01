using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    public float speedH = 2.0f;
    public float speedV = 2.0f;
    public float speedZoom = 2.0f;

    private float yaw = 0.0f;
    private float pitch = 0.0f;
    private Vector3 mouseWorldPosStart;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;

    void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;

        pitch = transform.eulerAngles.x;
        yaw = transform.eulerAngles.y;
    }

    // Update is called once per frame
    void Update()
    {
        // Rotate
        if (!Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButton(2))
        {
            yaw += speedH * Input.GetAxis("Mouse X");
            pitch -= speedV * Input.GetAxis("Mouse Y");

            transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
        }

        // Pan
        if (Input.GetKey(KeyCode.LeftShift))
        {
            var mousePos = Input.mousePosition;
            mousePos.z = Camera.main.transform.localPosition.y;

            if (Input.GetMouseButtonDown(2))
            {
                mouseWorldPosStart = Camera.main.ScreenToWorldPoint(mousePos);
            }
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButton(2))
            {
                var mouseWorldPosDiff = mouseWorldPosStart - Camera.main.ScreenToWorldPoint(mousePos);
                transform.position += mouseWorldPosDiff;
            }
        }

        // Zoom
        transform.localScale -= speedZoom * Input.GetAxis("Mouse ScrollWheel") * Vector3.one;

        // Reset Camera
        if (Input.GetKeyDown(KeyCode.R))
        {
            transform.SetPositionAndRotation(originalPosition, originalRotation);
            transform.localScale = originalScale;
            pitch = originalRotation.eulerAngles.x;
            yaw = originalRotation.eulerAngles.y;
        }
    }
}
