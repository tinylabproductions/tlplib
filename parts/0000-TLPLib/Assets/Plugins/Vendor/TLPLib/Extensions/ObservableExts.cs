using System;
using com.tinylabproductions.TLPLib.Reactive;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class ObservableExts {
    public static ISubscription subscribeWhileAlive<A>(this IObservable<A> obs, UnityEngine.Object obj, Act<A> onChange) {
      return obs.subscribe((val, sub) => {
        if (!obj) {
          sub.unsubscribe();
          return;
        }
        onChange(val);
      });
    }
  }
}
