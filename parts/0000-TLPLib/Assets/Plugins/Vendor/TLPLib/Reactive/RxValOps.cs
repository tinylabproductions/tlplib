using System;
using System.Collections.Generic;
using System.Linq;
using com.tinylabproductions.TLPLib.Extensions;
using com.tinylabproductions.TLPLib.Functional;

namespace com.tinylabproductions.TLPLib.Reactive {
  public static class RxValOps {
    public static ISubscription subscribe<A>(
      this IRxVal<A> src, Act<A> onValue, RxSubscriptionMode mode
    ) =>
      src.subscribe(new Observer<A>(onValue), mode);

    public static ISubscription subscribe<A>(
      this IRxVal<A> src, Act<A, ISubscription> onValue, RxSubscriptionMode mode
    ) {
      ISubscription sub = null;
      // ReSharper disable once AccessToModifiedClosure
      sub = src.subscribe(a => onValue(a, sub), mode);
      return sub;
    }

    public static IRxVal<B> map<A, B>(this IRxVal<A> src, Fn<A, B> mapper) =>
      new RxVal<B>(
        mapper(src.value),
        setValue => src.subscribe(
          (a, sub) => { if (!setValue(mapper(a))) sub.unsubscribe(); },
          RxSubscriptionMode.ForRxMapping
        )
      );

    #region #filter

    public static IRxVal<A> filter<A>(this IRxVal<A> rx, Fn<A, bool> predicate, Fn<A> onFiltered) =>
      rx.map(filterMapper(predicate, onFiltered));

    public static IRxVal<A> filter<A>(this IRxVal<A> rx, Fn<A, bool> predicate, A onFiltered) =>
      rx.map(filterMapper(predicate, onFiltered));

    #endregion

    public static IRxVal<B> flatMap<A, B>(this IRxVal<A> src, Fn<A, IRxVal<B>> mapper) {
      var bRx = mapper(src.value);

      return new RxVal<B>(
        bRx.value,
        setValue => {
          var subToBRx = bRx.subscribe(b => setValue(b), RxSubscriptionMode.ForRxMapping);

          var aSub = src.subscribe(
            a => {
              subToBRx.unsubscribe();
              bRx = mapper(a);
              setValue(bRx.value);
              subToBRx = bRx.subscribe(b => setValue(b), RxSubscriptionMode.ForRxMapping);
            },
            RxSubscriptionMode.ForRxMapping
          );
          return aSub.andThen(() => subToBRx.unsubscribe());
        }
      );
    }

    #region #zip

    public static IRxVal<Tpl<A, B>> zip<A, B>(this IRxVal<A> aSrc, IRxVal<B> bSrc) => 
      new RxVal<Tpl<A, B>>(
        F.t(aSrc.value, bSrc.value),
        setValue =>
          aSrc.subscribe(a => setValue(F.t(a, bSrc.value)), RxSubscriptionMode.ForRxMapping)
          .join(bSrc.subscribe(b => setValue(F.t(aSrc.value, b)), RxSubscriptionMode.ForRxMapping))
      );

    public static IRxVal<Tpl<A, B, C>> zip<A, B, C>(
      this IRxVal<A> rx, IRxVal<B> rx2, IRxVal<C> rx3
    ) => rx.zip(rx2).zip(rx3).map(t => t.flatten());

    public static IRxVal<Tpl<A, B, C, D>> zip<A, B, C, D>(
      this IRxVal<A> ref1, IRxVal<B> ref2, IRxVal<C> ref3, IRxVal<D> ref4
    ) => ref1.zip(ref2).zip(ref3).zip(ref4).map(t => t.flatten());

    public static IRxVal<Tpl<A, B, C, D, E>> zip<A, B, C, D, E>(
      this IRxVal<A> ref1, IRxVal<B> ref2, IRxVal<C> ref3, IRxVal<D> ref4, IRxVal<E> ref5
    ) => ref1.zip(ref2).zip(ref3).zip(ref4).zip(ref5).map(t => t.flatten());

    public static IRxVal<Tpl<A, A1, A2, A3, A4, A5>> zip<A, A1, A2, A3, A4, A5>(
      this IRxVal<A> ref1, IRxVal<A1> ref2, IRxVal<A2> ref3, IRxVal<A3> ref4, IRxVal<A4> ref5,
      IRxVal<A5> ref6
    ) => ref1.zip(ref2).zip(ref3).zip(ref4).zip(ref5).zip(ref6).map(t => t.flatten());

    #endregion

    // TODO: test
    /** Convert an enum of rx values into one rx value using a traversal function. **/
    public static IRxVal<B> traverse<A, B>(
      this IEnumerable<IRxVal<A>> valsEnum, Fn<IEnumerable<A>, B> traverseFn
    ) => traverse(valsEnum.ToArray(), traverseFn);

    /** Convert an enum of rx values into one rx value using a traversal function. **/
    public static IRxVal<B> traverse<A, B>(
      this ICollection<IRxVal<A>> vals, Fn<IEnumerable<A>, B> traverseFn
    ) {
      B getValue() => traverseFn(vals.Select(v => v.value));

      return new RxVal<B>(
        getValue(),
        setValue =>
          vals
            .Select(v => v.subscribe(_ => setValue(getValue()), RxSubscriptionMode.ForRxMapping))
            .ToArray() // strict evaluation
            .joinSubscriptions()
      );
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
              val.value = dict.isEmpty() ? Option<A>.None : dict.First().Value.some();
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
    ) => new Observable<B>(obs => rxVal.subscribe(v => obs.push(mapper(v))));

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
  }
}