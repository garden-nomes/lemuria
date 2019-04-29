using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialController : MonoBehaviour
{
  public GameObject initialTutorial;
  public GameObject hillsAndForestTutorial;
  public GameObject hillTutorial;
  public GameObject forestTutorial;

  int tilesBuilt = 0;
  int hillsBuilt = 0;
  int forestsBuilt = 0;

  void Awake()
  {
    Tile.OnBuild += OnTileBuild;
  }

  void Start()
  {
    StartCoroutine(InitialTutorialCoroutine());
  }

  IEnumerator InitialTutorialCoroutine()
  {
    yield return new WaitForSeconds(TileTypeData.Get(TileType.Flat).buildTime);
    initialTutorial.SetActive(true);
  }

  void OnTileBuild(TileType type)
  {
    tilesBuilt++;

    if (tilesBuilt == 4)
    {
      hillsAndForestTutorial.SetActive(true);
    }

    if (type == TileType.Forest)
    {
      forestsBuilt++;

      if (forestsBuilt == 2)
      {
        forestTutorial.SetActive(true);
      }
    }

    if (type == TileType.Hills)
    {
      hillsBuilt++;

      if (hillsBuilt == 2)
      {
        hillTutorial.SetActive(true);
      }
    }
  }
}
