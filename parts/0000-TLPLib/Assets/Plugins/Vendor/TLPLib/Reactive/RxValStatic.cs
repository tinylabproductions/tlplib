using System;
using com.tinylabproductions.TLPLib.dispose;

namespace com.tinylabproductions.TLPLib.Reactive {
  /// <summary>RxVal which has a constant value.</summary>
  class RxValStatic<A> : IRxVal<A> {
    public A value { get; }

    public int subscribers => 0;

    public RxValStatic(A value) { this.value = value; }

    public ISubscription subscribe(
      IDisposableTracker tracker, Act<A> onEvent,
      string callerMemberName = "",
      string callerFilePath = "",
      int callerLineNumber = 0
    ) {
      onEvent(value);
      return Subscription.empty;
    }

    public ISubscription subscribeWithoutEmit(
      IDisposableTracker tracker, Act<A> onEvent,
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