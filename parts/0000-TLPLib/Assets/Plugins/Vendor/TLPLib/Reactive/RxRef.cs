using System;
using com.tinylabproductions.TLPLib.Data;

namespace com.tinylabproductions.TLPLib.Reactive {
  /**
   * RxRef is a reactive reference, which stores a value and also acts as a IObserver.
   **/
  public interface IRxRef<A> : Ref<A>, IRxVal<A> {
    new A value { get; set; }
    IRxVal<A> asVal { get; }
  }

  /**
   * Mutable reference which is also an observable.
   **/
  public class RxRef<A> : RxBase<A>, IRxRef<A> {
    protected override A currentValue => _value;

    public A value {
      get { return currentValue; }
      set { submit(value); }
    }

    public RxRef(A initialValue) { _value = initialValue; }

    public override string ToString() => $"RxRef({_value})";

    public IRxVal<A> asVal => this;
  }

  public static class RxRef {
    public static IRxRef<A> a<A>(A value) => new RxRef<A>(value);
  }

  public static class RxRefOps {
    /** Returns a new ref that is bound to this ref and vice versa. **/
    public static IRxRef<B> comap<A, B>(this IRxRef<A> rx, Fn<A, B> mapper, Fn<B, A> comapper) {
      var bRef = RxRef.a(mapper(rx.value));
      bRef.subscribe(b => rx.value = comapper(b));
      return bRef;
    }
  }
}