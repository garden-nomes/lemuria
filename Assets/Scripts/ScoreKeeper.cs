using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreKeeper : MonoBehaviour
{
  public int points;
  public TileGrid grid;
  public Text scoreLabel;

  void Start()
  {
    points = 0;
  }

  void OnEnable()
  {
    Tile.OnScore += OnScore;
  }

  void OnDisable()
  {
    Tile.OnScore -= OnScore;
  }

  void OnScore(int p)
  {
    points += p;

    if (scoreLabel != null)
    {
      scoreLabel.text = $"{points}";
    }
  }
}
