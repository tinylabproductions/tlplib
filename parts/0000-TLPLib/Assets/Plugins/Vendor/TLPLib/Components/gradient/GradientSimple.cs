using UnityEngine;
using System.Collections.Generic;

namespace com.tinylabproductions.TLPLib.Components.gradient {
  [AddComponentMenu("UI/Effects/Gradient")]
  public class GradientSimple : GradientBase {
    public Color32 topColor = Color.white, bottomColor = Color.black;

    public override void ModifyVertices(List<UIVertex> vertexList) {
      GradientHelper.modifyVertices(vertexList, (c, t) => Color32.Lerp(bottomColor, topColor, t), type);
    }
  }
}