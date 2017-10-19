using System;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween {
  public static class TweenMutators {
    public static readonly Act<Vector3, Transform>
      position = (v, t) => t.position = v,
      localPosition = (v, t) => t.localPosition = v,
      localScale = (v, t) => t.localScale = t.localScale = v;
  }
}