using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControls : MonoBehaviour
{
  public float dragStartThreshold = 0.1f;
  public float minZoom = 1f;
  public float maxZoom = 10f;
  public float zoom = 5f;
  public float zoomSpeed = 0.1f;
  public float rotateSpeed = 0.5f;

  public Vector3 center = new Vector3(0f, 0f, 0f);
  [SerializeField] Vector3 offset;
  Vector3 dragStartMousePosition;
  float dragStartTime;
  Vector3 previouseMousePosition;

  void Start()
  {
    offset = (transform.position - center).normalized;
    zoom = minZoom + (maxZoom - minZoom) / 2f;
    previouseMousePosition = Input.mousePosition;
  }

  void Update()
  {
    PanCamera();
    ZoomCamera();
    RotateCamera();

    transform.position = center + offset * zoom;
    transform.LookAt(center);
  }

  void PanCamera()
  {
    if (Input.GetMouseButtonDown(0))
    {
      dragStartMousePosition = worldMousePosition;
      dragStartTime = Time.time;
    }

    if (Input.GetMouseButton(0))
    {
      center += (dragStartMousePosition - worldMousePosition);
    }
  }

  void ZoomCamera()
  {
    zoom -= Input.mouseScrollDelta.y * zoomSpeed;
    zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
  }

  void RotateCamera()
  {
    if (Input.GetMouseButton(0))
    {
      return;
    }

    var delta = (Input.mousePosition - previouseMousePosition) * -rotateSpeed;
    previouseMousePosition = Input.mousePosition;

    if (Input.GetMouseButton(1))
    {
      offset = Quaternion.Euler(delta.y, -delta.x, 0f) * offset;
      offset.y = Mathf.Clamp(offset.y, 0.1f, 1f);
      offset.Normalize();
    }
  }


  Vector3 worldMousePosition
  {
    get
    {
      var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      new Plane(Vector3.up, 0f).Raycast(ray, out var hit);
      return ray.GetPoint(hit);
    }
  }
}
