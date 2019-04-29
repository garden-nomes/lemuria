using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeOut : MonoBehaviour
{
  public AnimationCurve fadeOut = AnimationCurve.Linear(0f, 1f, 2f, 0f);
  public AnimationCurve fadeIn = AnimationCurve.Linear(0f, 0f, 2f, 1f);
  public float duration = 5.0f;

  [SerializeField]
  bool hasFadedIn;
  [SerializeField]
  float timer;
  Text text;

  void Awake()
  {
    text = GetComponent<Text>();
  }

  void OnEnable()
  {
    timer = 0f;
    hasFadedIn = false;
    SetOpacity(0f);
  }

  void Update()
  {
    timer += Time.deltaTime;

    if (!hasFadedIn)
    {
      var opacity = fadeIn.Evaluate(timer);
      SetOpacity(opacity);
      Debug.Log(opacity);

      if (opacity >= 1f)
      {
        hasFadedIn = true;
        timer = 0f;
      }
    }
    else if (timer > duration)
    {
      var opacity = fadeOut.Evaluate(timer - duration);

      if (opacity <= 0f)
      {
        gameObject.SetActive(false);
        return;
      }

      SetOpacity(opacity);
    }
  }

  void SetOpacity(float opacity)
  {
    var color = text.color;
    color.a = opacity;
    text.color = color;
  }
}
