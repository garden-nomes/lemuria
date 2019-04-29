using System.Collections.Generic;
using System.Linq;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(Renderer))]
public class WaterMesh : MonoBehaviour
{
  public float width = 100f;
  public float height = 100f;
  public float vertexDensity = 1f;
  public float noiseScale = .5f;
  public float waveHeight = .5f;
  public float waveHeightPadding = .1f;
  public float speed = 0.01f;

  float waveDirection =>
    (Mathf.PerlinNoise(Time.time * speed, 0f) * Mathf.PI * 2 * 10) % (Mathf.PI * 2);

  void Start()
  {
    // AttachMesh();
    transform.position = new Vector3(transform.position.x, -waveHeight, transform.position.z);
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

  void Update()
  {
    transform.position = new Vector3(
        transform.position.x,
        -waveHeight + (Mathf.Sin(Time.time * speed) * .5f + .5f) * waveHeight - waveHeightPadding,
        transform.position.z
    );
    // UpdateMesh();
  }


  Mesh GenerateMesh()
  {
    Polygon polygon = new Polygon();

    // add corners
    polygon.Add(new Vertex(-width / 2f, -height / 2f));
    polygon.Add(new Vertex(-width / 2f, height / 2f));
    polygon.Add(new Vertex(width / 2f, -height / 2f));
    polygon.Add(new Vertex(width / 2f, height / 2f));

    // fill with poisson distribution
    var rect = new Rect(-width / 2f, -height / 2f, width, height);
    foreach (var vertex in new PoissonDistribution()
      .Generate(
        rect,
        vertexDensity,
        polygon.Points.Select(p => new Vector2((float)p.x, (float)p.y)).ToList())
      .Select(v2 => new Vertex(v2.x, v2.y)))
    {
      polygon.Add(vertex);
    }

    // generate triangulation
    var triangulation = (TriangleNet.Mesh)polygon.Triangulate(new ConstraintOptions
    {
      ConformingDelaunay = true
    });

    // instantiate vertices
    var vertices = new List<Vector3>();
    foreach (var triangle in triangulation.triangles)
    {
      var triangleVertices = triangle.vertices.Select(
        vertex => new Vector3((float)vertex.x, GetHeight(vertex.x, vertex.y), (float)vertex.y))
        .Reverse();

      vertices.AddRange(triangleVertices);
    }

    // create mesh
    var mesh = new Mesh();
    mesh.vertices = vertices.ToArray();
    mesh.triangles = vertices.Select((_, index) => index).ToArray();
    mesh.RecalculateNormals();
    return mesh;
  }

  float GetHeight(double x, double y) => GetHeight((float)x, (float)y);

  float GetHeight(float x, float y) =>
    Mathf.PerlinNoise(
      (transform.position.x + x + Time.time * speed) * noiseScale,
      (transform.position.y + y) * noiseScale)
      * waveHeight;

  void UpdateMesh()
  {
    var meshFilter = GetComponent<MeshFilter>();
    meshFilter.sharedMesh.vertices = meshFilter.sharedMesh.vertices.Select(vertex =>
      vertex.y >= 0f ? new Vector3(vertex.x, GetHeight(vertex.x, vertex.z), vertex.z) : vertex
    ).ToArray();
    meshFilter.sharedMesh.RecalculateNormals();
  }
}
