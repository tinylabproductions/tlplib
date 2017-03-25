using System.Collections.Generic;
using Smooth.Collections;

namespace com.tinylabproductions.TLPLib.Reactive {
  public abstract class RxBase<A> : Observable<A> {
    static readonly IEqualityComparer<A> comparer = EqComparer<A>.Default;

    A _value;
    public uint valueVersion { get; private set; }

    // virtual here, because children might be doing something smarter with the value
    protected virtual A value {
      get { return _value; }
      set {
        _value = value;
        valueVersion++;
      }
    }

    protected RxBase(A value) {
      _value = value;
    }

    protected RxBase(A value, SubscribeToSource<A> subscribeFn) : base(subscribeFn) {
      _value = value;
    }

    protected override void submit(A a) {
      // Use our local copy and not value accessor here, because we need to check 
      // whether an update has happened and accessing value accessor might do other side
      // effects, like updating _value.
      if (!comparer.Equals(_value, a)) {
        if (!iterating) value = a;
        base.submit(a);
      }
    }

    public override ISubscription subscribe(IObserver<A> observer) => 
      subscribe(observer, true);

    public ISubscription subscribe(IObserver<A> observer, bool submitCurrentValue) {
      var subscription = base.subscribe(observer);
      if (submitCurrentValue) observer.push(value);
      return subscription;
    }
  }
}