using UnityEngine;
using System.Collections.Generic;

namespace com.tinylabproductions.TLPLib.Components.gradient {
  [AddComponentMenu("UI/Effects/Gradient Multiply")]
  public class GradientMultiply : GradientBase {
    public Color32 topColor = Color.white, bottomColor = Color.black;

    public override void ModifyVertices(List<UIVertex> vertexList) =>
      GradientHelper.modifyVertices(
        vertexList, (c, t) => mult(c, Color32.Lerp(bottomColor, topColor, t)), type, useGraphicAlpha
      );

    static Color32 mult(Color32 a, Color32 b) => new Color32(
      (byte) (a.r * b.r / 256),
      (byte) (a.g * b.g / 256),
      (byte) (a.b * b.b / 256),
      (byte) (a.a * b.a / 256)
    );
  }
}