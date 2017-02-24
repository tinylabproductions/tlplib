using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Functional;
using Smooth.Collections;

namespace com.tinylabproductions.TLPLib.Reactive {
  public abstract class RxBase<A> : Observable<A> {
    static readonly IEqualityComparer<A> comparer = EqComparer<A>.Default;

    protected A _value;
    // Hack to allow declaring get only #value in RxVal & get/set #value in RxRef
    protected abstract A currentValue { get; }

    protected RxBase() {}
    protected RxBase(SubscribeFn<A> subscribeFn) : base(
      subscribeFn, 
      // We need to be always subscribed, because we always need to have current value
      // in this rx value.
      beAlwaysSubscribed: true
    ) {}

    protected override void submit(A value) {
      if (!comparer.Equals(_value, value)) {
        if (!iterating) _value = value;
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
