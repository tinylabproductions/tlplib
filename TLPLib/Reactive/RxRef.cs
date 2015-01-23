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
    IRxVal<A> filter(Fn<A, bool> predicate, Fn<A> onFilter);
    IRxVal<A> filter(Fn<A, bool> predicate, A onFilter);
    IRxVal<Tpl<A, B>> zip<B>(IRxVal<B> ref2);
    IRxVal<Tpl<A, B, C>> zip<B, C>(IRxVal<B> ref2, IRxVal<C> ref3);
    IRxVal<Tpl<A, B, C, D>> zip<B, C, D>(IRxVal<B> ref2, IRxVal<C> ref3, IRxVal<D> ref4);
    IRxVal<Tpl<A, B, C, D, E>> zip<B, C, D, E>(IRxVal<B> ref2, IRxVal<C> ref3, IRxVal<D> ref4, IRxVal<E> ref5);
    IRxVal<Tpl<A, A1, A2, A3, A4, A5>> zip<A1, A2, A3, A4, A5>(
      IRxVal<A1> o1, IRxVal<A2> o2, IRxVal<A3> o3, IRxVal<A4> o4,
      IRxVal<A5> o5
    );
  }

  /**
   * RxRef is a reactive reference, which stores a value and also acts as a IObserver.
   **/
  public interface IRxRef<A> : IRxVal<A> {
    new A value { get; set; }
    /** Returns a new ref that is bound to this ref and vice versa. **/
    IRxRef<B> comap<B>(Fn<A, B> mapper, Fn<B, A> comapper);
    IRxVal<A> asVal { get; }
  }

  public static class RxVal {
    public static ObserverBuilder<Elem, IRxVal<Elem>> builder<Elem>(Elem value) {
      return RxRef.builder(value);
    }

    public static IRxVal<A> a<A>(
      A value, Option<IEqualityComparer<A>> comparer = new Option<IEqualityComparer<A>>()
    ) { return RxRef.a(value, comparer); }

    public static IRxVal<A> a<A>(
      A value, Fn<IObserver<A>, ISubscription> subscribeFn, 
      Option<IEqualityComparer<A>> comparer = new Option<IEqualityComparer<A>>()
    ) {
      return RxRef.a(value, subscribeFn, comparer);
    }

    public static IRxVal<A> cached<A>(A value) { return RxValCache<A>.get(value); }

    public static IRxVal<Option<B>> optFlatMap<A, B>(
      this IRxVal<Option<A>> source, Fn<A, IRxVal<Option<B>>> extractor
    ) {
      return source.flatMap(aOpt =>
        aOpt.map(extractor).getOrElse(cached(F.none<B>()))
      );
    }

    public static IRxVal<Option<B>> optFlatMap<A, B>(
      this IRxVal<Option<A>> source, Fn<A, Option<IRxVal<Option<B>>>> extractor
    ) {
      return source.flatMap(aOpt =>
        aOpt.flatMap(extractor).getOrElse(cached(F.none<B>()))
      );
    }

    public static IRxVal<Option<A>> extract<A>(this Option<IRxVal<A>> rxOpt) {
      return rxOpt.fold(cached(F.none<A>()), val => val.map(a => a.some()));
    }
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
      return subscribeFn => a(value, subscribeFn);
    }

    public static IRxRef<A> a<A>(
      A value, Option<IEqualityComparer<A>> comparer = new Option<IEqualityComparer<A>>()
    ) {
      return new RxRef<A>(value, comparer);
    }

    public static IRxRef<A> a<A>(
      A value, Fn<IObserver<A>, ISubscription> subscribeFn,
      Option<IEqualityComparer<A>> comparer = new Option<IEqualityComparer<A>>()
    ) {
      return new RxRef<A>(value, subscribeFn, comparer);
    }
  }

  public static class RxRefBase {
    public static Fn<A, A> filterMapper<A>(Fn<A, bool> predicate, Fn<A> onFiltered) {
      return a => predicate(a) ? a : onFiltered();
    }

    public static Fn<A, A> filterMapper<A>(Fn<A, bool> predicate, A onFiltered) {
      return a => predicate(a) ? a : onFiltered;
    }
  }

  public abstract class RxRefBase<A> : Observable<A> {
    protected A _value;
    public A value { get { return _value; } }

    protected RxRefBase(A initialValue) : base() { _value = initialValue; }
    protected RxRefBase(
      A initialValue, Fn<IObserver<A>, ISubscription> subscribeFn
    ) : base(subscribeFn) { _value = initialValue; }

    public new IRxVal<B> map<B>(Fn<A, B> mapper) {
      return mapImpl(mapper, RxVal.builder(mapper(value)));
    }

    public IRxVal<B> flatMap<B>(Fn<A, IRxVal<B>> mapper) {
      return flatMapImpl(mapper, RxVal.builder(mapper(value).value));
    }

    public IRxVal<A> filter(Fn<A, bool> predicate, Fn<A> onFiltered) {
      return map(RxRefBase.filterMapper(predicate, onFiltered));
    }

    public IRxVal<A> filter(Fn<A, bool> predicate, A onFiltered) {
      return map(RxRefBase.filterMapper(predicate, onFiltered));
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

    public IRxVal<Tpl<A, A1, A2, A3, A4, A5>> zip<A1, A2, A3, A4, A5>(
      IRxVal<A1> ref2, IRxVal<A2> ref3, IRxVal<A3> ref4, IRxVal<A4> ref5, IRxVal<A5> ref6
    ) { return zipImpl(
      ref2, ref3, ref4, ref5, ref6,
      RxVal.builder(F.t(value, ref2.value, ref3.value, ref4.value, ref5.value, ref6.value))
    ); }
  }

  /**
   * Mutable reference which is also an observable.
   **/
  public class RxRef<A> : RxRefBase<A>, IRxRef<A> {
    private static readonly IEqualityComparer<A> defaultComparer = EqComparer<A>.Default;
    private readonly IEqualityComparer<A> comparer;

    public new A value { 
      get { return _value; }
      set { submit(value); }
    }

    public RxRef(
      A initialValue, Option<IEqualityComparer<A>> comparer=new Option<IEqualityComparer<A>>()
    ) : base(initialValue) {
      this.comparer = comparer.getOrElse(defaultComparer);
    }

    public RxRef(
      A initialValue, Fn<IObserver<A>, ISubscription> subscribeFn,
      Option<IEqualityComparer<A>> comparer = new Option<IEqualityComparer<A>>()
    ) : base(initialValue, subscribeFn) {
      this.comparer = comparer.getOrElse(defaultComparer);
    }

    protected override void submit(A value) {
      if (!comparer.Equals(_value, value)) {
        _value = value;
        base.submit(value);
      }
    }

    public IRxRef<B> comap<B>(Fn<A, B> mapper, Fn<B, A> comapper) {
      var bRef = mapImpl(mapper, RxRef.builder(mapper(value)));
      bRef.subscribe(b => value = comapper(b));
      return bRef;
    }

    public IRxVal<A> asVal { get { return this; } }
  }
}
