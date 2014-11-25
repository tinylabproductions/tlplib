using System;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Tween {
  public static class TweenUtil {
    struct Destroyable {
      public readonly Act destroy;

      public Destroyable(AbstractGoTween tween) { destroy = tween.destroy; }
      public Destroyable(ITweenFuture tween) { destroy = tween.destroy; }
    }

    private delegate void SetCurrentDestroyable(Destroyable tween);
    public delegate void SetCurrentTween(AbstractGoTween tween);
    public delegate void SetCurrentTweenFuture(ITweenFuture tween);
    public delegate void SetNextTween(Fn<AbstractGoTween> tween);
    public delegate void SetNextFuture(Fn<Future<Unit>> future);

    /* Allows you to have only one running tween per closure. */
    private static A withTweenDestroyable<A>(Fn<SetCurrentDestroyable, A> runTween) {
      var current = F.none<Destroyable>();
      return runTween(tween => {
        current.each(_ => _.destroy());
        current = F.some(tween);
      });
    }

    /* Allows you to have only one running tween per closure. */
    public static A withTween<A>(Fn<SetCurrentTween, A> runTween) {
      return withTweenDestroyable(setDestroyable => 
        runTween(tween => setDestroyable(new Destroyable(tween)))
      );
    }

    /* Allows you to have only one running tween per closure. */
    public static void withTween(Act<SetCurrentTween> runTween) {
      withTween(set => { runTween(set); return F.unit; });
    }

    /* Allows you to have only one running tween per closure. */
    public static A withTweenFuture<A>(Fn<SetCurrentTweenFuture, A> runTween) {
      return withTweenDestroyable(setDestroyable => 
        runTween(tween => setDestroyable(new Destroyable(tween)))
      );
    }

    /* Allows you to have only one running tween per closure.
     * Chains tweens.
     */
    public static A withTweenChained<A>(Fn<SetNextTween, A> runTween) {
      var current = Future.successful(F.unit);
      return runTween(
        f => current = current.flatMap(_ => TweenFuture.a(f()))
      );
    }

    public static A withFutureChained<A>(Fn<SetNextFuture, A> runTween) {
      var current = Future.successful(F.unit);
      return runTween(
        f => current = current.flatMap(_ => f())
      );
    }

    public static void withFutureChained(Act<SetNextFuture> runTween) {
      withFutureChained(set => { runTween(set); return F.unit; });
    }

    /* Allows you to have only one running tween per closure. */
    public static void withTweenFuture(Act<SetCurrentTweenFuture> runTween) {
      withTweenFuture(set => { runTween(set); return F.unit; });
    }

    /* Sets target position to source position every frame, until unbindWhen is completed. */
    public static A bindPosition<A>(
      this A unbindWhen, Fn<Vector3> getSourcePosition, Act<Vector3> setTargetPosition
    ) where A : Future<Unit> {
      ASync.EveryFrame(() => {
        setTargetPosition(getSourcePosition());
        return unbindWhen.value.isEmpty;
      });
      return unbindWhen;
    }

    public static A bindPosition<A>(
      this A unbindWhen, Transform source, Transform target
    ) where A : Future<Unit> {
      return unbindWhen.bindPosition(() => source.position, p => target.position = p);
    }
  }
}
