using System;
using com.tinylabproductions.TLPLib.Components.dispose;
using com.tinylabproductions.TLPLib.Reactive;
using UnityEngine;

namespace com.tinylabproductions.TLPLib.Extensions {
  public static class ObservableExts {
    [Obsolete]
    public static ISubscription subscribeWhileAlive<A>(
      this IObservable<A> obs, GameObject obj, Act<A> onChange
    ) => obs.subscribe(obj.asDisposableTracker(), onChange);
  }
}
