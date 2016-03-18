using System;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Concurrent;
using com.tinylabproductions.TLPLib.Functional;
using Smooth.Collections;

namespace com.tinylabproductions.TLPLib.Reactive {
  /**
   * RxRef is a reactive reference, which stores a value and also acts as a IObserver.
   **/
  public interface IRxRef<A> : IRxVal<A>, IObserver<A> {
    new A value { get; set; }
    /** Returns a new ref that is bound to this ref and vice versa. **/
    IRxRef<B> comap<B>(Fn<A, B> mapper, Fn<B, A> comapper);
    IRxVal<A> toVal();
  }

  public static class RxRef {
    public static ObserverBuilder<Elem, IRxRef<Elem>> builder<Elem>(Elem value) {
      return builder => {
        var rxRef = new RxRef<Elem>(value);
        builder(rxRef);
        return rxRef;
      };
    }

    public static IRxRef<A> a<A>(A value) {
      return new RxRef<A>(value);
    }
  }
  
  public class RxRef<A> : Observable<A>, IRxRef<A> {
    A _value;
    public A value { 
      get { return _value; }
      set {
        if (EqComparer<A>.Default.Equals(_value, value)) return;
        _value = value;
        submit(value);
      }
    }

    public RxRef(A initialValue) {
      _value = initialValue;
    }

    public new IRxVal<B> map<B>(Fn<A, B> mapper) {
      return mapImpl(mapper, RxVal.builder(mapper(value)));
    }

    public IRxRef<B> comap<B>(Fn<A, B> mapper, Fn<B, A> comapper) {
      var bRef = mapImpl(mapper, RxRef.builder(mapper(value)));
      bRef.subscribe(b => value = comapper(b));
      return bRef;
    }

    public IRxVal<A> toVal() { return this; }

    public void push(A pushedValue) { value = pushedValue; }

    public override ISubscription subscribe(Act<A> onChange)
      { return subscribe(onChange, emitCurrent: true); }

    public ISubscription subscribe(Act<A> onChange, bool emitCurrent) {
      var subscription = base.subscribe(onChange);
      if (emitCurrent) onChange(value); // Emit current value on subscription.
      return subscription;
    }

    public override string ToString() { return $"RxRef({_value})"; }
  }
}
