using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class TileGrid : MonoBehaviour
{
  public float radius = 1f;
  public GameObject tilePrefab;

  // starts with lower-right, works counter-clockwise
  static readonly Vector2Int[] neighbors = {
    new Vector2Int(1, 0),
    new Vector2Int(1, -1),
    new Vector2Int(0, -1),
    new Vector2Int(-1, 0),
    new Vector2Int(-1, 1),
    new Vector2Int(0, 1),
  };

  static readonly float _sqrt3 = Mathf.Sqrt(3f);

  Dictionary<Vector2Int, Tile> tiles = new Dictionary<Vector2Int, Tile>();

  public Tile GetTile(Vector2Int hexCoords) =>
    tiles.TryGetValue(hexCoords, out var tile) ? tile : null;

  public Tile GetTile(Vector3 worldCoords) => GetTile(WorldToHex(worldCoords));

  void Start()
  {
    // add initial tile
    AddTile(Vector2Int.zero, TileType.Flat);
  }

  List<Tile> GetNeighbors(Vector2Int position)
  {
    return neighbors.Select(offset => GetTile(position + offset)).ToList();
  }

  public Tile[] allTiles => tiles.Values.ToArray();

  public bool CanBuild(Vector3 position, TileType type) => CanBuild(WorldToHex(position), type);

  public bool CanBuild(Vector2Int position, TileType type)
  {
    if (type == TileType.Mountain)
    {
      // mountains must be bordered by at least 4 hills
      return GetNeighbors(position)
        .Where(tile =>
          tile != null &&
          (tile.type == TileType.Hills || tile.type == TileType.Mountain))
        .Count() >= 4;
    }
    else if (type == TileType.Flat)
    {
      return GetNeighbors(position).Any(tile => tile != null);
    }

    return true;
  }

  public void AddTile(Vector2Int hexCoords, TileType type = TileType.Flat)
  {
    var obj = Instantiate(tilePrefab, HexToWorld(hexCoords), Quaternion.identity, transform);
    var tile = obj.GetComponent<Tile>();
    tile.type = type;
    tile.radius = radius;
    tiles.Add(hexCoords, tile);
  }

  public void AddTile(Vector3 worldCoords, TileType type = TileType.Flat) =>
    AddTile(WorldToHex(worldCoords), type);

  public Vector3 HexToWorld(Vector2 hexCoords)
  {
    var x = radius * 3f / 2f * hexCoords.x;
    var y = radius * (_sqrt3 / 2f * hexCoords.x + _sqrt3 * hexCoords.y);
    return new Vector3(x, 0f, y);
  }

  public Vector2Int WorldToHex(Vector3 worldCoords)
  {
    var x = 2f / 3f * worldCoords.x / radius;
    var y = (-1f / 3f * worldCoords.x + _sqrt3 / 3f * worldCoords.z) / radius;
    return new Vector2Int(Mathf.RoundToInt(x), Mathf.RoundToInt(y));
  }

  public float GetAverageHeightAtCorner(Vector3 worldCoords, int corner) =>
    GetAverageHeightAtCorner(WorldToHex(worldCoords), corner);

  public float GetAverageHeightAtCorner(Vector2Int hexCoords, int corner)
  {
    // corner starts at right-hand point, works counter-clockwise
    var tiles = new[]{
      hexCoords,
      neighbors[corner],
      neighbors[(corner + 1) % neighbors.Length],
    };

    return tiles
      .Select(coords => GetTile(coords))
      .Select(tile => tile == null ? 0f : TileTypeData.Get(tile.type).height)
      .Average();
  }
}
