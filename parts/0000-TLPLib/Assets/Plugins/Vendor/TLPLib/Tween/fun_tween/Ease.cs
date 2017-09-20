using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween {
  public delegate float Ease(float x);
  public static class Eases {
    const float HALF_PI = Mathf.PI / 2;

    public static readonly Ease
      linear = x => x,
      quadratic = x => x * x,
      cubic = x => x * x * x,
      sin = x => Mathf.Sin((x - 1) * HALF_PI) + 1;
  }
}