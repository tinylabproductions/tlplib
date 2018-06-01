using UnityEngine;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Utilities;

namespace com.tinylabproductions.TLPLib.Components.gradient {
  [AddComponentMenu("UI/Effects/Gradient")]
  public class GradientSimple : GradientBase {
    public Color32 topColor = Color.white, bottomColor = Color.black;

    public override void ModifyVertices(List<UIVertex> vertexList) {
      GradientHelper.modifyVertices(vertexList, (c, t) => Color32.Lerp(bottomColor, topColor, t), type);
    }

    public void setAlpha(float alpha) {
      var alpha32 = alpha.remap(0f, 1f, 0f, 255f).toByteClamped();
      topColor = topColor.with32Alpha(alpha32);
      bottomColor = bottomColor.with32Alpha(alpha32);
      OnDidApplyAnimationProperties();
    }
  }
}