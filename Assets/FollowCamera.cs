using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
  public CameraControls cam;
  public Vector3 offset = new Vector3(0f, -.5f, 0f);

  void Update()
  {
    transform.position = cam.center + offset;
  }
}
