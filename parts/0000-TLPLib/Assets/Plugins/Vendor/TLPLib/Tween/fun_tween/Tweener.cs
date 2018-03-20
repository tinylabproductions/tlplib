using System;
using com.tinylabproductions.TLPLib.Data;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween {
  public static class Tweener {
    [PublicAPI]
    public static Tweener<A, T> a<A, T>(Tween<A> tween, T t, Act<A, T> changeState) =>
      new Tweener<A, T>(tween, t, changeState);

    #region Helpers
    static Tweener<Vector3, Transform> tweenTransformVector(
      this Transform t, Vector3 start, Vector3 to, Ease ease, float duration,
      Act<Vector3, Transform> mutator
    ) => a(TweenLerp.vector3.tween(start, to, ease, duration), t, mutator);
    
    static Tweener<Vector2, RectTransform> tweenRectTransformVector(
      this RectTransform t, Vector2 start, Vector2 to, Ease ease, float duration,
      Act<Vector2, RectTransform> mutator
    ) => a(TweenLerp.vector2.tween(start, to, ease, duration), t, mutator);
    #endregion

    #region Transform Position
    [PublicAPI]
    public static Tweener<Vector3, Transform> tweenPosition(
      this Transform t, Vector3 start, Vector3 to, Ease ease, float duration
    ) => tweenTransformVector(t, start, to, ease, duration, TweenMutators.position);

    [PublicAPI]
    public static Tweener<Vector3, Transform> tweenPosition(
      this Transform t, Vector3 to, Ease ease, float duration
    ) => tweenPosition(t, t.position, to, ease, duration);

    [PublicAPI]
    public static Tweener<Vector3, Transform> tweenPositionRelative(
      this Transform t, Vector3 to, Ease ease, float duration
    ) => tweenPosition(t, t.position, t.position + to, ease, duration);

    [PublicAPI]
    public static Tweener<Vector3, Transform> tweenPositionRelative(
      this Tweener<Vector3, Transform> t, Vector3 to, Ease ease, float duration
    ) => t.t.tweenPosition(t.tween.end, t.tween.end + to, ease, duration);
    
    public static Tweener<Vector3, Transform> tweenLocalPosition(
      this Transform t, Vector3 start, Vector3 to, Ease ease, float duration
    ) => tweenTransformVector(t, start, to, ease, duration, TweenMutators.localPosition);
    #endregion

    #region Transform Scale
    [PublicAPI]
    public static Tweener<Vector3, Transform> tweenScale(
      this Transform t, Vector3 from, Vector3 to, Ease ease, float duration
    ) => tweenTransformVector(t, from, to, ease, duration, TweenMutators.localScale);

    [PublicAPI]
    public static Tweener<Vector3, Transform> tweenScaleRelative(
      this Transform t, Vector3 to, Ease ease, float duration
    ) => tweenScale(t, t.localScale, to, ease, duration);

    [PublicAPI]
    public static Tweener<Vector3, Transform> tweenScaleMultiply(
      this Transform t, float multiplier, Ease ease, float duration
    ) => tweenScale(t, t.localScale, t.localScale * multiplier, ease, duration);
    #endregion

    #region Transform Color
    [PublicAPI]
    public static Tweener<Color, Graphic> tweenColor(
      this Graphic g, Color from, Color to, Ease ease, float duration
    ) => a(TweenLerp.color.tween(from, to, ease, duration), g, TweenMutators.color);
    #endregion
    
    #region RectTransform Position
    [PublicAPI]
    public static Tweener<Vector2, RectTransform> tweenAnchoredPosition(
      this RectTransform t, Vector2 start, Vector2 to, Ease ease, float duration
    ) => tweenRectTransformVector(t, start, to, ease, duration, TweenMutators.anchoredPosition);

    [PublicAPI]
    public static Tweener<Vector2, RectTransform> tweenAnchoredPosition(
      this RectTransform t, Vector2 to, Ease ease, float duration
    ) => tweenAnchoredPosition(t, t.anchoredPosition, to, ease, duration);

    [PublicAPI]
    public static Tweener<Vector2, RectTransform> tweenAnchoredPositionRelative(
      this RectTransform t, Vector2 to, Ease ease, float duration
    ) => tweenAnchoredPosition(t, t.anchoredPosition, t.anchoredPosition + to, ease, duration);

    [PublicAPI]
    public static Tweener<Vector2, RectTransform> tweenAnchoredPositionRelative(
      this Tweener<Vector2, RectTransform> t, Vector2 to, Ease ease, float duration
    ) => t.t.tweenAnchoredPosition(t.tween.end, t.tween.end + to, ease, duration);
    #endregion

    [PublicAPI]
    public static Tweener<A, Ref<A>> tweenValue<A>(
      this Ref<A> reference, Tween<A> tween
    ) => a(tween, reference, (val, r) => r.value = val);
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