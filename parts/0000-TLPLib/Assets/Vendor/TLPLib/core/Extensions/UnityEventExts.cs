using System;
using com.tinylabproductions.TLPLib.Reactive;
using JetBrains.Annotations;
using pzd.lib.dispose;
using pzd.lib.reactive;
using UnityEngine.Events;

namespace com.tinylabproductions.TLPLib.Extensions {
  [PublicAPI] public static class UnityEventExts {
    [Obsolete("Please use variant with explicit tracker passed")]
    public static ISubscription subscribe<A>(this UnityEvent<A> evt, UnityAction<A> act) {
      evt.AddListener(act);
      return new Subscription(() => evt.RemoveListener(act));
    }
    
    public static ISubscription subscribe<A>(this UnityEvent<A> evt, IDisposableTracker tracker, UnityAction<A> act) {
      evt.AddListener(act);
      var sub = new Subscription(() => evt.RemoveListener(act));
      tracker.track(sub);
      return sub;
    }

    public static IRxObservable<A> toObservable<A>(this UnityEvent<A> evt) =>
      new Observable<A>(push => {
        var action = new UnityAction<A>(a => push(a));
        evt.AddListener(action);
        return new Subscription(() => evt.RemoveListener(action));
      });

    public static IRxVal<A> toRxVal<A>(this UnityEvent<A> evt, IDisposableTracker tracker, A initial) {
      var rx = RxRef.a(initial);
      evt.subscribe(tracker,a => rx.value = a);
      return rx;
    }
  }
}