using System;
using com.tinylabproductions.TLPLib.Functional;
using pzd.lib.collection;
using pzd.lib.concurrent;
using pzd.lib.reactive;
using pzdf = pzd.lib.functional;
using None = pzd.lib.functional.None;

namespace com.tinylabproductions.TLPLib.Concurrent {
  public static class IHeapFutureExts {
    public static Future<A> asFuture<A>(this IHeapFuture<A> f) => Future.a(f);
  }

  class FutureImpl<A> : IHeapFuture<A>, Promise<A> {
    // type optimized for il2cpp
    object[] listeners = EmptyArray<object>._;
    uint listenersCount;
    
    bool iterating;

    public bool isCompleted => value.isSome;
    public pzdf.Option<A> value { get; private set; } = pzdf.Option<A>.None;
    public bool valueOut(out A a) => value.valueOut(out a);

    public override string ToString() => $"{nameof(FutureImpl<A>)}({value})";

    public void complete(A v) {
      if (! tryComplete(v)) throw new IllegalStateException(
        $"Trying to complete future with \"{v}\" but it is already completed with \"{value.__unsafeGet}\""
      );
    }

    public bool tryComplete(A v) {
      // Cannot use fold here because of iOS AOT.
      var ret = value.isNone;
      if (ret) {
        value = F.some(v);
        // completed should be called only once
        completed(v);
      }
      return ret;
    }

    public ISubscription onComplete(Action<A> action) {
      if (value.isSome) {
        action(value.__unsafeGet);
        return Subscription.empty;
      }
      else {
        AList.add(ref listeners, ref listenersCount, action);
        return new Subscription(() => {
          if (iterating || listeners == null) return;
          AList.removeReplacingWithLast(listeners, ref listenersCount, action);
        });
      }
    }

    void completed(A v) {
      iterating = true;
      for (var idx = 0u; idx < listenersCount; idx++) ((Action<A>)listeners[idx])(v);
      AList.clear(listeners, ref listenersCount);
      listeners = null;
    }
  }
}
