using com.tinylabproductions.TLPLib.dispose;
using com.tinylabproductions.TLPLib.Reactive;
using JetBrains.Annotations;
using UnityEngine.Events;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class UnityEventExts {
    [PublicAPI] public static ISubscription subscribe<A>(this UnityEvent<A> evt, UnityAction<A> act) {
      evt.AddListener(act);
      return new Subscription(() => evt.RemoveListener(act));
    }

    [PublicAPI] public static IRxVal<A> toRxVal<A>(this UnityEvent<A> evt, IDisposableTracker tracker, A initial) {
      var rx = RxRef.a(initial);
      tracker.track(evt.subscribe(a => rx.value = a));
      return rx;
    }
  }
}