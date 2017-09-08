using System;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween {
  public static class Tweener {
    public static Tweener<A, T> a<A, T>(Tween<A> tween, T t, Act<A, T> changeState) =>
      new Tweener<A, T>(tween, t, changeState);

    #region Transform Position

    public static Tweener<Vector3, Transform> tweenPosition(
      this Transform t, Vector3 start, Vector3 to, Ease ease, float duration
    ) => a(
      TweenLerp.vector3.tween(start, to, ease, duration),
      t, TweenMutators.position
    );

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
  }

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