using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween {
  /// <summary><see cref="Ease"/> is a function from x ∈ [0, 1] to y</summary>
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