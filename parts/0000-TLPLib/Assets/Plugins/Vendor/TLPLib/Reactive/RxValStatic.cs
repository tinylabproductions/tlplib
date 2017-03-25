namespace com.tinylabproductions.TLPLib.Reactive {
  /** RxVal which has a constant value. */
  class RxValStatic<A> : IRxVal<A> {
    public A value { get; }
    public uint valueVersion => 0;

    public int subscribers => 0;
    public bool finished => true;

    public RxValStatic(A value) {
      this.value = value;
    }

    public ISubscription subscribe(IObserver<A> observer) => 
      subscribe(observer, true);

    public ISubscription subscribe(IObserver<A> observer, bool submitCurrentValue) {
      if (submitCurrentValue) observer.push(value);
      observer.finish();
      return Subscription.empty;
    }
  }

  static class RxValStatic {
    public static RxValStatic<A> a<A>(A a) => new RxValStatic<A>(a);
  }
}