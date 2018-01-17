using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.dispose;
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

  /// <summary>
  /// Mutable reference which is also an observable.
  /// </summary>
  public class RxRef<A> : IRxRef<A> {
    public readonly IEqualityComparer<A> comparer;
    
    readonly Subject<A> subject = new Subject<A>();
    public int subscribers => subject.subscribers;

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

    public RxRef(A value, IEqualityComparer<A> comparer = null) {
      this.comparer = comparer ?? EqComparer<A>.Default;
      _value = value;
    }

    public ISubscription subscribe(IDisposableTracker tracker, Act<A> onEvent) {
      var subscription = subject.subscribe(tracker, onEvent);
      onEvent(value);
      return subscription;
    }

    public ISubscription subscribeWithoutEmit(IDisposableTracker tracker, Act<A> onEvent) =>
      subject.subscribe(tracker, onEvent);

    public override string ToString() => $"{nameof(RxRef)}({value})";
    public IRxVal<A> asVal => this;
  }

  public static class RxRef {
    public static IRxRef<A> a<A>(A value) => new RxRef<A>(value);
  }
}