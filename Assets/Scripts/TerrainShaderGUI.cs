using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TerrainShaderGUI : ShaderGUI
{
  Gradient gradient = new Gradient();

  public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
  {
    var newGradient = EditorGUILayout.GradientField("Height Gradient", gradient);

    if (!newGradient.Equals(gradient))
    {
      gradient = newGradient;
      ((Material)materialEditor.target).SetTexture("_HeightRamp", GenerateGradientTexture(256));
    }

    base.OnGUI(materialEditor, properties);
  }

  Texture2D GenerateGradientTexture(int width)
  {
    var tex = new Texture2D(width, 1);

    for (int x = 0; x < width; x++)
    {
      var color = gradient.Evaluate((float)x / (float)width);
      tex.SetPixel(x, 0, color);
    }
    tex.Apply();

    return tex;
  }
}
