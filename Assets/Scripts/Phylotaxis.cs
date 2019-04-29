using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Phylotaxis : MonoBehaviour
{
  // use doubles for precision
  static readonly double goldenAngle = Math.PI * (3 - Math.Sqrt(5)); // radians

  public float radius = 10.0f;
  public GameObject circlePrefab;
  public float animationInterval = 0.01f;
  public ScoreKeeper scoreKeeper;
  public UIController uiController;
  public Color circleColor = Color.white;
  public Color pointPreviewPositiveColor = Color.green;
  public Color pointPreviewNegativeColor = Color.red;
  public AudioClip[] boops;
  public AudioSource audioSource;

  int _points = 0;
  bool isAnimationCoroutineRunning = false;

  Stack<GameObject> circles = new Stack<GameObject>();
  Stack<GameObject> negativeCircles = new Stack<GameObject>();

  void Update()
  {
    if (scoreKeeper.points != _points)
    {
      if (!isAnimationCoroutineRunning)
      {
        StartCoroutine(AnimateCoroutine());
      }
    }

    if (negativeCircles.Count != uiController.pointPreview - circles.Count)
    {
      while (negativeCircles.Count != 0)
      {
        Destroy(negativeCircles.Pop().gameObject);
      }

      while (negativeCircles.Count < uiController.pointPreview - circles.Count)
      {
        var position = GetPositionAboveZero(circles.Count + negativeCircles.Count);
        var pos3 = new Vector3(position.x, position.y, 0f);
        var obj = Instantiate(
            circlePrefab,
            transform.position + pos3,
            transform.rotation,
            transform);
        obj.GetComponent<Image>().color = pointPreviewNegativeColor;
        negativeCircles.Push(obj);
      }
    }

    var array = circles.ToArray();
    for (int i = 0; i < circles.Count; i++)
    {
      array[i].GetComponent<Image>().color =
          i < uiController.pointPreview && uiController.pointPreview <= scoreKeeper.points
            ? pointPreviewPositiveColor
            : circleColor;
    }
  }

  IEnumerator AnimateCoroutine()
  {
    isAnimationCoroutineRunning = true;

    while (_points != scoreKeeper.points)
    {
      if (_points < scoreKeeper.points)
      {
        AddPoint();
        _points++;
      }
      else if (_points > scoreKeeper.points)
      {
        RemovePoint();
        _points--;
      }

      yield return new WaitForSeconds(animationInterval);
    }

    isAnimationCoroutineRunning = false;
  }

  void AddPoint()
  {
    var position = GetPositionAboveZero(circles.Count);
    var pos3 = new Vector3(position.x, position.y, 0f);
    var obj = Instantiate(
        circlePrefab,
        transform.position + pos3,
        transform.rotation,
        transform);

    if (audioSource != null)
    {
      var clip = boops[UnityEngine.Random.Range(0, boops.Length)];
      audioSource.PlayOneShot(clip);
    }

    circles.Push(obj);
  }

  void RemovePoint()
  {
    if (circles.Count == 0)
    {
      return;
    }

    var obj = circles.Pop();
    Destroy(obj.gameObject);
  }

  Vector2 GetPosition(int n)
  {
    var a = goldenAngle * n;
    var d = Math.Sqrt(n * radius);
    return new Vector2((float)(Math.Cos(a) * d), (float)(Math.Sin(a) * d));
  }

  Vector2 GetPositionAboveZero(int n)
  {
    // Might be a better way to do this. For now, brute force works.
    int i = 0, j = 0;

    for (; ; )
    {
      var position = GetPosition(i);
      if (position.y >= 0f) j++;
      if (j == (n + 1)) return position;
      i++;
    }
  }
}
