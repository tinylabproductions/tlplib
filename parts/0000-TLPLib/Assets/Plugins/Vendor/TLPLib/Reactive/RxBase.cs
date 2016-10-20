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

    protected RxBase() : base() {}
    protected RxBase(Fn<IObserver<A>, ISubscription> subscribeFn) : base(
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

    #region Ops

    public new IRxVal<B> map<B>(Fn<A, B> mapper) {
      return mapImpl(mapper, RxVal.builder(() => mapper(currentValue)));
    }

    public IRxVal<B> flatMap<B>(Fn<A, IRxVal<B>> mapper) {
      return flatMapImpl(mapper, RxVal.builder(() => mapper(currentValue).value));
    }

    public IRxVal<A> filter(Fn<A, bool> predicate, Fn<A> onFiltered) {
      return map(RxVal.filterMapper(predicate, onFiltered));
    }

    public IRxVal<A> filter(Fn<A, bool> predicate, A onFiltered) {
      return map(RxVal.filterMapper(predicate, onFiltered));
    }

    public IRxVal<Tpl<A, B>> zip<B>(IRxVal<B> ref2) 
    { return zipImpl(ref2, RxVal.builder(() => F.t(currentValue, ref2.value))); }

    public IRxVal<Tpl<A, B, C>> zip<B, C>(IRxVal<B> ref2, IRxVal<C> ref3) 
    { return zipImpl(ref2, ref3, RxVal.builder(() => F.t(currentValue, ref2.value, ref3.value))); }

    public IRxVal<Tpl<A, B, C, D>> zip<B, C, D>(
      IRxVal<B> ref2, IRxVal<C> ref3, IRxVal<D> ref4
    ) { return zipImpl(
      ref2, ref3, ref4, RxVal.builder(() => F.t(currentValue, ref2.value, ref3.value, ref4.value))
    ); }

    public IRxVal<Tpl<A, B, C, D, E>> zip<B, C, D, E>(
      IRxVal<B> ref2, IRxVal<C> ref3, IRxVal<D> ref4, IRxVal<E> ref5
    ) { return zipImpl(
      ref2, ref3, ref4, ref5,
      RxVal.builder(() => F.t(currentValue, ref2.value, ref3.value, ref4.value, ref5.value))
    ); }

    public IRxVal<Tpl<A, A1, A2, A3, A4, A5>> zip<A1, A2, A3, A4, A5>(
      IRxVal<A1> ref2, IRxVal<A2> ref3, IRxVal<A3> ref4, IRxVal<A4> ref5, IRxVal<A5> ref6
    ) { return zipImpl(
      ref2, ref3, ref4, ref5, ref6,
      RxVal.builder(() => F.t(currentValue, ref2.value, ref3.value, ref4.value, ref5.value, ref6.value))
    ); }

    #endregion
  }
}
