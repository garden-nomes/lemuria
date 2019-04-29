using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Title : MonoBehaviour
{
  public AnimationCurve fade = AnimationCurve.Linear(0f, 1f, 2f, 0f);
  public float timeBeforeFade = 5.0f;

  float startTime;
  Text text;

  void Awake()
  {
    text = GetComponent<Text>();
  }

  void OnEnable()
  {
    startTime = Time.time;
    var color = text.color;
    color.a = 1f;
    text.color = color;
  }

  void Update()
  {
    var t = Time.time - startTime;

    if (t > timeBeforeFade)
    {
      var opacity = fade.Evaluate(t - timeBeforeFade);

      if (opacity < 0f)
      {
        gameObject.SetActive(false);
        return;
      }

      var color = text.color;
      color.a = opacity;
      text.color = color;
    }
  }
}
