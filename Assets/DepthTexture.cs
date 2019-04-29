using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DepthTexture : MonoBehaviour
{
  void Start()
  {
    GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
  }
}
