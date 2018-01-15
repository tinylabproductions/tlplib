using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween {
  /// <summary><see cref="Ease"/> is a function from x ∈ [0, 1] to y</summary>
  public delegate float Ease(float x);

  public static class Eases {
    const float HALF_PI = Mathf.PI / 2;
    // ReSharper disable CompareOfFloatsByEqualityOperator

    // https://gist.github.com/gre/1650294
    // https://github.com/acron0/Easings/blob/master/Easings.cs
    public static readonly Ease
      linear = p => p,
      quadIn = p => p * p,
      quadOut = p => p * (2 - p),
      quadInOut = p => p < .5f ? 2 * p * p : -1 + (4 - 2 * p) * p,
      cubicIn = p => p * p * p,
      cubicOut = p => (--p) * p * p + 1,
      cubicInOut = p => p < .5f ? 4 * p * p * p : (p - 1) * (2 * p - 2) * (2 * p - 2) + 1,
      quartIn = p => p * p * p * p,
      quartOut = p => 1 - (--p) * p * p * p,
      quartInOut = p => p < .5f ? 8 * p * p * p * p : 1 - 8 * (--p) * p * p * p,
      quintIn = p => p * p * p * p * p,
      quintOut = p => 1 + (--p) * p * p * p * p,
      quintInOut = p => p < .5f ? 16 * p * p * p * p * p : 1 + 16 * (--p) * p * p * p * p,
      sineIn = p => Mathf.Sin((p - 1) * HALF_PI) + 1,
      sineOut = p => Mathf.Sin(p * HALF_PI),
      sineInOut = p => .5f * (1 - Mathf.Cos(p * Mathf.PI)),
      circularIn = p => 1 - Mathf.Sqrt(1 - (p * p)),
      circularOut = p => Mathf.Sqrt((2 - p) * p),
      circularInOut = p => .5f * (p < .5
                             ? (1 - Mathf.Sqrt(1 - 4 * p * p))
                             : (Mathf.Sqrt(-(2 * p - 3) * (2 * p - 1)) + 1)),
      expoIn = p => (p == 0) ? p : Mathf.Pow(2, 10 * (p - 1)),
      expoOut = p => (p == 1) ? p : 1 - Mathf.Pow(2, -10 * p),
      expoInOut = p => {
        if (p == 0 || p == 1) return p;
        if (p < 0.5f) {
          return 0.5f * Mathf.Pow(2, (20 * p) - 10);
        }
        return -0.5f * Mathf.Pow(2, (-20 * p) + 10) + 1;
      },
      elasticIn = p => Mathf.Sin(13 * HALF_PI * p) * Mathf.Pow(2, 10 * (p - 1)),
      elasticOut = p => Mathf.Sin(-13 * HALF_PI * (p + 1)) * Mathf.Pow(2, -10 * p) + 1,
      elasticInOut = p => {
        if (p < 0.5f) {
          return 0.5f * Mathf.Sin(13 * HALF_PI * (2 * p)) * Mathf.Pow(2, 10 * ((2 * p) - 1));
        }
        return 0.5f * (Mathf.Sin(-13 * HALF_PI * ((2 * p - 1) + 1)) * Mathf.Pow(2, -10 * (2 * p - 1)) + 2);
      },
      backIn = p => p * p * p - p * Mathf.Sin(p * Mathf.PI),
      backOut = p => {
        var f = (1 - p);
        return 1 - (f * f * f - f * Mathf.Sin(f * Mathf.PI));
      },
      backInOut = p => {
        if (p < 0.5f) {
          var f = 2 * p;
          return 0.5f * (f * f * f - f * Mathf.Sin(f * Mathf.PI));
        }
        else {
          var f = (1 - (2 * p - 1));
          return 0.5f * (1 - (f * f * f - f * Mathf.Sin(f * Mathf.PI))) + 0.5f;
        }
      },
      bounceIn = p => 1 - bounceOut(1 - p),
      bounceOut = p => {
        if (p < 4 / 11.0f) {
          return (121 * p * p) / 16.0f;
        }
        if (p < 8 / 11.0f) {
          return (363 / 40.0f * p * p) - (99 / 10.0f * p) + 17 / 5.0f;
        }
        if (p < 9 / 10.0f) {
          return (4356 / 361.0f * p * p) - (35442 / 1805.0f * p) + 16061 / 1805.0f;
        }
        return (54 / 5.0f * p * p) - (513 / 25.0f * p) + 268 / 25.0f;
      },
      bounceInOut = p => p < .5f ? 0.5f * bounceIn(p * 2) : 0.5f * bounceOut(p * 2 - 1) + 0.5f;
    // ReSharper restore CompareOfFloatsByEqualityOperator
  }
}
