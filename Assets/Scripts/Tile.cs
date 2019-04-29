using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using UnityEngine;

public enum TileType { Flat, Hills, Mountain, Forest, OldForest }

[RequireComponent(typeof(MeshFilter), typeof(Renderer))]
public class Tile : MonoBehaviour
{
  // apologies, dear reader:
  // this class is large and full of terrors

  public float radius = 1f;
  public float vertexRadius = 0.1f;
  public float noiseScale = 1f;
  public float noiseOffset = 1000f;
  public float baseHeight = 1f;
  [Range(0f, 1f)]
  public float noiseAmplitude = .5f;
  public float manifoldExponent = 1.5f;
  public float treeDistributionRadius = 0.2f;
  public Color selectedEmissionColor = Color.white;
  public GameObject tree;
  public float physicalHeight = 0f;
  public bool isBuilding;
  [Range(0f, 2f)]
  public float snowLine = 1f;
  [Range(0f, 2f)]
  public float cliffLine = 0.5f;
  public Dictionary<FaceType, Material> meshMaterials;
  public bool continuouslyUpdate = false;

  public delegate void ScoreEvent(int points);
  public delegate void BuildEvent(TileType type);
  public static event ScoreEvent OnScore;
  public static event BuildEvent OnBuild;

  float _height = TileTypeData.Get(TileType.Flat).height;
  float scoreTimer = 0f;
  float forestTimer = 0f;
  Color originalEmissionColor;
  int[] baseVertices;
  TileGrid grid;

  static readonly float _sqrt3 = Mathf.Sqrt(3);

  public TileTypeData data => TileTypeData.Get(type);

  public enum FaceType { Grass, Cliff, Snow };

  TileType _type = TileType.Flat;
  public TileType type
  {
    get => _type;
    set
    {
      if (type != value)
      {
        UpdateType(value);
      }
    }
  }

  bool _isSelected = false;
  public bool isSelected
  {
    get => _isSelected;
    set
    {
      if (_isSelected != value)
      {
        UpdateIsSelected(value);
      }
    }
  }

  public List<TileType> canBuild
  {
    get
    {
      switch (type)
      {
        case TileType.Flat:
          return new List<TileType> { TileType.Forest, TileType.Hills };
        case TileType.Hills:
          return new List<TileType> { TileType.Mountain };
        default:
          return new List<TileType>();
      }
    }
  }

  void Awake()
  {
    originalEmissionColor = GetComponent<MeshRenderer>().material.GetColor("_EmissionColor");
    grid = GetComponentInParent<TileGrid>();
  }

  void UpdateType(TileType newType)
  {
    StartCoroutine(HeightTransitionCoroutine(type, newType));
    var typeData = TileTypeData.Get(newType);
    var oldTypeData = TileTypeData.Get(type);

    if (typeData.hasTrees && !oldTypeData.hasTrees)
    {
      SpawnTrees(newType);
      UpdateTreeType(newType);
    }
    else if (!typeData.hasTrees && oldTypeData.hasTrees)
    {
      RemoveTrees();
    }
    else if (typeData.treeType != oldTypeData.treeType)
    {
      UpdateTreeType(newType);
    }


    _type = newType;
    UpdateParticleSystem();
    scoreTimer = 1f / data.pointsPerSecond;
  }

  void UpdateParticleSystem(bool triggerPayload = false)
  {
    var ps = GetComponent<ParticleSystem>();
    if (ps != null && !isBuilding)
    {
      var em = ps.emission;
      em.rateOverTime = data.pointsPerSecond;

      if (triggerPayload)
      {
        var burst = new ParticleSystem.Burst(0f, (short)data.payload, (short)data.payload, 1, 0f);
        em.SetBursts(new[] { burst });
      }

      ps.Play();
    }
    else if (isBuilding)
    {
      ps.Stop();
    }
  }

  void UpdateIsSelected(bool isSelected)
  {
    _isSelected = isSelected;

    var renderer = GetComponent<Renderer>();
    if (isSelected)
    {
      originalEmissionColor = renderer.material.GetColor("_EmissionColor");
      renderer.material.SetColor("_EmissionColor", selectedEmissionColor);
    }
    else
    {
      renderer.material.SetColor("_EmissionColor", originalEmissionColor);
    }
  }

  IEnumerator HeightTransitionCoroutine(TileType? from, TileType to)
  {
    isBuilding = true;
    float fromHeight = from == null ? -baseHeight : TileTypeData.Get(from.Value).height;
    float toHeight = TileTypeData.Get(to).height;
    float transitionTime = TileTypeData.Get(to).buildTime;

    for (float t = 0f; t < transitionTime; t += Time.deltaTime)
    {
      var a = t / transitionTime;
      var eased = ((a *= 2f) <= 1f ? a * a : --a * (2f - a) + 1f) / 2f;
      _height = fromHeight + eased * (toHeight - fromHeight);
      UpdateMesh();
      yield return null;
    }

    _height = toHeight;
    UpdateMesh();
    isBuilding = false;

    if (OnScore != null)
    {
      OnScore(data.payload);
    }

    if (OnBuild != null)
    {
      OnBuild(to);
    }

    UpdateParticleSystem(true);
  }

  void SpawnTrees(TileType type)
  {
    var rect = new Rect(-radius, -radius, radius * 2, radius * 2);

    var points = new PoissonDistribution()
      .Generate(
        rect,
        treeDistributionRadius)
      .Where(p => IsInsideHex(p))
      .Select(v2 => new Vector3(v2.x, 0f, v2.y));

    float transitionTime = TileTypeData.Get(type).buildTime;

    foreach (var point in points)
    {
      var obj = Instantiate(tree, transform.position + point, Quaternion.identity, transform);
      obj.GetComponent<Tree>().growingAnimationLength = transitionTime;

    }
  }

  void RemoveTrees()
  {
    foreach (Transform child in transform)
    {
      Destroy(child.gameObject);
    }
  }

  void UpdateTreeType(TileType type)
  {
    var trees = GetComponentsInChildren<Tree>();
    var treeType = TileTypeData.Get(type).treeType;

    foreach (var tree in trees)
    {
      tree.growingAnimationLength = TileTypeData.Get(type).buildTime;
      tree.type = treeType;
    }
  }

  void Start()
  {
    scoreTimer = 1f / data.pointsPerSecond;
    AttachMesh();
    UpdateParticleSystem();
    StartCoroutine(HeightTransitionCoroutine(null, type));
  }

  void Update()
  {
    if (continuouslyUpdate)
    {
      UpdateMesh();
    }

    if (isBuilding)
    {
      return;
    }

    if (data.pointsPerSecond > 0f)
    {
      scoreTimer -= Time.deltaTime;

      if (scoreTimer <= 0)
      {
        if (OnScore != null)
        {
          OnScore(1);
        }

        scoreTimer = 1f / data.pointsPerSecond;
      }
    }

    if (type == TileType.Forest)
    {
      forestTimer += Time.deltaTime;

      if (forestTimer > data.timeToUpgrade)
      {
        type = TileType.OldForest;
      }
    }
  }

  void AttachMesh()
  {
    var meshFilter = GetComponent<MeshFilter>();
    meshFilter.sharedMesh = GenerateMesh();
  }

  void DetachMesh()
  {
    var meshFilter = GetComponent<MeshFilter>();
    meshFilter.sharedMesh = null;
  }

  Mesh GenerateMesh()
  {
    Polygon polygon = new Polygon();

    // add corners
    for (int i = 0; i < 6; i++)
    {
      var a = Mathf.PI * 2f * ((float)i / 6f);
      polygon.Add(new Vertex(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius));
    }

    // fill with poisson distribution
    var rect = new Rect(-radius, -radius, radius * 2, radius * 2);
    foreach (var vertex in new PoissonDistribution()
      .Generate(
        rect,
        vertexRadius,
        polygon.Points.Select(p => new Vector2((float)p.x, (float)p.y)).ToList())
      .Where(p => IsInsideHex(p))
      .Select(v2 => new Vertex(v2.x, v2.y)))
    {
      polygon.Add(vertex);
    }

    // generate triangulation
    var triangulation = (TriangleNet.Mesh)polygon.Triangulate();

    // instantiate vertices
    var vertices = new List<Vector3>();
    foreach (var triangle in triangulation.triangles)
    {
      var triangleVertices = triangle.vertices.Select(
        vertex => new Vector3((float)vertex.x, GetHeight(vertex.x, vertex.y), (float)vertex.y))
        .Reverse();

      vertices.AddRange(triangleVertices);
    }

    baseVertices = new int[18];

    // attach base
    for (int i = 0; i < 6; i++)
    {
      var t1 = Mathf.PI * 2f * ((float)i / 6f);
      var t2 = Mathf.PI * 2f * ((float)(i + 1) / 6f);
      var p1 = new Vector3(Mathf.Cos(t1), 0f, Mathf.Sin(t1));
      var p2 = new Vector3(Mathf.Cos(t2), 0f, Mathf.Sin(t2));
      var p3 = new Vector3(Mathf.Cos(t2), -baseHeight, Mathf.Sin(t2));
      var p4 = new Vector3(Mathf.Cos(t1), -baseHeight, Mathf.Sin(t1));
      vertices.Add(p1);
      vertices.Add(p2);
      baseVertices[i * 3] = vertices.Count;
      vertices.Add(p3);
      vertices.Add(p1);
      baseVertices[i * 3 + 1] = vertices.Count;
      vertices.Add(p3);
      baseVertices[i * 3 + 2] = vertices.Count;
      vertices.Add(p4);
    }

    // create mesh
    var mesh = new Mesh();
    mesh.vertices = vertices.ToArray();
    mesh.RecalculateNormals();
    SetTriangles(mesh);
    CalculatePhysicalHeight(mesh);
    return mesh;
  }

  void SetTriangles(Mesh mesh)
  {
    // need to divide triangles into submeshes to apply different materials
    var faceTypes = new[] { FaceType.Cliff, FaceType.Grass, FaceType.Snow }; // order is important
    var subMeshes = SortFacesByType(mesh.vertices);
    mesh.subMeshCount = faceTypes.Length;

    for (int i = 0; i < faceTypes.Length; i++)
    {
      var faceType = faceTypes[i];

      if (!subMeshes.ContainsKey(faceType))
      {
        mesh.SetTriangles(new int[0], i);
        continue;
      }

      mesh.SetTriangles(subMeshes[faceType].ToArray(), i);
    }
  }

  Dictionary<FaceType, List<int>> SortFacesByType(Vector3[] vertices)
  {
    var result = new Dictionary<FaceType, List<int>>();

    for (var i = 0; i < vertices.Length; i += 3)
    {
      var faceType = GetFaceType(vertices[i], vertices[i + 1], vertices[i + 2]);

      if (!result.ContainsKey(faceType))
      {
        result[faceType] = new List<int>(vertices.Length);
      }

      result[faceType].Add(i);
      result[faceType].Add(i + 1);
      result[faceType].Add(i + 2);
    }

    return result;
  }

  FaceType GetFaceType(Vector3 a, Vector3 b, Vector3 c)
  {
    var height = (a.y + b.y + c.y) / 3f;

    return height > snowLine
      ? FaceType.Snow
      : (height > cliffLine ? FaceType.Cliff : FaceType.Grass);
  }

  Vector3 Normal(Vector3 a, Vector3 b, Vector3 c) => Vector3.Cross(b - a, c - a).normalized;

  void CalculatePhysicalHeight(Mesh mesh)
  {
    float treeHeight = 0f;
    var trees = GetComponentsInChildren<Tree>();
    if (trees.Length > 0)
    {
      treeHeight = trees.Select(t => t.currentHeight).Max();
    }

    physicalHeight = mesh.vertices.Select(v => v.y).Max() + treeHeight;
  }

  float GetHeight(double x, double y) => GetHeight((float)x, (float)y);

  float GetHeight(float x, float y) => GetNoiseHeight(x, y) * GetManifoldHeight(x, y);

  float GetNoiseHeight(float x, float y)
  {
    return Mathf.PerlinNoise(
      (transform.position.x + x) * noiseScale + noiseOffset,
      (transform.position.y + y) * noiseScale + noiseOffset)
      * noiseAmplitude + (1 - noiseAmplitude);
  }

  float GetManifoldHeight(float x, float y)
  {
    // FYI I have no what the word "manifold" means. It just feels appropriate.

    if (_height < 0f)
    {
      return _height;
    }

    return Mathf.Pow((1f - Mathf.Sqrt(x * x + y * y)), manifoldExponent) * _height;

    // was trying to do something fancier below with interpolating height between tiles, didn't
    // have the time, leaving it here in case I come back to it

    // angle of vector
    var a = Mathf.Atan2(y, x);

    // modulus doesn't do negative numbers
    while (a < 0f) a += Mathf.PI * 2f;
    a %= Mathf.PI * 2f;

    // find the corners the points lies between
    int c1 = Mathf.FloorToInt(a / (Mathf.PI * 2f) * 6f);
    int c2 = (c1 + 1) % 6;
    float ca1 = c1 * Mathf.PI / 3f;
    float ca2 = c2 * Mathf.PI / 3f;

    Vector2 p = new Vector2(x, y);
    Vector2 p1 = new Vector2(Mathf.Cos(c1), Mathf.Sign(c1));
    Vector2 p2 = new Vector2(Mathf.Cos(c2), Mathf.Sign(c2));

    var average =
      ((p - p1).magnitude * grid.GetAverageHeightAtCorner(transform.position, c1) +
       (p - p2).magnitude * grid.GetAverageHeightAtCorner(transform.position, c2) +
       (p - Vector2.zero).magnitude * _height) / 3f;

    return average;
  }

  void UpdateMesh()
  {
    var meshFilter = GetComponent<MeshFilter>();
    meshFilter.sharedMesh.vertices = meshFilter.sharedMesh.vertices.Select((vertex, index) =>
      baseVertices.Contains(index)
        ? vertex
        : new Vector3(vertex.x, GetHeight(vertex.x, vertex.z), vertex.z)
    ).ToArray();

    meshFilter.sharedMesh.RecalculateNormals();
    CalculatePhysicalHeight(meshFilter.sharedMesh);
    SetTriangles(meshFilter.sharedMesh);
  }

  bool IsInsideHex(Vector2 p)
  {
    var x = Mathf.Abs(p.x) / radius;
    var y = Mathf.Abs(p.y) / radius;

    if (y > _sqrt3 / 2f * radius || x > radius)
    {
      return false;
    }

    return -1f * _sqrt3 * x + _sqrt3 - y > 0f;
  }
}
