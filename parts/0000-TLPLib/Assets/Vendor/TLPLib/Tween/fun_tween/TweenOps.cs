using System;
using JetBrains.Annotations;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween {
  public static class TweenOps {
    [PublicAPI]
    public static readonly Tween<float>.Ops float_ = Tween.ops<float>(Mathf.LerpUnclamped, (a1, a2) => a1 - a2);
    [PublicAPI]
    public static readonly Tween<Vector2>.Ops vector2 = Tween.ops<Vector2>(Vector2.LerpUnclamped, (a1, a2) => a1 - a2);
    [PublicAPI]
    public static readonly Tween<Vector3>.Ops vector3 = Tween.ops<Vector3>(Vector3.LerpUnclamped, (a1, a2) => a1 - a2);
    [PublicAPI]
    public static readonly Tween<Quaternion>.Ops quaternion = 
      Tween.ops<Quaternion>(
        Quaternion.LerpUnclamped,
        // https://forum.unity.com/threads/subtracting-quaternions.317649/
        (a1, a2) => a1 * Quaternion.Inverse(a2)
      );
    [PublicAPI]
    public static readonly Tween<Color>.Ops color = Tween.ops<Color>(Color.LerpUnclamped, (a1, a2) => a1 - a2);

    [PublicAPI] public static readonly Tween<int>.Ops intRounded, intFloored, intCeiled;

    static TweenOps() {
      int intDiff(int a1, int a2) => a1 - a2;

      Tween<int>.Ops opsFor(Func<float, int> fToI) => Tween.ops<int>(
        (start, end, t) => fToI(float_.lerp(start, end, t)),
        intDiff
      );

      intRounded = opsFor(Mathf.RoundToInt);
      intFloored = opsFor(Mathf.FloorToInt);
      intCeiled = opsFor(Mathf.CeilToInt);
    }

    [PublicAPI]
    public static Tween<A>.Ops fromNumeric<A>(Numeric<A> n) => Tween.ops<A>(
      (start, end, y) => n.add(start, n.multiply(n.subtract(end, start), y)),
      n.subtract
    );

    [PublicAPI]
    public static Tween<A> tween<A>(
      this Tween<A>.Ops ops, A start, A end, bool isRelative, Ease ease, float duration
    ) => new Tween<A>(start, end, isRelative, ease, ops, duration);
  
    public interface Numeric<A> {
      A add(A a1, A a2);
      A subtract(A a1, A a2);
      A multiply(A a1, float y);
    }
  }
}