using System;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween {
  public static class Tweener {
    public static Tweener<A, T> a<A, T>(Tween<A> tween, T t, Act<A, T> changeState) =>
      new Tweener<A, T>(tween, t, changeState);

    #region Helpers
    static Tweener<Vector3, Transform> tweenTransformVector(
      this Transform t, Vector3 start, Vector3 to, Ease ease, float duration,
      Act<Vector3, Transform> mutator
    ) => a(TweenLerp.vector3.tween(start, to, ease, duration), t, mutator);
    #endregion

    #region Transform Position
    public static Tweener<Vector3, Transform> tweenPosition(
      this Transform t, Vector3 start, Vector3 to, Ease ease, float duration
    ) => tweenTransformVector(t, start, to, ease, duration, TweenMutators.position);

    public static Tweener<Vector3, Transform> tweenPosition(
      this Transform t, Vector3 to, Ease ease, float duration
    ) => tweenPosition(t, t.position, to, ease, duration);

    public static Tweener<Vector3, Transform> tweenPositionRelative(
      this Transform t, Vector3 to, Ease ease, float duration
    ) => tweenPosition(t, t.position, t.position + to, ease, duration);

    public static Tweener<Vector3, Transform> tweenPositionRelative(
      this Tweener<Vector3, Transform> t, Vector3 to, Ease ease, float duration
    ) => t.t.tweenPosition(t.tween.end, t.tween.end + to, ease, duration);
    #endregion

    #region Transform Scale
    public static Tweener<Vector3, Transform> tweenScale(
      this Transform t, Vector3 from, Vector3 to, Ease ease, float duration
    ) => tweenTransformVector(t, from, to, ease, duration, TweenMutators.localScale);

    public static Tweener<Vector3, Transform> tweenScaleRelative(
      this Transform t, Vector3 to, Ease ease, float duration
    ) => tweenScale(t, t.localScale, to, ease, duration);

    public static Tweener<Vector3, Transform> tweenScaleMultiply(
      this Transform t, float multiplier, Ease ease, float duration
    ) => tweenScale(t, t.localScale, t.localScale * multiplier, ease, duration);
    #endregion

    #region Transform Color
    public static Tweener<Color, Graphic> tweenColor(
      this Graphic g, Color from, Color to, Ease ease, float duration
    ) => a(TweenLerp.color.tween(from, to, ease, duration), g, TweenMutators.color);
    #endregion
  }

  /// <summary>
  /// Knows how to change state of some property <see cref="A"/> on <see cref="T"/>.
  ///
  /// For example how to change <see cref="Vector3"/> of <see cref="Transform.position"/>.
  /// </summary>
  public class Tweener<A, T> : TweenSequenceElement {
    public float duration => tween.duration;

    public readonly Tween<A> tween;
    public readonly T t;
    readonly Act<A, T> changeState;

    public Tweener(Tween<A> tween, T t, Act<A, T> changeState) {
      this.tween = tween;
      this.t = t;
      this.changeState = changeState;
    }

    public void setRelativeTimePassed(float t, bool playingForwards) =>
      changeState(tween.eval(t), this.t);

    public override string ToString() =>
      $"{nameof(Tweener)}[on {t}, {tween}]";
  }
}