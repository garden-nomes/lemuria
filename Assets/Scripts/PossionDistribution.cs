
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PoissonDistribution
{
  public List<Vector2> Generate(Rect bounds, float radius, List<Vector2> initialPoints = null)
  {
    int expectedPoints = Mathf.FloorToInt(bounds.width * bounds.height / (radius * radius));
    var points = new List<Vector2>(expectedPoints);

    if (initialPoints != null)
    {
      points.AddRange(initialPoints);
    }

    for (int i = 0; i < expectedPoints * 3; i++)
    {
      var p = new Vector2(
        Random.Range(bounds.xMin, bounds.xMax),
        Random.Range(bounds.yMin, bounds.yMax));

      if (points.All(p2 => (p2 - p).sqrMagnitude > radius * radius))
      {
        points.Add(p);
      }
    }

    return points;
  }
}