using System;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Tween.fun_tween.path;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween {
  public static class Tweener {
    [PublicAPI]
    public static Tweener<A, T> a<A, T>(Tween<A> tween, T t, TweenMutator<A, T> changeState) =>
      new Tweener<A, T>(tween, t, changeState);

    #region Helpers

    static Tweener<Vector3, Transform> tweenTransformVector(
      this Transform t, Vector3 start, Vector3 to, Ease ease, float duration,
      TweenMutator<Vector3, Transform> mutator, bool relative = false
    ) => a(TweenOps.vector3.tween(start, to, relative, ease, duration), t, mutator);
    
    static Tweener<Vector2, RectTransform> tweenRectTransformVector(
      this RectTransform t, Vector2 start, Vector2 to, Ease ease, float duration,
      TweenMutator<Vector2, RectTransform> mutator
    ) => a(TweenOps.vector2.tween(start, to, false, ease, duration), t, mutator);
        
    #endregion

    #region Transform Position
    [PublicAPI]
    public static Tweener<Vector3, Transform> tweenPosition(
      this Transform t, Vector3 start, Vector3 to, Ease ease, float duration, bool relative = false
    ) => tweenTransformVector(t, start, to, ease, duration, TweenMutators.position, relative);

    [PublicAPI]
    public static Tweener<Vector3, Transform> tweenPosition(
      this Transform t, Vector3 to, Ease ease, float duration
    ) => tweenPosition(t, t.position, to, ease, duration);

    [PublicAPI]
    public static Tweener<Vector3, Transform> tweenPositionRelative(
      this Transform t, Vector3 to, Ease ease, float duration
    ) => tweenPosition(t, Vector3.zero, to, ease, duration, true);

    [PublicAPI]
    public static Tweener<Vector3, Transform> tweenPositionRelative(
      this Tweener<Vector3, Transform> t, Vector3 to, Ease ease, float duration
    ) => t.t.tweenPosition(t.tween.end, t.tween.end + to, ease, duration);
    
    [PublicAPI]
    public static Tweener<Vector3, Transform> tweenLocalPosition(
      this Transform t, Vector3 start, Vector3 to, Ease ease, float duration
    ) => tweenTransformVector(t, start, to, ease, duration, TweenMutators.localPosition);

    [PublicAPI]
    public static Tweener<Vector3, Transform> tweenLocalPositionFrom(
      this Transform t, Vector3 from, Ease ease, float duration
    ) => tweenLocalPosition(t, from, t.localPosition, ease, duration);
    #endregion

    #region Transform Scale
    [PublicAPI]
    public static Tweener<Vector3, Transform> tweenScale(
      this Transform t, Vector3 from, Vector3 to, Ease ease, float duration
    ) => tweenTransformVector(t, from, to, ease, duration, TweenMutators.localScale);

    [PublicAPI]
    public static Tweener<Vector3, Transform> tweenScaleFrom(
      this Transform t, Vector3 from, Ease ease, float duration
    ) => tweenScale(t, from, t.localScale, ease, duration);

    [PublicAPI]
    public static Tweener<Vector3, Transform> tweenScaleRelative(
      this Transform t, Vector3 to, Ease ease, float duration
    ) => tweenScale(t, t.localScale, to, ease, duration);

    [PublicAPI]
    public static Tweener<Vector3, Transform> tweenScaleMultiply(
      this Transform t, float multiplier, Ease ease, float duration
    ) => tweenScale(t, t.localScale, t.localScale * multiplier, ease, duration);
    #endregion
    
    #region Transform Rotation
    [PublicAPI]
    public static Tweener<Vector3, Transform> tweenLocalRotation(
      this Transform t, Vector3 from, Vector3 to, Ease ease, float duration
    ) => tweenTransformVector(t, from, to, ease, duration, TweenMutators.localEulerAngles);
    #endregion

    #region Color
    [PublicAPI]
    public static Tweener<Color, Graphic> tweenColor(
      this Graphic g, Color from, Color to, Ease ease, float duration
    ) => a(TweenOps.color.tween(from, to, false, ease, duration), g, TweenMutators.graphicColor);

    [PublicAPI]
    public static Tweener<float, Graphic> tweenColorAlpha(
      this Graphic g, float from, float to, Ease ease, float duration
    ) => a(TweenOps.float_.tween(from, to, false, ease, duration), g, TweenMutators.graphicColorAlpha);
    
    [PublicAPI]
    public static Tweener<Color, Shadow> tweenColor(
      this Shadow s, Color from, Color to, Ease ease, float duration
    ) => a(TweenOps.color.tween(from, to, false, ease, duration), s, TweenMutators.shadowEffectColor);
    #endregion

    #region Image
    [PublicAPI]
    public static Tweener<float, Image> tweenFillAmount(
      this Image i, float from, float to, Ease ease, float duration
    ) => a(TweenOps.float_.tween(from, to, false, ease, duration), i, TweenMutators.imageFillAmount);
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
    
    #region Transform Path
    [PublicAPI]
    public static Tweener<float, Transform> tweenTransformByPath(
      this Transform t, float from, float to, Vector3Path path, Ease ease, float duration
      ) => a(TweenOps.float_.tween(from, to, false, ease, duration), t, TweenMutators.path(path));
    #endregion
    
    [PublicAPI]
    public static TweenTimelineElement tweenFloat(
      float from, float to, Ease ease, float duration, Act<float> setValue
    ) => a(TweenOps.float_.tween(from, to, false, ease, duration), F.unit, ((value, target, relative) => setValue(value)));

    [PublicAPI]
    public static Tweener<A, Ref<A>> tweenValue<A>(
      this Ref<A> reference, Tween<A> tween, Fn<A, A, A> add
    ) => a(
      tween, reference, 
      (val, @ref, r) => { @ref.value = r ? add(@ref.value, val) : val; }
    );
  }

  /// <summary>
  /// Knows how to change state of some property <see cref="A"/> on <see cref="T"/>.
  ///
  /// For example how to change <see cref="Vector3"/> of <see cref="Transform.position"/>.
  /// </summary>
  public class Tweener<A, T> : TweenTimelineElement, IApplyStateAt {
    [PublicAPI] public float duration => tween.duration;

    [PublicAPI] public readonly Tween<A> tween;
    [PublicAPI] public readonly T t;
    [PublicAPI] public readonly TweenMutator<A, T> changeState;

    public Tweener(Tween<A> tween, T t, TweenMutator<A, T> changeState) {
      this.tween = tween;
      this.t = t;
      this.changeState = changeState;
    }

    public void setRelativeTimePassed(
      float previousTimePassed, float timePassed, bool playingForwards, bool applyEffectsForRelativeTweens
    ) {
      if (applyEffectsForRelativeTweens || !tween.isRelative) {
        changeState(tween.eval(previousTimePassed, timePassed, playingForwards), t, tween.isRelative);
      }
    }

    public bool asApplyStateAt(out IApplyStateAt applyStateAt) {
      applyStateAt = this;
      return true;
    }

    public void applyStateAt(float time) {
      // We do not apply relative tween states, because they do not make sense in a fixed time point.
      if (!tween.isRelative) {
        changeState(tween.evalAt(time), t, false);
      }
    }

    public override string ToString() {
      var relativeS = tween.isRelative ? "relative " : "";
      return $"{nameof(Tweener)}[{relativeS}on {t}, {tween}]";
    }
  }
}