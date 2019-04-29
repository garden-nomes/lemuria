using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

[RequireComponent(typeof(MeshFilter), typeof(Renderer))]
public class EmptyTileMarker : MonoBehaviour
{
  public float radius = 1f;
  public float bottom = -1f;
  public float top = 1f;
  public int textureWidth = 128;
  public Gradient gradient;

  void OnEnable()
  {
    GetComponent<MeshFilter>().sharedMesh = GenerateMesh();
    GetComponent<Renderer>().sharedMaterial.mainTexture = GenerateTexture();
  }

  Mesh GenerateMesh()
  {
    var vertices = new List<Vector3>(36);
    var uvs = new List<Vector2>(36);

    var uvBottom = new Vector2(0f, 0f);
    var uvTop = new Vector2(.99f, 0f);

    for (int i = 0; i < 6; i++)
    {
      var t1 = Mathf.PI * 2f * i / 6f;
      var t2 = Mathf.PI * 2f * (i - 1) / 6f;

      // side
      var p1 = new Vector3(Mathf.Cos(t1) * radius, bottom, Mathf.Sin(t1) * radius);
      var p2 = new Vector3(Mathf.Cos(t2) * radius, bottom, Mathf.Sin(t2) * radius);
      var p3 = new Vector3(Mathf.Cos(t2) * radius, top, Mathf.Sin(t2) * radius);
      var p4 = new Vector3(Mathf.Cos(t1) * radius, top, Mathf.Sin(t1) * radius);
      vertices.Add(p1); uvs.Add(uvBottom);
      vertices.Add(p2); uvs.Add(uvBottom);
      vertices.Add(p3); uvs.Add(uvTop);
      vertices.Add(p1); uvs.Add(uvBottom);
      vertices.Add(p3); uvs.Add(uvTop);
      vertices.Add(p4); uvs.Add(uvTop);

      vertices.Add(p1); uvs.Add(uvBottom);
      vertices.Add(p3); uvs.Add(uvTop);
      vertices.Add(p2); uvs.Add(uvBottom);
      vertices.Add(p1); uvs.Add(uvBottom);
      vertices.Add(p4); uvs.Add(uvTop);
      vertices.Add(p3); uvs.Add(uvTop);
    }

    // create mesh
    var mesh = new Mesh();
    mesh.vertices = vertices.ToArray();
    mesh.triangles = vertices.Select((_, index) => index).ToArray();
    mesh.uv = uvs.ToArray();
    mesh.RecalculateNormals();
    return mesh;
  }

  public Texture2D GenerateTexture()
  {
    var texture = new Texture2D(textureWidth, 1);
    for (var x = 0; x < textureWidth; x++)
    {
      texture.SetPixel(x, 0, gradient.Evaluate((float)x / (float)textureWidth));
    }
    texture.Apply();
    return texture;
  }
}
