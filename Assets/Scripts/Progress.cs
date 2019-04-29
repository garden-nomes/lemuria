using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Progress : MonoBehaviour
{
  [Range(0f, 1f)]
  public float value = 0.5f;

  void Update()
  {
    var rectTransform = (RectTransform)transform;
    var parentRectTransform = (RectTransform)transform.parent;
    var width = parentRectTransform.rect.width;
    var targetWidth = value * width;
    rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0f, targetWidth);
  }
}
