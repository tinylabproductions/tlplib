using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween {
  public interface TweenNumeric<A> {
    A add(A a1, A a2);
    A subtract(A a1, A a2);
    A multiply(A a1, float y);
  }

  /// <summary>Knows how to linearly interpolate <see cref="A"/>. Should return start when y = 0 and end when y = 1.</summary>
  public delegate A TweenLerp<A>(A start, A end, float y);
  public static class TweenLerp {
    public static readonly TweenLerp<float> float_ = Mathf.LerpUnclamped;
    public static readonly TweenLerp<Vector2> vector2 = Vector2.LerpUnclamped;
    public static readonly TweenLerp<Vector3> vector3 = Vector3.LerpUnclamped;
    public static readonly TweenLerp<Quaternion> quaternion = Quaternion.LerpUnclamped;
    public static readonly TweenLerp<Color> color = Color.LerpUnclamped;

    public static TweenLerp<A> fromNumeric<A>(TweenNumeric<A> n) =>
      (start, end, y) => n.add(start, n.multiply(n.subtract(end, start), y));

    public static Tween<A> tween<A>(
      this TweenLerp<A> lerp, A start, A end, Ease ease, float duration
    ) => new Tween<A>(start, end, ease, lerp, duration);
  }
}