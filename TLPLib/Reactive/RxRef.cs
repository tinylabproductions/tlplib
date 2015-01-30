using System;

namespace com.tinylabproductions.TLPLib.Reactive {
  /**
   * RxRef is a reactive reference, which stores a value and also acts as a IObserver.
   **/
  public interface IRxRef<A> : IRxVal<A> {
    new A value { get; set; }
    /** Returns a new ref that is bound to this ref and vice versa. **/
    IRxRef<B> comap<B>(Fn<A, B> mapper, Fn<B, A> comapper);
    IRxVal<A> asVal { get; }
  }

  /**
   * Mutable reference which is also an observable.
   **/
  public class RxRef<A> : RxBase<A>, IRxRef<A> {
    protected override A currentValue { get { return _value; } }

    public A value {
      get { return currentValue; }
      set { submit(value); }
    }

    public RxRef(A initialValue) : base() { _value = initialValue; }

    #region IRxRef ops

    public IRxRef<B> comap<B>(Fn<A, B> mapper, Fn<B, A> comapper) {
      var bRef = RxRef.a(mapper(value));
      bRef.subscribe(b => value = comapper(b));
      return bRef;
    }

    public IRxVal<A> asVal { get { return this; } }

    #endregion
  }

  public static class RxRef {
    public static IRxRef<A> a<A>(A value) { return new RxRef<A>(value); }
  }
}
