using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonController : MonoBehaviour
{
  public Vector2Int hexCoords;
  public TileType type;

  Text label;
  Button button;
  ScoreKeeper scoreKeeper;
  TileGrid grid;
  TileTypeData data => TileTypeData.Get(type);
  Progress progressBar;

  bool isEnabled =>
    scoreKeeper.points >= data.cost &&
    grid.CanBuild(hexCoords, type);

  void Start()
  {
    progressBar = GetComponentInChildren<Progress>();
    button = GetComponent<Button>();
    label = GetComponentInChildren<Text>();
    scoreKeeper = transform.root.GetComponentInChildren<ScoreKeeper>();
    grid = GetComponentInParent<TileGrid>();
  }

  void Update()
  {
    label.text = data.label;
    button.interactable = isEnabled;
    progressBar.value = (float)scoreKeeper.points / (float)data.cost;
    progressBar.gameObject.SetActive(progressBar.value < 1f);
  }
}
