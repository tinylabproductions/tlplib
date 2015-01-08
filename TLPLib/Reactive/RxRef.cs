using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;
using Smooth.Collections;

namespace com.tinylabproductions.TLPLib.Reactive {
  /**
   * RxVal is an observable which has a current value.
   **/
  public interface IRxVal<A> : IObservable<A> {
    A value { get; }
    new IRxVal<B> map<B>(Fn<A, B> mapper);
    IRxVal<B> flatMap<B>(Fn<A, IRxVal<B>> mapper);
    IRxVal<Tpl<A, B>> zip<B>(IRxVal<B> ref2);
    IRxVal<Tpl<A, B, C>> zip<B, C>(IRxVal<B> ref2, IRxVal<C> ref3);
    IRxVal<Tpl<A, B, C, D>> zip<B, C, D>(IRxVal<B> ref2, IRxVal<C> ref3, IRxVal<D> ref4);
    IRxVal<Tpl<A, B, C, D, E>> zip<B, C, D, E>(IRxVal<B> ref2, IRxVal<C> ref3, IRxVal<D> ref4, IRxVal<E> ref5);
  }

  /**
   * RxRef is a reactive reference, which stores a value and also acts as a IObserver.
   **/
  public interface IRxRef<A> : IRxVal<A> {
    new A value { get; set; }
    /** Returns a new ref that is bound to this ref and vice versa. **/
    IRxRef<B> comap<B>(Fn<A, B> mapper, Fn<B, A> comapper);
  }

  /* RxRef for mutable values. Cannot be changed, because the object itself is mutable. */
  public interface IRxMutRef<A> : IRxVal<A> {
    /* Execute change function and notify subscribers about the change. */
    A change(Act<A> change);
    /* Notify subscribers that the value inside has mutated. */
    void changed();
  }

  public static class RxVal {
    public static ObserverBuilder<Elem, IRxVal<Elem>> builder<Elem>(Elem value) {
      return RxRef.builder(value);
    }

    public static IRxVal<A> a<A>(A value) { return RxRef.a(value); }
    public static IRxVal<A> cached<A>(A value) { return RxValCache<A>.get(value); }
  }

  static class RxValCache<A> {
    static readonly Dictionary<A, IRxVal<A>> staticCache = new Dictionary<A, IRxVal<A>>();

    public static IRxVal<A> get(A value) {
      return staticCache.get(value).getOrElse(() => {
        var cached = (IRxVal<A>) RxRef.a(value);
        staticCache.Add(value, cached);
        return cached;
      });
    }
  }

  public static class RxRef {
    public static ObserverBuilder<Elem, IRxRef<Elem>> builder<Elem>(Elem value) {
      return subscriptionFn => {
        var rxRef = a(value);
        subscriptionFn(new Observer<Elem>(v => rxRef.value = v));
        return rxRef;
      };
    }

    public static IRxRef<A> a<A>(A value) {
      return new RxRef<A>(value);
    }

    public static IRxRef<A> a<A>(A value, IEqualityComparer<A> comparer) {
      return new RxRef<A>(value, comparer);
    }
  }

  public abstract class RxRefBase<A> : Observable<A> {
    protected A _value;
    public A value { get { return _value; } }

    protected RxRefBase(A initialValue) { _value = initialValue; }

    public new IRxVal<B> map<B>(Fn<A, B> mapper) {
      return mapImpl(mapper, RxVal.builder(mapper(value)));
    }

    public IRxVal<B> flatMap<B>(Fn<A, IRxVal<B>> mapper) {
      return flatMapImpl(mapper, RxVal.builder(mapper(value).value));
    }

    public override ISubscription subscribe(IObserver<A> observer) {
      var subscription = base.subscribe(observer);
      observer.push(value); // Emit current value on subscription.
      return subscription;
    }

    public IRxVal<Tpl<A, B>> zip<B>(IRxVal<B> ref2) 
    { return zipImpl(ref2, RxVal.builder(F.t(value, ref2.value))); }

    public IRxVal<Tpl<A, B, C>> zip<B, C>(IRxVal<B> ref2, IRxVal<C> ref3) 
    { return zipImpl(ref2, ref3, RxVal.builder(F.t(value, ref2.value, ref3.value))); }

    public IRxVal<Tpl<A, B, C, D>> zip<B, C, D>(
      IRxVal<B> ref2, IRxVal<C> ref3, IRxVal<D> ref4
    ) { return zipImpl(
      ref2, ref3, ref4, RxVal.builder(F.t(value, ref2.value, ref3.value, ref4.value))
    ); }

    public IRxVal<Tpl<A, B, C, D, E>> zip<B, C, D, E>(
      IRxVal<B> ref2, IRxVal<C> ref3, IRxVal<D> ref4, IRxVal<E> ref5
    ) { return zipImpl(
      ref2, ref3, ref4, ref5, 
      RxVal.builder(F.t(value, ref2.value, ref3.value, ref4.value, ref5.value))
    ); }
  }

  /**
   * Mutable reference which is also an observable.
   * 
   * Notes:
   * 
   * * Beware that RxRef#value setter does not change the value immediately 
   *   if you do it from a subscription to this observable. The value is only
   *   changed when the value broadcast for current subscriber list is complete.
   *   TODO: test this
   * 
   **/
  public class RxRef<A> : RxRefBase<A>, IRxRef<A> {
    private static readonly IEqualityComparer<A> defaultComparer = EqComparer<A>.Default;
    private readonly IEqualityComparer<A> comparer;

    public new A value { 
      get { return _value; }
      set { if (! comparer.Equals(_value, value)) submit(value); }
    }

    public RxRef(A initialValue) : base(initialValue) {
      comparer = defaultComparer;
      // Assign values to this ref when the subscribers get them.
      subscribe(a => _value = a);
    }

    public RxRef(A initialValue, IEqualityComparer<A> comparer) : base(initialValue) 
    { this.comparer = comparer; }

    public IRxRef<B> comap<B>(Fn<A, B> mapper, Fn<B, A> comapper) {
      var bRef = mapImpl(mapper, RxRef.builder(mapper(value)));
      bRef.subscribe(b => value = comapper(b));
      return bRef;
    }
  }

  public class RxMutRef {
    public static IRxMutRef<A> a<A>(A value) { return new RxMutRef<A>(value); }
  }

  public class RxMutRef<A> : RxRefBase<A>, IRxMutRef<A> {
    public RxMutRef(A initialValue) : base(initialValue) {}

    public A change(Act<A> change) {
      change(_value);
      changed();
      return _value;
    }

    public void changed() { submit(_value); }
  }
}
