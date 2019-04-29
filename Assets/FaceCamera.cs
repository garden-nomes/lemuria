using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
  public Vector3 offset = Vector3.zero;

  void Update()
  {
    var targetRotation = Camera.main.transform.rotation.eulerAngles.y;

    var rotation = Quaternion.Euler(new Vector3(0f, targetRotation, 0f) + offset);
    transform.rotation = rotation;
  }
}
