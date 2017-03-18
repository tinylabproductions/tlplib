using System;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Data;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Reactive {
  /**
   * RxVal is an observable which has a current value.
   * 
   * Because it is immutable, the only way for it to change is if its source changes.
   **/
  public interface IRxVal<out A> : VersionedVal<A>, IObservable<A> {}
  
  public class RxVal<A> : RxBase<A>, IRxVal<A> {
    public class SourceProperties {
      public readonly Fn<bool> shouldUpdate;
      public readonly Fn<A> getCurrentValue;

      public SourceProperties(Fn<bool> shouldUpdate, Fn<A> getCurrentValue) {
        this.shouldUpdate = shouldUpdate;
        this.getCurrentValue = getCurrentValue;
      }
    }

    readonly SourceProperties sourceProperties;

    public RxVal(
      SourceProperties sourceProperties, SubscribeFn<A> subscribeFn
    ) : base(subscribeFn) {
      _value = default(A);
      this.sourceProperties = sourceProperties;
    }

    protected override A currentValue { get {
      /* Update current value from source because we have no subscribers, 
       * thus are not subscribed to the source and the value 
       * was not pushed by it to this RxVal. */
      if (subscribers == 0) {
        if (sourceProperties.shouldUpdate())
            _value = sourceProperties.getCurrentValue();
      }
      return _value;
    } }

    public A value => currentValue;

    public override string ToString() => $"{nameof(RxVal)}({value})";
  }

  public static class RxVal {
    #region Constructors

    /* Never changing RxVal. Useful for lifting values into reactive values. */
    public static IRxVal<A> a<A>(A value) => RxValStatic.a(value);
    public static IRxVal<A> cached<A>(A value) => RxValCache<A>.get(value);

    /* RxVal that gets its value from other reactive source where the value is always available. */
    public static IRxVal<A> a<A>(
      RxVal<A>.SourceProperties sourceProperties, SubscribeFn<A> subscribeFn
    ) => 
      new RxVal<A>(sourceProperties, subscribeFn);

    public static IRxVal<B> a<A, B>(
      VersionedVal<A> val, Fn<A, B> mapper, SubscribeFn<B> subscribeFn
    ) {
      var lastVersion = Option<uint>.None;
      var sp = new RxVal<B>.SourceProperties(
        () => lastVersion.isEmpty || val.valueVersion != lastVersion.get,
        () => {
          lastVersion = val.valueVersion.some();
          return mapper(val.value);
        }
      );
      return a(sp, subscribeFn);
    }

//    /* RxVal that gets its value from other reactive source */
//    public static IRxVal<A> a<A>(A initial, SubscribeFn<A> subscribeFn) => 
//      new RxVal<A>(initial, subscribeFn);

    #endregion

    #region Ops

    static void subscribeToRescans<A>(
      IEnumerable<IRxVal<A>> vals, Action rescan
    ) {
      var doRescans = false;
      foreach (var rxVal in vals)
        rxVal.subscribe(_ => { if (doRescans) rescan(); });
      doRescans = true;
      rescan();
    }

    // TODO: test
    /** Convert an enum of rx values into one rx value using a traversal function. **/
    public static IRxVal<B> traverse<A, B>(
      this IEnumerable<IRxVal<A>> vals, Fn<IEnumerable<A>, B> traverse
    ) {
      Fn<IEnumerable<A>> readValues = () => vals.Select(v => v.value);
      var val = RxRef.a(traverse(readValues()));

      // TODO: this is probably suboptimal.
      Action rescan = () => val.value = traverse(readValues());

      subscribeToRescans(vals, rescan);
      return val;
    }

    /* Returns any value that satisfies the predicate. Order is not guaranteed. */
    public static IRxVal<Option<A>> anyThat<A, C>(
      this C vals, Fn<A, bool> predicate
    ) where C : IEnumerable<IRxVal<A>> {
      var val = RxRef.a(F.none<A>());
      var dict = new Dictionary<IRxVal<A>, A>();

      foreach (var rx in vals)
        rx.subscribe(a => {
          var matched = predicate(a);

          if (matched) {
            dict[rx] = a;
            if (val.value.isEmpty) val.value = a.some();
          }
          else {
            dict.Remove(rx);
            if (val.value.isDefined) {
              val.value = 
                dict.isEmpty() 
                  ? Option<A>.None 
                  : dict[dict.Keys.First()].some();
            }
          }
        });
      return val;
    }

    public static IRxVal<Option<A>> anyThat<A>(
      this IEnumerable<IRxVal<A>> vals, Fn<A, bool> predicate
    ) => vals.anyThat<A, IEnumerable<IRxVal<A>>>(predicate);

    public static IRxVal<bool> anyOf<C>(this C vals, bool searchFor=true)
      where C : IEnumerable<IRxVal<bool>> 
    => 
      vals.anyThat<bool, C>(b => searchFor ? b : !b).map(_ => _.isDefined);

    public static IRxVal<Option<A>> anyDefined<A>(
      this IEnumerable<IRxVal<Option<A>>> vals
    ) => 
      vals
      .anyThat<Option<A>, IEnumerable<IRxVal<Option<A>>>>(opt => opt.isDefined)
      .map(_ => _.flatten());

    // TODO: test
    /**
     * Convert IRxVal<A> to IObservable<B>.
     *
     * Useful for converting from RxVal to event source. For example
     * ```someRxVal.map(_ => F.unit)``` would only emit one event, because
     * the backing value would still be IValueObservable.
     *
     * Thus we'd need to use ```someRxVal.toEventSource(_ => F.unit)```.
     **/
    public static IObservable<B> toEventSource<A, B>(
      this IRxVal<A> rxVal, Fn<A, B> mapper
    ) => new Observable<B>(obs => rxVal.subscribe(v => obs.push(mapper(v)), obs.finish));

    public static IObservable<Unit> toEventSource<A>(this IRxVal<A> o) => 
      o.toEventSource(_ => F.unit);

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
    ) => source.map(aOpt => aOpt.map(mapper));

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
    static readonly Dictionary<A, RxValStatic<A>> staticCache = new Dictionary<A, RxValStatic<A>>();

    public static IRxVal<A> get(A value) => 
      staticCache.getOrUpdate(value, () => RxValStatic.a(value));
  }

  /** RxVal which has a constant value. */
  class RxValStatic<A> : IRxVal<A> {
    public A value { get; }

    public uint valueVersion => 0u;
    public int subscribers => 0;
    public bool finished => true;

    public RxValStatic(A value) {
      this.value = value;
    }

    public ISubscription subscribe(IObserver<A> observer) {
      observer.push(value);
      observer.finish();
      return Subscription.empty;
    }
  }
  static class RxValStatic {
    public static RxValStatic<A> a<A>(A a) => new RxValStatic<A>(a);
  }
}