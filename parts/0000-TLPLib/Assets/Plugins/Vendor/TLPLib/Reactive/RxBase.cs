using System.Collections.Generic;
using Smooth.Collections;

namespace com.tinylabproductions.TLPLib.Reactive {
  public abstract class RxBase<A> : Observable<A> {
    static readonly IEqualityComparer<A> comparer = EqComparer<A>.Default;

    protected A _value;
    public uint valueVersion { get; private set; }
    // Hack to allow declaring get only #value in RxVal & get/set #value in RxRef
    protected abstract A currentValue { get; }

    protected RxBase() {}
    protected RxBase(SubscribeFn<A> subscribeFn) : base(subscribeFn) {}

    protected override void submit(A value) {
      if (!comparer.Equals(_value, value)) {
        if (!iterating) {
          _value = value;
          valueVersion++;
        }
        base.submit(value);
      }
    }

    public override ISubscription subscribe(IObserver<A> observer) {
      var subscription = base.subscribe(observer);
      observer.push(currentValue); // Emit current value on subscription.
      return subscription;
    }
  }
}
