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
    ) => tweenTransformVector(t, start, to, ease, duration, TweenMutatorsU.position, relative);

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
    ) => tweenTransformVector(t, start, to, ease, duration, TweenMutatorsU.localPosition);

    [PublicAPI]
    public static Tweener<Vector3, Transform> tweenLocalPositionFrom(
      this Transform t, Vector3 from, Ease ease, float duration
    ) => tweenLocalPosition(t, from, t.localPosition, ease, duration);
    #endregion

    #region Transform Scale
    [PublicAPI]
    public static Tweener<Vector3, Transform> tweenScale(
      this Transform t, Vector3 from, Vector3 to, Ease ease, float duration
    ) => tweenTransformVector(t, from, to, ease, duration, TweenMutatorsU.localScale);

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
    ) {
      var localScale = t.localScale;
      return tweenScale(t, localScale, localScale * multiplier, ease, duration);
    }

    #endregion
    
    #region Transform Rotation
    [PublicAPI]
    public static Tweener<Vector3, Transform> tweenLocalRotation(
      this Transform t, Vector3 from, Vector3 to, Ease ease, float duration
    ) => tweenTransformVector(t, from, to, ease, duration, TweenMutatorsU.localEulerAngles);
    #endregion

    #region Color
    [PublicAPI]
    public static Tweener<Color, Graphic> tweenColor(
      this Graphic g, Color from, Color to, Ease ease, float duration
    ) => a(TweenOps.color.tween(from, to, false, ease, duration), g, TweenMutatorsU.graphicColor);

    [PublicAPI]
    public static Tweener<float, Graphic> tweenColorAlpha(
      this Graphic g, float from, float to, Ease ease, float duration
    ) => a(TweenOps.float_.tween(from, to, false, ease, duration), g, TweenMutatorsU.graphicColorAlpha);
    
    [PublicAPI]
    public static Tweener<Color, Shadow> tweenColor(
      this Shadow s, Color from, Color to, Ease ease, float duration
    ) => a(TweenOps.color.tween(from, to, false, ease, duration), s, TweenMutatorsU.shadowEffectColor);
    #endregion

    #region Image
    [PublicAPI]
    public static Tweener<float, Image> tweenFillAmount(
      this Image i, float from, float to, Ease ease, float duration
    ) => a(TweenOps.float_.tween(from, to, false, ease, duration), i, TweenMutatorsU.imageFillAmount);
    #endregion
    
    #region RectTransform Position
    [PublicAPI]
    public static Tweener<Vector2, RectTransform> tweenAnchoredPosition(
      this RectTransform t, Vector2 start, Vector2 to, Ease ease, float duration
    ) => tweenRectTransformVector(t, start, to, ease, duration, TweenMutatorsU.anchoredPosition);

    [PublicAPI]
    public static Tweener<Vector2, RectTransform> tweenAnchoredPosition(
      this RectTransform t, Vector2 to, Ease ease, float duration
    ) => tweenAnchoredPosition(t, t.anchoredPosition, to, ease, duration);

    [PublicAPI]
    public static Tweener<Vector2, RectTransform> tweenAnchoredPositionRelative(
      this RectTransform t, Vector2 to, Ease ease, float duration
    ) {
      var anchoredPosition = t.anchoredPosition;
      return tweenAnchoredPosition(t, anchoredPosition, anchoredPosition + to, ease, duration);
    }

    [PublicAPI]
    public static Tweener<Vector2, RectTransform> tweenAnchoredPositionRelative(
      this Tweener<Vector2, RectTransform> t, Vector2 to, Ease ease, float duration
    ) => t.t.tweenAnchoredPosition(t.tween.end, t.tween.end + to, ease, duration);
    #endregion
    
    #region Transform Path
    [PublicAPI]
    public static Tweener<float, Transform> tweenTransformByPath(
      this Transform t, float from, float to, Vector3Path path, Ease ease, float duration
      ) => a(TweenOps.float_.tween(from, to, false, ease, duration), t, TweenMutatorsU.path(path));
    #endregion
    
    [PublicAPI]
    public static TweenTimelineElement tweenFloat(
      float from, float to, Ease ease, float duration, Action<float> setValue
    ) => a(TweenOps.float_.tween(from, to, false, ease, duration), F.unit, ((value, target, relative) => setValue(value)));

    [PublicAPI]
    public static Tweener<A, Ref<A>> tweenValue<A>(
      this Ref<A> reference, Tween<A> tween, Func<A, A, A> add
    ) => a(
      tween, reference, 
      (val, @ref, r) => { @ref.value = r ? add(@ref.value, val) : val; }
    );
  }
}