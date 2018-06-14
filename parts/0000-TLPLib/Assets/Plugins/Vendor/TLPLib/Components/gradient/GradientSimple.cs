using UnityEngine;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;
using JetBrains.Annotations;

namespace com.tinylabproductions.TLPLib.Components.gradient {
  [AddComponentMenu("UI/Effects/Gradient")]
  public class GradientSimple : GradientBase {
    public Color32 topColor = Color.white, bottomColor = Color.black;

    public override void ModifyVertices(List<UIVertex> vertexList) {
      GradientHelper.modifyVertices(vertexList, (c, t) => Color32.Lerp(bottomColor, topColor, t), type);
    }

    [PublicAPI]
    public void setAlpha(float alpha) {
      var alpha32 = Mathf.Lerp(0, 255, alpha).roundToByteClamped();
      topColor.a = alpha32;
      bottomColor.a = alpha32;
      OnDidApplyAnimationProperties();
    }
  }
}