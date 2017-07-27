using System;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween.fun_tween {
  public static class Tweener {
    public static Tweener<A> a<A>(Tween<A> tween, Act<A> changeState) =>
      new Tweener<A>(tween, changeState);

    public static Tweener<Vector3> tweenPosition(
      this Transform t, Vector3 start, Vector3 to, Ease ease, float duration
    ) => a(
      TweenLerp.vector3.tween(start, to, ease, duration),
      TweenMutators.position(t)
    );

    public static Tweener<Vector3> tweenPosition(
      this Transform t, Vector3 to, Ease ease, float duration
    ) => a(
      TweenLerp.vector3.tween(t.position, to, ease, duration),
      TweenMutators.position(t)
    );

    public static Tweener<Vector3> tweenPositionRelative(
      this Transform t, Vector3 to, Ease ease, float duration
    ) => a(
      TweenLerp.vector3.tween(t.position, t.position + to, ease, duration),
      TweenMutators.position(t)
    );
  }

  public class Tweener<A> : TweenSequenceElement {
    public float duration => tween.duration;

    readonly Tween<A> tween;
    readonly Act<A> changeState;

    public Tweener(Tween<A> tween, Act<A> changeState) {
      this.tween = tween;
      this.changeState = changeState;
    }

    public void setRelativeTimePassed(float t) => 
      changeState(tween.eval(t / duration));
  }
}