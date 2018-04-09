using System;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween {
  public static class TweenMutators {
    public static readonly Act<Vector3, Transform>
      position = (v, t) => t.position = v,
      localPosition = (v, t) => t.localPosition = v,
      localScale = (v, t) => t.localScale = t.localScale = v;
    
    public static readonly Act<Vector2, RectTransform>
      anchoredPosition = (v, t) => t.anchoredPosition = v;

    public static readonly Act<Color, Graphic>
      color = (c, g) => g.color = c;
    
    public static readonly Act<float, Image>
      fillAmount = (f, g) => g.fillAmount = f;
  }
}