using System;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using com.tinylabproductions.TLPLib.Reactive;

namespace com.tinylabproductions.TLPLib.binding {
  public static class Unity {
    public static ISubscription bind<A>(
      this IObservable<A> observable, Fn<A, Coroutine> f
    ) {
      var lastCoroutine = F.none<Coroutine>();
      Action stopOpt = () => { foreach (var c in lastCoroutine) { c.stop(); } };
      var sub = observable.subscribe(a => {
        stopOpt();
        lastCoroutine = f(a).some();
      });
      return sub.andThen(stopOpt);
    }
  }
}