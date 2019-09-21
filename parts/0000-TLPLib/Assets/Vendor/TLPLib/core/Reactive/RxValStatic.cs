using System;
using com.tinylabproductions.TLPLib.dispose;
using pzd.lib.reactive;

namespace com.tinylabproductions.TLPLib.Reactive {
  /// <summary>RxVal which has a constant value.</summary>
  class RxValStatic<A> : IRxVal<A> {
    public A value { get; }

    public int subscribers => 0;

    public RxValStatic(A value) { this.value = value; }

    public ISubscription subscribe(
      IDisposableTracker tracker, Action<A> onEvent,
      string callerMemberName = "",
      string callerFilePath = "",
      int callerLineNumber = 0
    ) {
      onEvent(value);
      return Subscription.empty;
    }

    public void subscribe(
      IDisposableTracker tracker, Action<A> onEvent, out ISubscription subscription,
      string callerMemberName = "", string callerFilePath = "", int callerLineNumber = 0
    ) {
      subscription = Subscription.empty;
      onEvent(value);
    }

    public ISubscription subscribeWithoutEmit(
      IDisposableTracker tracker, Action<A> onEvent,
      string callerMemberName = "",
      string callerFilePath = "",
      int callerLineNumber = 0
    ) =>
      Subscription.empty;
  }

  static class RxValStatic {
    public static RxValStatic<A> a<A>(A a) => new RxValStatic<A>(a);
  }
}