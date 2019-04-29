using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using UnityEngine;

public enum TreeType { Normal, Old }

public class TreeTypeData
{
  static readonly float scale = (1f + Mathf.Sqrt(5f)) / 2f;

  static public TreeTypeData Get(TreeType type)
  {
    switch (type)
    {
      case TreeType.Normal:
        return new TreeTypeData
        {
          height = .25f,
          radius = .0625f
        };
      case TreeType.Old:
        return new TreeTypeData
        {
          height = .25f * Mathf.Pow(scale, 2f),
          radius = .0625f * Mathf.Pow(scale, 1f)
        };
      default:
        return Get(TreeType.Normal);
    }
  }

  public float height;
  public float radius;
  public float variance = 0.5f;
}

[RequireComponent(typeof(MeshFilter))]
public class Tree : MonoBehaviour
{
  public float trunkRatio = 0.15f;
  public int segments = 6;
  public int ringSegments = 6;
  public float growingAnimationLength = 2f;

  TreeType _type = TreeType.Normal;
  public TreeType type
  {
    get => _type;
    set
    {
      if (_type != value)
      {
        UpdateType(value);
      }
    }
  }

  public float currentHeight => _height;
  float _height = 0f;
  float _radius = TreeTypeData.Get(TreeType.Normal).radius;

  void Start()
  {
    AttachMesh();
    StartCoroutine(GrowingAnimationCoroutine());
  }

  void OnEnable()
  {
    AttachMesh();
    StartCoroutine(GrowingAnimationCoroutine());
  }

  void OnDisable()
  {
    DetachMesh();
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

  void UpdateType(TreeType newType)
  {
    _type = newType;
    StartCoroutine(GrowingAnimationCoroutine());
  }

  IEnumerator GrowingAnimationCoroutine()
  {
    var fromHeight = _height;
    var fromRadius = _radius;
    var toHeight = TreeTypeData.Get(_type).height;
    var toRadius = TreeTypeData.Get(_type).radius;
    var variance = TreeTypeData.Get(_type).variance;

    toHeight = Random.Range(toHeight - toHeight * variance, toHeight + toHeight * variance);
    toRadius = Random.Range(toRadius - toRadius * variance, toRadius + toRadius * variance);

    for (float t = 0f; t < growingAnimationLength; t += Time.deltaTime)
    {
      var a = t / growingAnimationLength;
      var eased = ((a *= 2f) <= 1f ? a * a : --a * (2f - a) + 1f) / 2f;
      _height = eased * (toHeight - fromHeight) + fromHeight;
      _radius = eased * (toRadius - fromRadius) + fromRadius;
      UpdateMesh();
      yield return null;
    }

    _height = toHeight;
    _radius = toRadius;
    UpdateMesh();
  }

  List<Vector3> GenerateVertices()
  {
    var vertices = new List<Vector3>();

    // trunk
    vertices.AddRange(GetCone(0f, trunkRatio * _height, _radius * 0.25f, _radius * 0.25f));

    // segments
    for (int i = 0; i < segments; i++)
    {
      var bottom = ((float)i / (float)segments) * _height * (1 - trunkRatio) + _height * trunkRatio;
      var top = ((float)(i + 1) / (float)segments) * _height * (1 - trunkRatio) + _height * trunkRatio;
      var bottomRadius = (1 - ((float)i / (float)segments)) * _radius;
      var topRadius = (1 - ((float)(i + 1) / (float)segments)) * _radius * .5f;
      var twist = (i + 1) / (float)segments * Mathf.PI * 2f * _height;

      vertices.AddRange(GetCone(bottom, top, bottomRadius, topRadius, twist));
    }

    return vertices;
  }

  Mesh GenerateMesh()
  {
    var vertices = GenerateVertices();

    // create mesh
    var mesh = new Mesh();
    mesh.vertices = vertices.ToArray();
    mesh.triangles = vertices.Select((_, index) => index).ToArray();
    mesh.RecalculateNormals();
    return mesh;
  }

  void UpdateMesh()
  {
    var mesh = GetComponent<MeshFilter>().sharedMesh;
    mesh.vertices = GenerateVertices().ToArray();
    mesh.RecalculateNormals();
  }

  List<Vector3> GetCone(float bottom, float top, float bottomRadius, float topRadius, float twist = 0f)
  {
    var vertices = new List<Vector3>(ringSegments * 6);

    for (int i = 0; i < ringSegments; i++)
    {
      var t1 = Mathf.PI * 2f * ((float)i / (float)ringSegments) + twist;
      var t2 = Mathf.PI * 2f * ((float)(i - 1) / (float)ringSegments) + twist;

      // side
      var p1 = new Vector3(Mathf.Cos(t1) * bottomRadius, bottom, Mathf.Sin(t1) * bottomRadius);
      var p2 = new Vector3(Mathf.Cos(t2) * bottomRadius, bottom, Mathf.Sin(t2) * bottomRadius);
      var p3 = new Vector3(Mathf.Cos(t2) * topRadius, top, Mathf.Sin(t2) * topRadius);
      var p4 = new Vector3(Mathf.Cos(t1) * topRadius, top, Mathf.Sin(t1) * topRadius);
      vertices.Add(p1);
      vertices.Add(p2);
      vertices.Add(p3);
      vertices.Add(p1);
      vertices.Add(p3);
      vertices.Add(p4);

      // caps
      vertices.Add(p2);
      vertices.Add(p1);
      vertices.Add(new Vector3(0f, bottom, 0f));
      vertices.Add(p4);
      vertices.Add(p3);
      vertices.Add(new Vector3(0f, top, 0f));
    }

    return vertices;
  }
}
