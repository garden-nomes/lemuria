using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
  public TileGrid grid;
  public ScoreKeeper scoreKeeper;

  public float maxClickHoldTime = 0.1f;
  float mousePressedAt;
  Tile activeTile = null;

  public Canvas tileUICanvas;
  public float tileUIHeight = 0.25f;
  public GameObject buttonPrefab;
  public GameObject emptyTileMarker;

  Vector3 mousePosition
  {
    get
    {
      var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      var plane = new Plane(Vector3.up, 0f);
      plane.Raycast(ray, out var hit);
      return ray.GetPoint(hit);
    }
  }

  void Start()
  {
    RefreshButtons();
  }

  void Update()
  {
    if (grid == null)
    {
      return;
    }

    SetActiveTile();
    UpdateTileUI();
  }

  void SetActiveTile()
  {
    var tile = grid.GetTile(mousePosition);

    if (tile != activeTile)
    {
      if (activeTile != null) activeTile.isSelected = false;
      if (tile != null) tile.isSelected = true;
      activeTile = tile;

      RefreshButtons();
    }

    if (activeTile == null)
    {
      emptyTileMarker.SetActive(true);
      emptyTileMarker.transform.position = grid.HexToWorld(grid.WorldToHex(mousePosition));
    }
    else
    {
      emptyTileMarker.SetActive(false);
    }

  }

  void UpdateTileUI()
  {
    if (activeTile != null)
    {
      var height = tileUIHeight + activeTile.physicalHeight;
      tileUICanvas.transform.position = activeTile.transform.position + Vector3.up * height;
    }
    else
    {
      var height = tileUIHeight;
      var position = grid.HexToWorld(grid.WorldToHex(mousePosition));
      tileUICanvas.transform.position = position + Vector3.up * height;
    }
  }

  void SetTile(TileType type)
  {
    var cost = TileTypeData.Get(type).cost;

    if (scoreKeeper.points < cost || !grid.CanBuild(mousePosition, type))
    {
      return;
    }

    scoreKeeper.points -= cost;

    if (activeTile != null)
    {
      activeTile.type = type;
    }
    else
    {
      grid.AddTile(mousePosition, type);
    }

    RefreshButtons();
  }

  void RefreshButtons()
  {
    RemoveButtons();
    AttachButtons();
  }

  void AttachButtons()
  {
    var availableTileTypes = activeTile == null
        ? new List<TileType> { TileType.Flat }
        : activeTile.canBuild;

    foreach (var type in availableTileTypes)
    {
      if (!grid.CanBuild(mousePosition, type))
      {
        continue;
      }

      var instance = Instantiate(
        buttonPrefab,
        tileUICanvas.transform.position,
        tileUICanvas.transform.rotation,
        tileUICanvas.transform);

      var buttonController = instance.GetComponent<ButtonController>();
      buttonController.hexCoords = grid.WorldToHex(mousePosition);
      buttonController.type = type;

      var button = instance.GetComponent<Button>();
      button.onClick.AddListener(() => SetTile(type));
    }
  }

  void RemoveButtons()
  {
    foreach (var button in tileUICanvas.GetComponentsInChildren<Button>())
    {
      Destroy(button.gameObject);
    }
  }
}
