namespace com.tinylabproductions.TLPLib.Reactive {
  /** RxVal which has a constant value. */
  class RxValStatic<A> : IRxVal<A> {
    public A value { get; }

    public int subscribers => 0;

    public RxValStatic(A value) {
      this.value = value;
    }

    public ISubscription subscribe(IObserver<A> observer) => 
      subscribe(observer, RxSubscriptionMode.ForSideEffects);

    public ISubscription subscribe(IObserver<A> observer, RxSubscriptionMode mode) {
      if (mode == RxSubscriptionMode.ForSideEffects) observer.push(value);
      return Subscription.empty;
    }
  }

  static class RxValStatic {
    public static RxValStatic<A> a<A>(A a) => new RxValStatic<A>(a);
  }
}