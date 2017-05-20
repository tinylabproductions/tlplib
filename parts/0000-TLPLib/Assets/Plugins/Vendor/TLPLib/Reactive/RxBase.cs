using System.Collections.Generic;
using Smooth.Collections;

namespace com.tinylabproductions.TLPLib.Reactive {
  public abstract class RxBase<A> : IObservable<A> {
    public readonly IEqualityComparer<A> comparer;
    protected readonly Subject<A> subject = new Subject<A>();

    A _value;
    public A value {
      get { return _value; }
      set {
        if (!comparer.Equals(_value, value)) {
          _value = value;
          subject.push(value);
        }
      }
    }

    protected RxBase(A value, IEqualityComparer<A> comparer = null) {
      this.comparer = comparer ?? EqComparer<A>.Default;
      _value = value;
    }

    public ISubscription subscribe(IObserver<A> observer) =>
      subscribe(observer, true);

    public virtual ISubscription subscribe(IObserver<A> observer, bool submitCurrentValue) {
      var subscription = subject.subscribe(observer);
      if (submitCurrentValue) observer.push(value);
      return subscription;
    }

    public int subscribers => subject.subscribers;
  }
}