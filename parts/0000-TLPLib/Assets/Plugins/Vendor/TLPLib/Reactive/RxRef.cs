using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Data;
using Smooth.Collections;

namespace com.tinylabproductions.TLPLib.Reactive {
  /**
   * RxRef is a reactive reference, which stores a value and also acts as a IObservable.
   **/
  public interface IRxRef<A> : Ref<A>, IRxVal<A> {
    new A value { get; set; }
    IRxVal<A> asVal { get; }
  }

  /**
   * Mutable reference which is also an observable.
   **/
  public class RxRef<A> : IRxRef<A> {
    public readonly IEqualityComparer<A> comparer;
    protected readonly Subject<A> subject = new Subject<A>();
    public int subscribers => subject.subscribers;

    A _value;
    public A value {
      get { return _value; }
      set { RxBase.set(comparer, ref _value, value, subject); }
    }

    public RxRef(A value, IEqualityComparer<A> comparer = null) {
      this.comparer = comparer ?? EqComparer<A>.Default;
      _value = value;
    }

    public ISubscription subscribe(IObserver<A> observer) => 
      subscribe(observer, RxSubscriptionMode.ForSideEffects);

    public ISubscription subscribe(IObserver<A> observer, RxSubscriptionMode mode) {
      var subscription = subject.subscribe(observer);
      if (mode == RxSubscriptionMode.ForSideEffects) observer.push(value);
      return subscription;
    }

    public override string ToString() => $"{nameof(RxRef)}({value})";
    public IRxVal<A> asVal => this;
  }

  public static class RxRef {
    public static IRxRef<A> a<A>(A value) => new RxRef<A>(value);
  }
}