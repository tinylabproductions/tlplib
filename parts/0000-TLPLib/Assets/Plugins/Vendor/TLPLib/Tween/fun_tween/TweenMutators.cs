using System;
using com.tinylabproductions.TLPLib.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween {
  public static class TweenMutators {
    public static readonly Act<Vector3, Transform>
      position = (v, t) => t.position = v,
      localPosition = (v, t) => t.localPosition = v,
      localScale = (v, t) => t.localScale = v,
      localEulerAngles = (v, t) => t.localEulerAngles = v;
    
    public static readonly Act<Vector2, RectTransform>
      anchoredPosition = (v, t) => t.anchoredPosition = v;

    public static readonly Act<Color, Graphic>
      color = (c, g) => g.color = c;

    public static readonly Act<float, Graphic>
      colorAlpha = (alpha, g) => {
        var c = g.color;
        c.a = alpha;
        g.color = c;
      };
  }
}