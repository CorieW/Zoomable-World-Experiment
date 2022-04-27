using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [Header("Camera Borders")]
    [SerializeField]
    private Vector3 _borderPoint1;
    [SerializeField]
    private Vector3 _borderPoint2;

    [Header("Camera Movement")]
    [SerializeField]
    private float _speed;
    [Tooltip("Essentially allows moving the camera faster as camera is zoomed out further.")]
    [SerializeField]
    private float _speedMultiplierByZoom;

    [Header("Camera Rotation")]
    [SerializeField]
    private float _rotationSpeed;

    [Header("Camera Zoom")]
    [SerializeField]
    private float _maxZoom;
    [SerializeField]
    private float _minZoom;
    [SerializeField]
    private float _scrollMultiplier;
    [SerializeField]
    private float _zoomSpeed;
    [Tooltip("Essentially allows zooming to become slower as zooming out.")]
    [SerializeField]
    private float _zoomSpeedMultiplierByZoom;

    private float _zoomGoal;

    private void Start()
    {
        _zoomGoal = transform.position.y;
    }

    private void Update()
    {
        HandleMovement();
        HandleRotation();
        HandleZooming();
    }

    private void HandleMovement()
    {
        float zoomMultiplier = Mathf.Max(1, transform.position.y * _speedMultiplierByZoom);
        float hori = Input.GetAxis("Horizontal") * zoomMultiplier * _speed * Time.deltaTime;
        float vert = Input.GetAxis("Vertical") * zoomMultiplier * _speed * Time.deltaTime;

        transform.position += transform.right * hori;
        transform.position += transform.up * vert;
        transform.position = new Vector3(
                                Mathf.Clamp(transform.position.x, _borderPoint1.x, _borderPoint2.x), 
                                transform.position.y,
                                Mathf.Clamp(transform.position.z, _borderPoint1.z, _borderPoint2.z
                            ));
    }

    private void HandleRotation()
    {
        if (Input.GetKey(KeyCode.Q))
        {
            transform.eulerAngles += Vector3.down * _rotationSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.E))
        {
            transform.eulerAngles += Vector3.up * _rotationSpeed * Time.deltaTime;
        }
    }

    private void HandleZooming()
    {
        if (Input.mouseScrollDelta.y != 0) AdjustZoomGoal();
        else ZoomToGoal();
    }

    private void AdjustZoomGoal()
    {
        float closenessSpeedMultiplier = Mathf.InverseLerp(_minZoom, _maxZoom, transform.position.y);
        float zoom = (-Input.mouseScrollDelta.y * _scrollMultiplier) * closenessSpeedMultiplier;
        _zoomGoal = Mathf.Clamp(_zoomGoal + zoom, _minZoom, _maxZoom);
    }

    private void ZoomToGoal()
    {
        float zoomMultiplier = Mathf.Max(1, transform.position.y * _zoomSpeedMultiplierByZoom);
        transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, _zoomGoal, transform.position.z), _zoomSpeed * zoomMultiplier * Time.deltaTime);
    }
}
