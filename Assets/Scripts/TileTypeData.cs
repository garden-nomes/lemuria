using UnityEngine;

public class TileTypeData
{
  public float buildTime;
  public float height;
  public bool hasTrees = false;
  public TreeType treeType = TreeType.Normal;
  public string label;
  public float pointsPerSecond = 0f;
  public int cost;
  public int payload = 0;
  public float timeToUpgrade;

  // golden ratio
  static readonly float scale = (1f + Mathf.Sqrt(5f)) / 2f;

  public static TileTypeData Flat = new TileTypeData
  {
    height = Mathf.Pow(scale, -4f),
    label = "Land",
    buildTime = 5f,
    payload = 8,
    cost = 5
  };

  public static TileTypeData Hills = new TileTypeData
  {
    height = Mathf.Pow(scale, 1f),
    label = "Hills",
    buildTime = 10f,
    payload = 15,
    cost = 15
  };

  public static TileTypeData Mountain = new TileTypeData
  {
    height = Mathf.Pow(scale, 2f),
    label = "Mountain",
    buildTime = 20f,
    payload = 50,
    cost = 100
  };

  public static TileTypeData Forest = new TileTypeData
  {
    height = Mathf.Pow(scale, -4f),
    hasTrees = true,
    label = "Forest",
    pointsPerSecond = .25f,
    buildTime = 10f,
    cost = 10,
    timeToUpgrade = 60f
  };

  public static TileTypeData OldForest = new TileTypeData
  {
    height = Mathf.Pow(scale, -4f),
    hasTrees = true,
    treeType = TreeType.Old,
    label = "Old Forest",
    buildTime = 10f,
    pointsPerSecond = 1f
  };

  static public TileTypeData Get(TileType type)
  {
    switch (type)
    {
      case TileType.Flat:
        return Flat;
      case TileType.Hills:
        return Hills;
      case TileType.Mountain:
        return Mountain;
      case TileType.Forest:
        return Forest;
      case TileType.OldForest:
        return OldForest;
      default:
        return Flat;
    }
  }
}
