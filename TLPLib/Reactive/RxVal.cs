using System;
using System.Collections.Generic;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Reactive {
  /**
   * RxVal is an observable which has a current value.
   * 
   * Because it is immutable, the only way for it to change is if its source changes.
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
  
  public class RxVal<A> : RxBase<A>, IRxVal<A> {
    readonly Option<Fn<A>> getCurrentValue;

    public RxVal(A value) : base() { _value = value; }

    public RxVal(A value, Fn<IObserver<A>, ISubscription> subscribeFn) 
      : base(subscribeFn) { _value = value; }

    public RxVal(
      Fn<A> getCurrentValue, Fn<IObserver<A>, ISubscription> subscribeFn
    ) : base(subscribeFn) {
      _value = getCurrentValue();
      this.getCurrentValue = getCurrentValue.some();
    }

    protected override A currentValue { get {
      /* Update current value from source because we have no subscribers, 
       * thus are not subscribed to the source and the value 
       * was not pushed by it to this RxVal. */
      if (subscribers == 0 && getCurrentValue.isDefined) {
        _value = getCurrentValue.get();
      }
      return _value;
    } }

    public A value { get { return currentValue; } }
  }

  public static class RxVal {
    public static ObserverBuilder<Elem, IRxVal<Elem>> builder<Elem>(
      Fn<Elem> getCurrentValue
    ) {
      return subscribeFn => a(getCurrentValue, subscribeFn);
    }

    #region Constructors

    /* Never changing RxVal. Useful for lifting values into reactive values. */
    public static IRxVal<A> a<A>(A value) { return new RxVal<A>(value); }
    public static IRxVal<A> cached<A>(A value) { return RxValCache<A>.get(value); }
    
    /* RxVal that gets its value from other reactive source where the value is always available. */
    public static IRxVal<A> a<A>(Fn<A> getCurrentValue, Fn<IObserver<A>, ISubscription> subscribeFn) 
    { return new RxVal<A>(getCurrentValue, subscribeFn); }
    
    /* RxVal that gets its value from other reactive source */
    public static IRxVal<A> a<A>(A initial, Fn<IObserver<A>, ISubscription> subscribeFn) 
    { return new RxVal<A>(initial, subscribeFn); }

    #endregion

    #region Ops

    public static IRxVal<Option<B>> optFlatMap<A, B>(
      this IRxVal<Option<A>> source, Fn<A, IRxVal<B>> extractor
    ) {
      return source.flatMap(aOpt =>
        aOpt.map(extractor).map(rxVal => rxVal.map(val => val.some()))
        .getOrElse(cached(F.none<B>()))
      );
    }

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

    public static IRxVal<Option<B>> optMap<A, B>(
      this IRxVal<Option<A>> source, Fn<A, B> mapper
    ) {
      return source.map(aOpt => aOpt.map(mapper));
    }

    public static IRxVal<Option<A>> extract<A>(this Option<IRxVal<A>> rxOpt) {
      return rxOpt.fold(cached(F.none<A>()), val => val.map(a => a.some()));
    }

    public static Fn<A, A> filterMapper<A>(Fn<A, bool> predicate, Fn<A> onFiltered) {
      return a => predicate(a) ? a : onFiltered();
    }

    public static Fn<A, A> filterMapper<A>(Fn<A, bool> predicate, A onFiltered) {
      return a => predicate(a) ? a : onFiltered;
    }

    #endregion
  }

  static class RxValCache<A> {
    static readonly Dictionary<A, IRxVal<A>> staticCache = new Dictionary<A, IRxVal<A>>();

    public static IRxVal<A> get(A value) {
      return staticCache.get(value).getOrElse(() => {
        var cached = (IRxVal<A>)RxRef.a(value);
        staticCache.Add(value, cached);
        return cached;
      });
    }
  }
}
