using System;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Reactive {
  public static class RxValOps {
    public static IRxVal<B> map<A, B>(this IRxVal<A> rx, Fn<A, B> mapper) {
      var lastKnownAVersion = Option<uint>.None;

      Fn<bool> needsUpdate = () => 
        lastKnownAVersion.isEmpty 
        || lastKnownAVersion.__unsafeRawValue != rx.valueVersion;
      Fn<B> getLatestValue = () => {
        // Update on value pull.
        lastKnownAVersion = rx.valueVersion.some();
        return mapper(rx.value);
      };

      var sourceProperties = new RxVal<B>.SourceProperties(needsUpdate, getLatestValue);
      var subscribeFn = ObservableOpImpls.map(
        obs => rx.subscribe(obs, false), 
        (A a) => {
          // Update on value push.
          lastKnownAVersion = rx.valueVersion.some();
          return mapper(a);
        }
      );
      return RxVal.a(sourceProperties, subscribeFn);
    }

    #region #filter

    public static IRxVal<A> filter<A>(this IRxVal<A> rx, Fn<A, bool> predicate, Fn<A> onFiltered) =>
      rx.map(filterMapper(predicate, onFiltered));

    public static IRxVal<A> filter<A>(this IRxVal<A> rx, Fn<A, bool> predicate, A onFiltered) =>
      rx.map(filterMapper(predicate, onFiltered));

    #endregion

    public static IRxVal<B> flatMap<A, B>(this IRxVal<A> rx, Fn<A, IRxVal<B>> originalMapper) {
      var lastKnownAVersion = Option<uint>.None;
      var lastMappedBRx = Option<IRxVal<B>>.None;
      var lastKnownBVersion = default(uint);

      Fn<A, IRxVal<B>> mapper = a => {
        var mapped = originalMapper(a);
        lastMappedBRx = mapped.some();
        lastKnownBVersion = mapped.valueVersion;
        return mapped;
      };

      Fn<bool> needsUpdateA = () =>
        // Didn't request yet
        lastKnownAVersion.isEmpty
        // Source changed since last request
        || rx.valueVersion != lastKnownAVersion.get;
      Fn<bool> needsUpdate = () =>
        needsUpdateA()
        // Source did not change, but perhaps the mapped value changed?
        || lastKnownBVersion != lastMappedBRx.get.valueVersion;
      
      Fn<B> getLatestValue = () => {
        var aNeedsUpdate = needsUpdateA();
        var bRx =
          aNeedsUpdate || lastMappedBRx.isEmpty
          // No value has been requested yet, update it ourselves
          ? mapper(rx.value)
          // Extract the latest RX
          : lastMappedBRx.get;
        // Update on value pull
        lastKnownAVersion = rx.valueVersion.some();
        lastKnownBVersion = bRx.valueVersion;
        return bRx.value;
      };

      var sourceProperties = new RxVal<B>.SourceProperties(needsUpdate, getLatestValue);
      var originalSubscribeFn = ObservableOpImpls.flatMap(
        obs => rx.subscribe(obs, false),
        mapper
      );
      SubscribeToSource<B> subscribeFn = 
        originalObserver => {
          var obs = new Observer<B>(
            b => {
              // Update on value push.
//              lastKnownAVersion = rx.valueVersion.some();
              lastKnownBVersion = lastMappedBRx.get.valueVersion;
              originalObserver.push(b);
            },
            originalObserver.finish
          );
          return originalSubscribeFn(obs);
        };

      return RxVal.a(sourceProperties, subscribeFn);
    }

    #region #zip

    public static IRxVal<Tpl<A, B>> zip<A, B>(this IRxVal<A> rx, IRxVal<B> rx2) => null;
//      RxVal.a(
//        () => F.t(rx.value, rx2.value),
//        ObservableOpImpls.zip(rx, rx2)
//      );

    public static IRxVal<Tpl<A, B, C>> zip<A, B, C>(
      this IRxVal<A> rx, IRxVal<B> rx2, IRxVal<C> rx3
    ) => null;
//      RxVal.a(
//        () => F.t(rx.value, rx2.value, rx3.value),
//        ObservableOpImpls.zip(rx, rx2, rx3)
//      );

    public static IRxVal<Tpl<A, B, C, D>> zip<A, B, C, D>(
      this IRxVal<A> ref1, IRxVal<B> ref2, IRxVal<C> ref3, IRxVal<D> ref4
    ) => null;
//      RxVal.a(
//        () => F.t(ref1.value, ref2.value, ref3.value, ref4.value),
//        ObservableOpImpls.zip(ref1, ref2, ref3, ref4)
//      );

    public static IRxVal<Tpl<A, B, C, D, E>> zip<A, B, C, D, E>(
      this IRxVal<A> ref1, IRxVal<B> ref2, IRxVal<C> ref3, IRxVal<D> ref4, IRxVal<E> ref5
    ) => null;
//      RxVal.a(
//        () => F.t(ref1.value, ref2.value, ref3.value, ref4.value, ref5.value),
//        ObservableOpImpls.zip(ref1, ref2, ref3, ref4, ref5)
//      );

    public static IRxVal<Tpl<A, A1, A2, A3, A4, A5>> zip<A, A1, A2, A3, A4, A5>(
      this IRxVal<A> ref1, IRxVal<A1> ref2, IRxVal<A2> ref3, IRxVal<A3> ref4, IRxVal<A4> ref5,
      IRxVal<A5> ref6
    ) => null;
    //      RxVal.a(
    //        () => F.t(ref1.value, ref2.value, ref3.value, ref4.value, ref5.value, ref6.value),
    //        ObservableOpImpls.zip(ref1, ref2, ref3, ref4, ref5, ref6)
    //      );

    #endregion
      
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
      this IRxVal<Option<A>> source, Fn<A, IRxVal<Option<B>>> extractor
    ) => 
      source.flatMap(aOpt =>
        aOpt.fold(
          () => RxVal.cached(F.none<B>()),
          extractor
        )
      );

    public static IRxVal<Option<B>> optFlatMap<A, B>(
      this IRxVal<Option<A>> source, Fn<A, IRxVal<B>> extractor
    ) =>
      source.optFlatMap(a => extractor(a).map(b => b.some()));

    public static IRxVal<Option<B>> optFlatMap<A, B>(
      this IRxVal<Option<A>> source, Fn<A, Option<IRxVal<Option<B>>>> extractor
    ) => 
      source.flatMap(aOpt =>
        aOpt.flatMap(extractor).getOrElse(RxVal.cached(F.none<B>()))
      );

    public static IRxVal<Option<B>> optMap<A, B>(
      this IRxVal<Option<A>> source, Fn<A, B> mapper
    ) => source.map(aOpt => aOpt.map(mapper));

    public static IRxVal<Option<A>> extract<A>(this Option<IRxVal<A>> rxOpt) => 
      rxOpt.fold(RxVal.cached(F.none<A>()), val => val.map(a => a.some()));

    public static Fn<A, A> filterMapper<A>(Fn<A, bool> predicate, Fn<A> onFiltered) => 
      a => predicate(a) ? a : onFiltered();

    public static Fn<A, A> filterMapper<A>(Fn<A, bool> predicate, A onFiltered) => 
      a => predicate(a) ? a : onFiltered;

    static void subscribeToRescans<A>(
      IEnumerable<IRxVal<A>> vals, Action rescan
    ) {
      var doRescans = false;
      foreach (var rxVal in vals)
        rxVal.subscribe(_ => { if (doRescans) rescan(); });
      doRescans = true;
      rescan();
    }
  }
}