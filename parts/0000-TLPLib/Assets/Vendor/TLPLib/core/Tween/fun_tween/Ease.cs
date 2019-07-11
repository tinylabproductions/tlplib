using com.tinylabproductions.TLPLib.Tween.fun_tween.serialization.eases;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween {
  /// <summary><see cref="Ease"/> is a function from x ∈ [0, 1] to y</summary>
  public delegate float Ease(float x);

  public static class Eases {
    const float HALF_PI = Mathf.PI / 2;
    // ReSharper disable CompareOfFloatsByEqualityOperator

    /// <see cref="SimpleSerializedEase"/>
    // https://gist.github.com/gre/1650294
    // https://github.com/acron0/Easings/blob/master/Easings.cs
    [PublicAPI]
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

    /// <summary>
    /// Punches a Vector3 towards the given direction and then back to the starting one
    /// as if it was connected to the starting position via an elastic.
    /// 
    /// https://github.com/Demigiant/dotween/blob/develop/_DOTween.Assembly/DOTween/DOTween.cs
    /// 
    /// <para>This tween type generates some GC allocations at startup</para>
    /// </summary>
    /// <param name="vibrato">Indicates how much will the punch vibrate</param>
    /// <param name="elasticity">Represents how much (0 to 1) the vector will go beyond the starting position when bouncing backwards.
    /// 1 creates a full oscillation between the direction and the opposite decaying direction,
    /// while 0 oscillates only between the starting position and the decaying direction</param>
    [PublicAPI]
    public static Ease punch(int vibrato = 10, float elasticity = 1f) {
      // ReSharper disable SuggestVarOrType_BuiltInTypes, SuggestVarOrType_Elsewhere
      elasticity = Mathf.Clamp01(elasticity);
      const float direction = 1;
      float strength = direction;
      int totIterations = vibrato;
      if (totIterations < 2) totIterations = 2;
      float decayXTween = strength / totIterations;
      // Calculate and store the duration of each tween
      float[] tDurations = new float[totIterations];
      float sum = 0;
      for (int i = 0; i < totIterations; ++i) {
        float iterationPerc = (i + 1) / (float)totIterations;
        float tDuration = iterationPerc;
        sum += tDuration;
        tDurations[i] = tDuration;
      }
      float tDurationMultiplier = 1f / sum; // Multiplier that allows the sum of tDurations to equal the set duration
      for (int i = 0; i < totIterations; ++i) tDurations[i] = tDurations[i] * tDurationMultiplier;
      // Create the tween
      float[] tos = new float[totIterations];
      tos[0] = direction;
      for (int i = 1; i < totIterations - 1; ++i) {
        if (i % 2 != 0) tos[i] = -Mathf.Clamp(direction, -strength * elasticity, strength * elasticity);
        else tos[i] = Mathf.Clamp(direction, -strength, strength);
        strength -= decayXTween;
      }
      tos[totIterations-1] = 0;

      return p => {
        var idx = 0;
        // This is not an optimal solution.
        // But there is no need to optimize if we don't use this tween a lot.
        while (idx < totIterations - 1 && p > tDurations[idx]) {
          p -= tDurations[idx];
          idx++;
        }
        var from = idx == 0 ? 0 : tos[idx-1];
        var to = tos[idx];
        var ratio = p / tDurations[idx];
        return Mathf.Lerp(from, to, quadOut(ratio));
      };
      // ReSharper restore SuggestVarOrType_BuiltInTypes, SuggestVarOrType_Elsewhere
    }
  }
}
