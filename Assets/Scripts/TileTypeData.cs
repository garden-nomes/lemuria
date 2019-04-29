public class TileTypeData
{
  public float buildTime;
  public float height;
  public bool hasTrees = false;
  public TreeType treeType = TreeType.Normal;
  public string label;
  public float pointsPerSecond;
  public int cost;
  public int payload;
  public float timeToUpgrade;

  static public TileTypeData Get(TileType type)
  {
    switch (type)
    {
      case TileType.Flat:
        return new TileTypeData
        {
          height = .1f,
          label = "Flat",
          buildTime = 2f,
          pointsPerSecond = 0f,
          payload = 2,
          cost = 1
        };
      case TileType.Hills:
        return new TileTypeData
        {
          height = 1f,
          label = "Hills",
          buildTime = 5f,
          pointsPerSecond = 0f,
          payload = 15,
          cost = 10
        };
      case TileType.Mountain:
        return new TileTypeData
        {
          height = 3f,
          label = "Mountain",
          pointsPerSecond = 0f,
          buildTime = 10f,
          payload = 150,
          cost = 100
        };
      case TileType.Forest:
        return new TileTypeData
        {
          height = .1f,
          hasTrees = true,
          label = "Forest",
          pointsPerSecond = 1f,
          buildTime = 10f,
          payload = 15,
          cost = 10,
          timeToUpgrade = 60f
        };
      case TileType.OldForest:
        return new TileTypeData
        {
          height = .1f,
          hasTrees = true,
          treeType = TreeType.Old,
          label = "Old Forest",
          buildTime = 10f,
          pointsPerSecond = 5f
        };
      default:
        return Get(TileType.Flat);
    }
  }
}
