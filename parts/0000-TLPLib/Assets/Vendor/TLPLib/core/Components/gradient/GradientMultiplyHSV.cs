using UnityEngine;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;

namespace com.tinylabproductions.TLPLib.Components.gradient {
  [AddComponentMenu("UI/Effects/Gradient Multiply HSV")]
  public class GradientMultiplyHSV : GradientBase {
#pragma warning disable 649
    [SerializeField] Color topColor = Color.white, bottomColor = Color.black;
#pragma warning restore 649

    public override void ModifyVertices(List<UIVertex> vertexList) {
      var c1 = topColor.RGBToHSV();
      var c2 = bottomColor.RGBToHSV();
      GradientHelper.modifyVertices(
        vertexList, (c, t) => {
          var a = ((Color) c).RGBToHSV();
          var b = Color.Lerp(c2, c1, t);
          return new Color(a.r + b.r, a.g * b.g, a.b * b.b).HSVToRGB();
        },
        type
      );
    }
  }
}